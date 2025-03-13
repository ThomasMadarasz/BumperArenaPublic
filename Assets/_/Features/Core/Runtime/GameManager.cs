using Archi.Runtime;
using Audio.Runtime;
using Data.Runtime;
using Enum.Runtime;
using Interfaces.Runtime;
using Mirror;
using Progression.Runtime;
using SceneManager.runtime;
using ScriptableEvent.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Runtime;

namespace Core.Runtime
{
    public class GameManager : CNetBehaviour
    {
        #region Exposed

        public static GameManager s_instance;

        [SerializeField] private GameEvent _onGameSceneReady;
        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private GameEvent _onStartGame;
        [SerializeField] private GameEvent _onStartSpawnPlayers;
        [SerializeField] private GameEvent _onStartSpline;
        [SerializeField] private GameEventT _onLoadSceneRequired;
        [SerializeField] private GameEventT _onRemainingGameTimeChanged;

        [SerializeField] private GameEventT _onStartMusic;
        [SerializeField] private GameEvent _onStopMusic;

        [SerializeField] private CustomisationData _customisationData;

        [SerializeField] private int _roundCountdownTime;
        [SerializeField] private int _roundDuration;

        [SerializeField] private string _roundResultSceneName;

        [SerializeField] private float _timeBeforeRoundResultScene;

        [SerializeField] private ProgressionDataValues _progressionData;

        [SerializeField] private SFXData _endOfRoundSoonSfx;
        [SerializeField] private SFXData _countdownSfx;
        [SerializeField] private SFXData _endGameSfx;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update()
        {
            if (!_inOvertime) return;
            OvertimeUpdate();
        }

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            _roundTimer = new(_roundDuration, OnRoundTimerOver);
            _roundTimer.OnValueChanged += OnRoundTimerValueChanged;

            _onStartGame.RegisterListener(StartRoundCountdown);
            _onRoundFinished.RegisterListener(OnRoundFinished);
            _onRoundStart.RegisterListener(OnRoundStart);
        }

        public void ResetManager()
        {
            _roundTimer.Stop();

            _playerMaterials.Clear();
            _playerEmissveMaterials.Clear();
            _scoreboardEmissiveMaterials.Clear();
            _forcedPlayerMaterials.Clear();
            _forcedPlayerEmissiveMaterials.Clear();
            _playerId_AI.Clear();

            _overtime = 0;
            _inOvertime = false;
            _playEndOfRoundSound = false;
        }

        public void SetupManagers(IEnumerable<GameObject> players, TeamModeEnum teamMode)
        {
            var teams = players.Select(x => x.GetComponent<ITeamable>()).ToList();
            var kill = players.Select(x => x.GetComponent<IKillable>());
            var graphics = players.Select(x => x.GetComponent<IPlayerGraphic>()).ToList();
            var pl = players.Select(x => x.GetComponent<IPlayer>()).ToList();

            teams.ForEach(x => x.SetUniqueID(teams.ToList().IndexOf(x)));

            int index = 0;
            foreach (var player in players)
            {
                int id = player.GetComponent<ITeamable>().GetUniqueID();

                IPlayerGraphic graphic = player.GetComponent<IPlayerGraphic>();
                Material mat = graphic.GetMaterial();
                Material emissiveMat = graphic.GetEmissiveMaterial();
                Material scoreboardeMat = graphic.GetScoreboardMaterial();

                _playerId_AI.Add(id, pl[index].IsAnAI());

                AddPlayerMaterials(id, mat);
                AddPlayerEmissveMaterials(id, emissiveMat);
                AddScoreboardEmissveMaterials(id, scoreboardeMat);
                index++;
            }

            //TeamManager.s_instance.SetupTeams(teams, teamMode);
            ScoreManager.s_instance.SetupScores(teams);

            //UpdatePlayerMaterialsOnNewRound(teamMode, teams, graphics);

            _onGameSceneReady.Raise();
        }

