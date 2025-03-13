using Archi.Runtime;
using Core.Runtime;
using ScriptableEvent.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.UI.Runtime
{
    public class ScoreBoardUI : CBehaviour
    {
        #region Exposed

        [SerializeField] GameEvent _onAllPlayerReady;        

        [SerializeField] private ScoreUI[] _scoreUis;
        [SerializeField] private ScoreUI[] _scoreDuoUis;
        [SerializeField] private Transform _scoreUIParent;
        [SerializeField] private GameObject _soloParent;
        [SerializeField] private GameObject _duoParent;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onAllPlayerReady.UnregisterListener(OnAllPlayerReady);
            ScoreManager.s_instance.m_onPlayerScoreChanged -= UpdateScore;
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onAllPlayerReady.RegisterListener(OnAllPlayerReady);
        }

        private void OnAllPlayerReady()
        {
            ScoreManager.s_instance.m_onPlayerScoreChanged += UpdateScore;

            _isDuoMode = GameManager.s_instance.GetLastTeamModeGame() == Enum.Runtime.TeamModeEnum.Duo;
            _soloParent.SetActive(!_isDuoMode);
            _duoParent.SetActive(_isDuoMode);

            SetupScoreBoard(_isDuoMode ? _scoreDuoUis : _scoreUis, _isDuoMode);
        }

        private void SetupScoreBoard(ScoreUI[] uis, bool isDuoMode)
        {
            Dictionary<int, int> players = ScoreManager.s_instance.GetPlayersScore();

            int teamID1Count = 0;
            int teamID2Count = 0;

            for (int i = 0; i < 4; i++)
            {
                var dictEntry = players.Skip(i).Take(1);

                if (dictEntry == null) continue;

                int key = dictEntry.FirstOrDefault().Key;
                int teamID = ScoreManager.s_instance.GetTeamIDWithPlayerID(key);

                if (isDuoMode)
                {
                    if (teamID == 1) teamID1Count++;
                    else if (teamID == 2) teamID2Count++;

                    if (teamID1Count > 1 && teamID == 1) continue;
                    if (teamID2Count > 1 && teamID == 2) continue;
                }

                bool isAnAI = GameManager.s_instance.GetIfPlayerIsAnAI(key);

                if (isDuoMode)
                {
                    int indexDuoID = teamID == 1 ? 1 : 0;

                    uis[indexDuoID].UpdatePlayer(key, isAnAI, isDuoMode, teamID);
                    uis[indexDuoID].UpdateScore(0);
                }
                else
                {
                    uis[i].UpdatePlayer(key, isAnAI, isDuoMode, teamID);
                    uis[i].UpdateScore(0);
                }

                Material mat = null;
                if (isDuoMode) mat = GameManager.s_instance.GetTeamMaterialWithID(teamID - 1);
                else mat = GameManager.s_instance.GetPlayerMaterialWithUniqueID(key);

                UpdateColorText(i, teamID, mat.color, isDuoMode);
                UpdateScoreVisual(key, 0, i, teamID, isAnAI, isDuoMode, teamID);
            }
        }

        private void UpdateScore(int playerId, int value, int teamID)
        {
            int index = -1;

            if (_isDuoMode)
            {
                for (int i = 0; i < _scoreDuoUis.Length; i++)
                {
                    if (_scoreDuoUis[i].m_teamId == teamID)
                    {
                        index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _scoreUis.Length; i++)
                {
                    if (_scoreUis[i].m_playerId == playerId)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index == -1)
            {
                Debug.LogError("Player/Team not find, impossible to update scoreboard ui");
                return;
            }

            bool isAnAI = GameManager.s_instance.GetIfPlayerIsAnAI(playerId);
            bool isDuoMode = GameManager.s_instance.GetLastTeamModeGame() == Enum.Runtime.TeamModeEnum.Duo;

            UpdateScoreVisual(playerId, value, index, index, isAnAI, isDuoMode, teamID);
        }

        #endregion


        #region UI

        private void UpdateScoreVisual(int playerId, int value, int index, int duoIndex, bool isAnAI, bool isDuoMode, int teamID)
        {
            if (isDuoMode)
            {
                int indexDuoID = duoIndex == 1 ? 1 : 0;
                _scoreDuoUis[indexDuoID].UpdatePlayer(playerId, isAnAI, isDuoMode, teamID);
                _scoreDuoUis[indexDuoID].UpdateScore(value);
            }
            else
            {
                _scoreUis[index].UpdatePlayer(playerId, isAnAI, isDuoMode, teamID);
                _scoreUis[index].UpdateScore(value);
            }
        }

        private void UpdateColorText(int index, int duoIndex, Color color, bool isDuoMode)
        {
            if (isDuoMode)
            {
                int indexDuoID = duoIndex == 1 ? 1 : 0;
                _scoreDuoUis[indexDuoID].UpdateColorText(color);
            }
            else
            {
                _scoreUis[index].UpdateColorText(color);
            }
        }

        #endregion


        #region Private

        private bool _isDuoMode;

        #endregion
    }
}