using Archi.Runtime;
using Core.Runtime;
using Data.Runtime;
using Enum.Runtime;
using ScriptableEvent.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Runtime;

namespace Voting.Runtime
{
    public class VotingManager : CBehaviour
    {
        #region Exposed

        public static VotingManager s_instance;

        [SerializeField] private float _maxVotingTime;
        [SerializeField] private float _timeWhenAllPlayerIsReady;

        [SerializeField] private string _forceMapNameToLoad;
        [SerializeField] private TeamModeEnum _forceTeamMode;

        [SerializeField] private ModeData _modeData;

        [SerializeField] private GameEventT _onLoadSceneRequired;
        [SerializeField] private GameEvent _onMapVoteStart;

        [HideInInspector] public Action<float> m_onTimerValueChanged;
        [HideInInspector] public Action<ModeInfos[], ModeData> m_onSelectionFinished;
        [HideInInspector] public Action m_onTimerFinished;

        [SerializeField] private CustomisationData _customisationData;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        public void ResetManager()
        {
            _isFirstVoting = true;
            _lastPlayedModeIndex = null;
            _excludedMapByMode.Clear();
        }

        private void Setup()
        {
            s_instance = this;

            _onMapVoteStart.RegisterListener(NewVoteRoundStart);
        }

        private void NewVoteRoundStart()
        {

        }

        public void PickupRandomMode(TeamModeEnum mode, int playerCount, Dictionary<int, int> playerIndexAndLocalIndex, List<int> materialsIndex)
        {
            _playerCount = playerCount;
            _voteCount = new int[4] { 0, 0, 0, 0 };

            _playerIndexAndLocalIndex = playerIndexAndLocalIndex;
            _localPlayerColor.Clear();

            int i = 0;
            foreach (var kvp in playerIndexAndLocalIndex.Where(x => x.Key != -1))
            {
                Material mat = _customisationData.m_materials[materialsIndex[i]];
                _localPlayerColor.Add(kvp.Value, mat.color);
                i++;
            }

            if (_isFirstVoting)
            {
                _isFirstVoting = false;
                _selectedGameModes = GetFirstRandomModes();
            }
            else _selectedGameModes = GetRandomModes(mode, _lastPlayedModeIndex);

            foreach (var item in _selectedGameModes)
            {
                item.m_mapsLogicIndex = GetRandomMaps(item);
            }

            m_onSelectionFinished?.Invoke(_selectedGameModes, _modeData);

            _timer = new(_maxVotingTime, OnTimerOver);
            _timer.OnValueChanged += OnTimerValueChanged;
            _timer.Start();
        }

        public int GetPlayerCount() => _playerCount;

        public void AllPlayerIsReady()
        {
            if (_onReady) return;
            _onReady = true;
            _timer.Stop();

            _timer = new(_timeWhenAllPlayerIsReady, OnTimerOver);
            _timer.OnValueChanged += OnTimerValueChanged;

            _timer.Start();
        }

        private MapData GetMostVotedMap()
        {
            MapData map = null;

            int maxValue = _voteCount.Max(x => x);
            bool equality = _voteCount.Count(x => x == maxValue) > 1;

            if (equality)
            {
                int random = Utility.m_rng.Next(0, 2);
                int skipped = 0;

                int mapIndex = 0;

                for (int i = 0; i < _voteCount.Length; i++)
                {
                    if (_voteCount[i] != maxValue) continue;

                    if (random == skipped)
                    {
                        mapIndex = i;
                        break;
                    }
                    else skipped++;
                }

                int modeIndex = GetModeIndex(mapIndex);
                mapIndex = GetMapIndex(modeIndex, mapIndex);

                map = GetMapFromIndex(modeIndex, mapIndex);
            }
            else
            {
                int mapIndex = _voteCount.ToList().IndexOf(maxValue);
                int modeIndex = GetModeIndex(mapIndex);
                mapIndex = GetMapIndex(modeIndex, mapIndex);

                map = GetMapFromIndex(modeIndex, mapIndex);
            }

            return map;
        }

        private TeamModeEnum GetTeamModeForMap(MapData map)
        {
            return _modeData.m_gameModesData.FirstOrDefault(x => x.m_maps.Count(y => y.m_logicName == map.m_logicName) > 0).m_teamMode;
        }

        private GameModeData GetSelectedMode(MapData map)
        {
            return _modeData.m_gameModesData.FirstOrDefault(x => x.m_maps.Count(y => y.m_logicName == map.m_logicName) > 0);
        }

        public void ForceTimerValue(float value)
        {
            _timer.Stop();
            _timer = null;

            _timer = new(value, OnTimerOver);
            _timer.OnValueChanged += OnTimerValueChanged;
            _timer.Start();
        }

        public ModeData GetModeData() => _modeData;

        #endregion


        #region Utils & Tools

        private int GetModeIndex(int mapIndex)
        {
            if (mapIndex == 1 || mapIndex == 3) return 1;
            else return 0;
        }

        private int GetMapIndex(int modeIndex, int mapIndex)
        {
            if (modeIndex == 0)
            {
                return mapIndex == 0 ? 0 : 1;
            }
            else
            {
                return mapIndex == 1 ? 0 : 1;
            }
        }

