using Archi.Runtime;
using Core.Data.Runtime;
using Enum.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Runtime;

namespace Core.Runtime
{
    public class SpawnManager : CNetBehaviour
    {
        #region Exposed

        public static SpawnManager s_instance;

        [SerializeField] private GameEventT2 _onPlayerDie;
        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private GameEvent _onStartSpawnPlayers;
        [SerializeField] private GameEventT _disablePlayerGaphics;

        [SerializeField] private RespawnData _respawnData;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        public void SpawnPlayers(IEnumerable<IKillable> players, TeamModeEnum teamMode)
        {
            _playerRespawns.Clear();
            _currentTeamMode = teamMode;

            foreach (IKillable p in players)
            {
                if (_playerRespawns.ContainsKey(p))
                {
                    Debug.LogError("Player already register");
                    return;
                }

                _playerRespawns.Add(p, new(_respawnData.m_timeBeforeStartingRespawn, OnRespawnTimerOver, p));
            }

            _spawnPoints.Clear();
            _spawnPoints = SceneInformations.s_instance.m_spawnPoints.ToList();
            _spawnPoints.ForEach(x => x.m_isAvailable = true);

            foreach (var player in _playerRespawns)
            {
                SpawnPlayer(player.Key);
            }
        }

        private void Setup()
        {
            s_instance = this;

            _onPlayerDie.RegisterListener(OnPlayerDie);
            _onRoundFinished.RegisterListener(DisableRespawns);
            _onStartSpawnPlayers.RegisterListener(StartingSpawnPlayers);
        }

        #endregion


        #region Utils & Tools

        private void StartingSpawnPlayers()
        {
            _spawnPoints.ForEach(x => x.StartSpawnPlayer());
        }

        private void DisableRespawns()
        {
            _playerRespawns.ForEach(x => x.Value.Stop());
            _spawnPoints.ForEach(x => x.StopSpawnPlayer());
        }

        private void OnPlayerDie(object obj, object obj2)
        {
            object[] args = (object[])obj;
            IKillable player = args[0] as IKillable;
            if (player == null)
            {
                Debug.LogError("Player is null");
                return;
            }

            _playerRespawns[player].Start();
        }

        private void OnRespawnTimerOver(IKillable player)
        {
            int teamID = player.GetTeamable().GetTeamID();
            GetAvailableSpawnPoint(teamID).RespawnPlayer(player);
        }

        private void SpawnPlayer(IKillable player)
        {
            int teamID = player.GetTeamable().GetTeamID();
            GetAvailableSpawnPoint(teamID).SpawnPlayer(player);
        }

        private SpawnPoint GetAvailableSpawnPoint(int playerTeamID)
        {
            SpawnPoint point = null;

            if (_currentTeamMode == TeamModeEnum.Duo)
            {
                point = _spawnPoints.Where(x => x.m_isAvailable && x.m_teamID == playerTeamID).GetRandom();
            }
            else
            {
                point = _spawnPoints.Where(x => x.m_isAvailable).GetRandom();
            }

            int index = _spawnPoints.IndexOf(point);
            _spawnPoints[index].m_isAvailable = false;

            return point;
        }

        #endregion


        #region Private

        private Dictionary<IKillable, TimerT<IKillable>> _playerRespawns = new();
        private List<SpawnPoint> _spawnPoints = new();

        private TeamModeEnum _currentTeamMode;

        #endregion
    }
}