        public void SetupManagersOnNewRound(IEnumerable<GameObject> players, TeamModeEnum teamMode)
        {
            _playersUniquesIds.Clear();
            _lastTeamModeGame = teamMode;

            var teams = players.Select(x => x.GetComponent<ITeamable>()).ToList();
            var kill = players.Select(x => x.GetComponent<IKillable>());
            var graphics = players.Select(x => x.GetComponent<IPlayerGraphic>()).ToList();

            TeamManager.s_instance.SetupTeams(teams, teamMode);

            UpdatePlayerMaterialsOnNewRound(teamMode, teams, graphics);

            _playersUniquesIds = teams.Select(x => x.GetUniqueID()).ToList();

            ScoreManager.s_instance.UpdateScore(teams);
            SpawnManager.s_instance.SpawnPlayers(kill, teamMode);
        }

        private IEnumerator RoundCountdown()
        {
            AudioManager.s_instance.PlaySfx(_countdownSfx._sfx.GetRandom(), false);
            _onStartSpawnPlayers?.Raise();

            for (int i = _roundCountdownTime; i > 0; i--)
            {
                ScoreManager.s_instance.OnRoundTimerValueChanged(i, false);
                yield return new WaitForSeconds(1);
            }

            _onRoundStart.Raise();
            string sceneName = SceneLoader.s_instance.GetCurrentActiveScene();
            _onStartMusic.Raise(sceneName);
        }

        private void OvertimeUpdate()
        {
            _overtime += Time.deltaTime;

            ScoreManager.s_instance.OnRoundTimerValueChanged(_overtime, true);

            object[] objs = new object[2] { _overtime, true };
            _onRemainingGameTimeChanged?.Raise(objs);

            if (!HasARoundWinner()) return;

            _inOvertime = false;
            _onRoundFinished.Raise();
            _onStopMusic.Raise();
        }

        #endregion


        #region Utils & Tools

        public void SetCurrentGameModeID(int id) => _currentGameModeID = id;

        public int GetCurrentGameMode() => _currentGameModeID;

        private void UpdatePlayerMaterialsOnNewRound(TeamModeEnum teamMode, List<ITeamable> teams, List<IPlayerGraphic> graphics)
        {
            _forcedPlayerMaterials.Clear();
            _forcedPlayerMaterialIds.Clear();
            _forcedPlayerEmissiveMaterials.Clear();

            if (teamMode == TeamModeEnum.Duo)
            {
                for (int i = 0; i < teams.Count; i++)
                {
                    int matId = teams[i].GetTeamID() - 1;

                    Material mat = _customisationData.m_teamModeDuoMaterials[matId];
                    Material emissiveMat = _customisationData.m_teamModeDuoEmissiveMaterials[matId];

                    _forcedPlayerMaterials.Add(teams[i].GetUniqueID(), mat);
                    _forcedPlayerEmissiveMaterials.Add(teams[i].GetUniqueID(), emissiveMat);

                    graphics[i].SwitchHighToLowPolyMesh();
                    graphics[i].SetUpInGameAnimator();
                    graphics[i].ForceMaterial(matId, true);
                }
            }
            else
            {
                for (int i = 0; i < teams.Count; i++)
                {
                    int id = teams[i].GetUniqueID();
                    int matId = graphics[i].GetMaterialIndex();

                    Material mat = _customisationData.m_materials[matId];
                    Material emissiveMat = _customisationData.m_emissiveMaterials[matId];

                    _forcedPlayerMaterials.Add(id, mat);
                    _forcedPlayerMaterialIds.Add(id, matId);
                    _forcedPlayerEmissiveMaterials.Add(id, emissiveMat);

                    graphics[i].SwitchHighToLowPolyMesh();
                    graphics[i].SetUpInGameAnimator();
                    graphics[i].ForceMaterial(id, false);
                }
            }
        }

        private void StartRoundCountdown()
        {
            StartCoroutine(nameof(RoundCountdown));
        }

        private void OnRoundStart()
        {
            _roundTimer.Start();
        }

        private void OnRoundFinished()
        {
            StartCoroutine(LaunchRoundResultScene(_timeBeforeRoundResultScene));
        }

