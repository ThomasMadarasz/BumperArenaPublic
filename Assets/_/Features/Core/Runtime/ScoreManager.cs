using Archi.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Runtime
{
    public class ScoreManager : CNetBehaviour
    {
        #region Exposed

        public static ScoreManager s_instance;

        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private GameEvent _onAllPlayerAsSceneLoaded;

        [SerializeField][Min(1)] private int _victoryRoundCountToWinGame;

        [HideInInspector] public Action<float, bool> m_onTimerValueChanged;
        [HideInInspector] public Action<int, int, int> m_onPlayerScoreChanged;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        public void ResetManager()
        {
            _scorePoints.Clear();
            _canScorePoint = false;
        }

        /// <summary>
        /// Use for score poitn for only one player
        /// </summary>
        /// <param name="team"></param>
        public void ScorePoint(ITeamable team)
        {
            if (!isServer) return;
            if (!_canScorePoint) return;

            if (!_scorePoints.ContainsKey(team))
            {
                Debug.LogError("Team not register");
                return;
            }
            _scorePoints[team].m_roundScore++;
            team.OnScorePoint(GameManager.s_instance.GetCurrentGameMode());

            m_onPlayerScoreChanged?.Invoke(team.GetUniqueID(), _scorePoints[team].m_roundScore, team.GetTeamID());
        }

        /// <summary>
        /// Use for score point for one or multiple player
        /// </summary>
        /// <param name="teamId"></param>
        public void ScorePoint(int teamId)
        {
            IEnumerable<ITeamable> teamables = _scorePoints.Keys.Where(x => x.GetTeamID() == teamId);
            if (teamables == null) return;

            teamables.ForEach(x => ScorePoint(x));
        }


        public void SetupScores(IEnumerable<ITeamable> teams)
        {
            _scorePoints.Clear();

            foreach (ITeamable t in teams)
            {
                _scorePoints.Add(t, new());
            }
        }

        public void UpdateScore(IEnumerable<ITeamable> teams)
        {
            Dictionary<ITeamable, ScorePoint> newScore = new();

            foreach (var kvp in _scorePoints)
            {
                ITeamable team = teams.FirstOrDefault(x => x.GetUniqueID() == kvp.Key.GetUniqueID());
                ScorePoint score = new ScorePoint() { m_roundScore = 0, m_gameScore = kvp.Value.m_gameScore };

                newScore.Add(team, score);
            }

            _scorePoints.Clear();
            _scorePoints = newScore;
        }

        public bool HasARoundWinner()
        {
            int highestScore = _scorePoints.Values.Max(x => x.m_roundScore);
            int teamScoreEquality = _scorePoints.Values.Count(x => x.m_roundScore == highestScore);

            if (teamScoreEquality > 1)
            {
                var winners = _scorePoints.Where(x => x.Value.m_roundScore == highestScore).Select(x => x.Key).ToList();

                int team1 = int.MinValue;
                bool isSameTeam = true;

                foreach (var item in winners)
                {
                    if (team1 == int.MinValue) team1 = item.GetTeamID();
                    else
                    {
                        if (team1 == item.GetTeamID()) continue;
                        else isSameTeam = false;
                    }
                }

                return isSameTeam;
            }

            return teamScoreEquality == 1;
        }

        public int GetWinnerRoundTeamID()
        {
            int highestScore = _scorePoints.Values.Max(x => x.m_roundScore);

            return _scorePoints.FirstOrDefault(x => x.Value.m_roundScore == highestScore).Key.GetTeamID();
        }

        public int GetWinnerOfLastRound() => _roundWinnerTeamID;

        public bool HasAGameWinner()
        {
            return _scorePoints.Values.Count(x => x.m_gameScore == _victoryRoundCountToWinGame) > 0;
        }

        public int GetWinnerGameTeamID()
        {
            return _scorePoints.FirstOrDefault(x => x.Value.m_gameScore == _victoryRoundCountToWinGame).Key.GetTeamID();
        }

        public bool APlayerWillSoonWinTheGame()
        {
            return _scorePoints.Count(x => x.Value.m_gameScore == _victoryRoundCountToWinGame - 1) > 0;
        }

        private void Setup()
        {
            s_instance = this;

            _onAllPlayerAsSceneLoaded.RegisterListener(OnAllPlayerReady);
            _onRoundStart.RegisterListener(OnRoundStart);
            _onRoundFinished.RegisterListener(OnRoundFinished);
        }

        public int GetTeamableScore(ITeamable teamable) => _scorePoints.FirstOrDefault(x => x.Key == teamable).Value.m_roundScore;

        #endregion


        #region Utils & Tools

        public Dictionary<int, int> GetPlayersScore()
        {
            Dictionary<int, int> dict = new();

            foreach (var kvp in _scorePoints)
            {
                dict.Add(kvp.Key.GetUniqueID(), kvp.Value.m_gameScore);
            }

            return dict;
        }

        private void OnAllPlayerReady()
        {

        }

        public int GetTeamIDWithPlayerID(int id) => _scorePoints.Keys.FirstOrDefault(x => x.GetUniqueID() == id).GetTeamID();

        public void OnRoundTimerValueChanged(float time, bool isOvertime)
        {
            int roundedTime = Mathf.RoundToInt(time);

            if (_remainingTime == roundedTime) return;
            _remainingTime = roundedTime;

            m_onTimerValueChanged?.Invoke(time, isOvertime);
        }

        private void OnRoundStart() => _canScorePoint = true;
        private void OnRoundFinished()
        {
            _canScorePoint = false;
            int teamId = GetWinnerRoundTeamID();
            _roundWinnerTeamID = teamId;

            foreach (var kvp in _scorePoints)
            {
                if (kvp.Key.GetTeamID() == teamId) kvp.Value.m_gameScore += 1;
                kvp.Value.m_roundScore = 0;
            }
        }

        #endregion


        #region Private

        private Dictionary<ITeamable, ScorePoint> _scorePoints = new();

        private bool _canScorePoint;

        private int _remainingTime;
        private int _roundWinnerTeamID;

        #endregion
    }
}