        private MapData GetMapFromIndex(int modeIndex, int mapIndex)
        {
            int logicModeIndex = _selectedGameModes[modeIndex].m_modeLogicIndex;
            int logicMapIndex = _selectedGameModes[modeIndex].m_mapsLogicIndex[mapIndex];

            return _modeData.m_gameModesData
                .FirstOrDefault(x => x.m_logicIndex == logicModeIndex).m_maps
                .FirstOrDefault(x => x.m_logicIndex == logicMapIndex);
        }

        private void OnTimerOver()
        {
            m_onTimerFinished?.Invoke();
            MapData map = GetMostVotedMap();
            TeamModeEnum mode = GetTeamModeForMap(map);

            _lastPlayedModeIndex = GetSelectedMode(map).m_logicIndex;

            SceneData data = new SceneData()
            {
                m_sceneName = map.m_logicName,
                m_teamMode = mode
            };

            if (_excludedMapByMode.ContainsKey((int)_lastPlayedModeIndex))
                _excludedMapByMode[(int)_lastPlayedModeIndex].Add(map.m_logicIndex);
            else
                _excludedMapByMode.Add((int)_lastPlayedModeIndex, new List<int>() { map.m_logicIndex });

            if (!string.IsNullOrWhiteSpace(_forceMapNameToLoad))
            {
                data.m_sceneName = _forceMapNameToLoad;
                data.m_teamMode = _forceTeamMode;
            }

            _onLoadSceneRequired.Raise(data);
        }

        private void OnTimerValueChanged(float time)
        {
            m_onTimerValueChanged?.Invoke(time);
        }

        private ModeInfos[] GetFirstRandomModes()
        {
            ModeInfos[] infos = new ModeInfos[2];

            List<GameModeData> availableMode = _modeData.m_gameModesData.ToList();
            List<GameModeData> duoModes = availableMode.Where(x => x.m_teamMode == TeamModeEnum.Duo).ToList();

            int index = Utility.m_rng.Next(0, duoModes.Count);
            infos[0] = new ModeInfos() { m_modeLogicIndex = duoModes[index].m_logicIndex };

            duoModes.Remove(duoModes[index]);

            index = Utility.m_rng.Next(0, duoModes.Count);
            infos[1] = new ModeInfos() { m_modeLogicIndex = duoModes[index].m_logicIndex };

            return infos;
        }

        private ModeInfos[] GetRandomModes(TeamModeEnum mode, int? modeToExcludeIndex)
        {
            ModeInfos[] infos = new ModeInfos[2];
            List<GameModeData> availableMode = _modeData.m_gameModesData.ToList();

            if (modeToExcludeIndex != null)
            {
                availableMode.Remove(availableMode.FirstOrDefault(x => x.m_logicIndex == modeToExcludeIndex));
            }

            if (mode != TeamModeEnum.Unknown)
                availableMode = _modeData.m_gameModesData.Where(x => x.m_teamMode == mode).ToList();

            int index = Utility.m_rng.Next(0, availableMode.Count);
            infos[0] = new ModeInfos() { m_modeLogicIndex = availableMode[index].m_logicIndex };

            availableMode.Remove(availableMode[index]);

            index = Utility.m_rng.Next(0, availableMode.Count);
            infos[1] = new ModeInfos() { m_modeLogicIndex = availableMode[index].m_logicIndex };

            return infos;
        }

        private int[] GetRandomMaps(ModeInfos info)
        {
            int[] maps = new int[2];

            List<MapData> availableMaps = _modeData.m_gameModesData.FirstOrDefault(x => x.m_logicIndex == info.m_modeLogicIndex).m_maps.ToList();

            if (_excludedMapByMode.ContainsKey(info.m_modeLogicIndex))
            {
                var excludedMaps = _excludedMapByMode[info.m_modeLogicIndex];
                List<MapData> availableMapsWithExcluded = availableMaps.Where(x => !excludedMaps.Contains(x.m_logicIndex)).ToList();

                if (availableMapsWithExcluded.Count < 2)
                {
                    int lastPlayedMap = excludedMaps.Last();
                    _excludedMapByMode[info.m_modeLogicIndex] = new List<int>() { lastPlayedMap };
                    availableMaps = availableMaps.Where(x => x.m_logicIndex != lastPlayedMap).ToList();
                }
                else availableMaps = availableMapsWithExcluded;
            }

            int index = Utility.m_rng.Next(0, availableMaps.Count);
            maps[0] = availableMaps[index].m_logicIndex;

            availableMaps.Remove(availableMaps[index]);

            index = Utility.m_rng.Next(0, availableMaps.Count);
            maps[1] = availableMaps[index].m_logicIndex;

            return maps;
        }

        public Dictionary<int, int> GetPlayersIndex() => _playerIndexAndLocalIndex;

        public Color GetLocalPlayersColors(int playerID)
        {
            return _localPlayerColor[playerID];
        }

        public void UpdateVoteCount(int index, int count) => _voteCount[index] = count;

        #endregion


        #region Private

        private Timer _timer;
        private int _playerCount;

        private bool _onReady;
        private bool _isFirstVoting = true;

        private ModeInfos[] _selectedGameModes;
        private int? _lastPlayedModeIndex;

        private int[] _voteCount;

        private Dictionary<int, int> _playerIndexAndLocalIndex;
        private Dictionary<int, Color> _localPlayerColor = new();
        private Dictionary<int, List<int>> _excludedMapByMode = new();

        #endregion
    }
}