        private void OnRoundTimerOver()
        {
            if (_currentGameModeID == 2 || _currentGameModeID == 4) return;
            bool hasAWinner = HasARoundWinner();

            if (hasAWinner) FinishRound();
            else
                _inOvertime = true;
        }

        private void OnRoundTimerValueChanged(float time)
        {
            ScoreManager.s_instance.OnRoundTimerValueChanged(time, false);

            object[] objs = new object[2] { time, false };
            _onRemainingGameTimeChanged?.Raise(objs);

            if (_currentGameModeID == 4) return;
            if (time <= 10f && !_playEndOfRoundSound)
            {
                _playEndOfRoundSound = true;
                AudioManager.s_instance.PlaySfx(_endOfRoundSoonSfx._sfx.GetRandom(), false);
            }
        }

        private bool HasARoundWinner() => ScoreManager.s_instance.HasARoundWinner();

        private IEnumerator LaunchRoundResultScene(float waitingTime)
        {
            yield return new WaitForSeconds(waitingTime);

            SceneData data = new()
            {
                m_sceneName = _roundResultSceneName,
                m_teamMode = Enum.Runtime.TeamModeEnum.Unknown
            };

            //ProgresionManager.s_instance.GainCoin(_progressionData.m_coinGainedOnRoundFinished);
            _onLoadSceneRequired.Raise(data);
        }

        public void FinishRound()
        {
            AudioManager.s_instance.PlaySfx(_endGameSfx._sfx.GetRandom(), false);
            _onRoundFinished.Raise();
            _onStopMusic.Raise();
            if (_roundTimer != null) _roundTimer.Stop();
        }

        public Material GetPlayerMaterialWithUniqueID(int index)
        {
            return _playerMaterials[index];
        }

        public Material GetTeamMaterialWithID(int index)
        {
            return _customisationData.m_teamModeDuoMaterials[index];
        }

        public Material GetScoreboardEmissiveMaterialWithPlayerUniqueID(int index)
        {
            return _scoreboardEmissiveMaterials[index];
        }

        public Material GetPlayerEmissiveMaterialWithUniqueID(int index)
        {
            return _playerEmissveMaterials[index];
        }

        public Material GetForcedPlayerMaterialWithUniqueID(int index)
        {
            return _forcedPlayerMaterials[index];
        }

        public Material GetForcedPlayerEmissiveMaterialWithUniqueID(int index)
        {
            return _forcedPlayerEmissiveMaterials[index];
        }

        public int GetForcedPlayerMateriaIDlWithUniqueID(int index)
        {
            return _forcedPlayerMaterialIds[index];
        }

        private void AddPlayerMaterials(int playerId, Material mat)
        {
            _playerMaterials.Add(playerId, mat);
        }

        private void AddPlayerEmissveMaterials(int playerId, Material mat)
        {
            _playerEmissveMaterials.Add(playerId, mat);
        }

        private void AddScoreboardEmissveMaterials(int playerId, Material mat)
        {
            _scoreboardEmissiveMaterials.Add(playerId, mat);
        }

        public List<int> GetPlayersUniqueID()
        {
            return _playersUniquesIds;
        }

        public bool GetIfPlayerIsAnAI(int uniqueId)
        {
            return _playerId_AI[uniqueId];
        }

        public TeamModeEnum GetLastTeamModeGame() => _lastTeamModeGame;

        #endregion


        #region Private

        private Timer _roundTimer;

        private bool _inOvertime;
        private bool _playEndOfRoundSound;

        private float _overtime;

        private int _currentGameModeID;

        private TeamModeEnum _lastTeamModeGame;

        private Dictionary<int, Material> _playerMaterials = new();
        private Dictionary<int, Material> _playerEmissveMaterials = new();
        private Dictionary<int, Material> _scoreboardEmissiveMaterials = new();
        private Dictionary<int, Material> _forcedPlayerMaterials = new();
        private Dictionary<int, Material> _forcedPlayerEmissiveMaterials = new();
        private Dictionary<int, int> _forcedPlayerMaterialIds = new();
        private Dictionary<int, bool> _playerId_AI = new();
        private List<int> _playersUniquesIds = new();
        private List<Color> _playerForcedColors = new();

        #endregion
    }
}