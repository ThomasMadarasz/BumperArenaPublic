using Archi.Runtime;
using Interfaces.Runtime;
using Mirror;
using RoundResult.Runtime;
using ScriptableEvent.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Runtime
{
    public class EndRoundManager : CNetBehaviour
    {
        #region Exposed

        public static EndRoundManager s_instance;

        [SerializeField] private string _mapvoteSceneName;

        [SerializeField] private GameEvent _onRoundResultStarted;
        [SerializeField] private GameEventT _onLoadSceneRequired;
        [SerializeField] private GameEventT _onRoundResultSceneFinished;
        [SerializeField] private GameEvent _onSetupRoundResultScene;
        [SerializeField] private GameEvent _backToMainMenu;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            _onRoundResultStarted.RegisterListener(StartRoundResult);
            _onRoundResultSceneFinished.RegisterListener(OnRoundResultSceneFinished);
            _onSetupRoundResultScene.RegisterListener(SetupRoundResultScene);
        }

        [ServerCallback]
        private void StartRoundResult()
        {
            if (!isServer) return;
        }

        [ServerCallback]
        private void OnRoundResultSceneFinished(object obj)
        {
            bool goToMainMenu = (bool)obj;

            if (goToMainMenu) GotToMainMenuScene();
            else LaunchMapVoteScene();
        }

        [ServerCallback]
        private void LaunchMapVoteScene()
        {
            // Unknown = Solo or Duo

            Enum.Runtime.TeamModeEnum teamMode = Enum.Runtime.TeamModeEnum.Unknown;

            if (ScoreManager.s_instance.APlayerWillSoonWinTheGame())
            {
                //Force gamemode to Solo
                teamMode = Enum.Runtime.TeamModeEnum.Solo;
                Debug.Log("Game mode force to Solo");
            }

            SceneData data = new SceneData()
            {
                m_sceneName = _mapvoteSceneName,
                m_teamMode = teamMode
            };

            _onLoadSceneRequired.Raise(data);
        }

        [ServerCallback]
        public void LoadResult(List<GameObject> players)
        {
            _players = players;
        }

        #endregion


        #region Utils & Tools

        [ServerCallback]
        private void GotToMainMenuScene()
        {
            _backToMainMenu.Raise();
        }

        [ServerCallback]
        private void SetupRoundResultScene()
        {
            ScoreManager sc = ScoreManager.s_instance;

            bool hasAGameWinner = sc.HasAGameWinner();
            int winnerTeamId = hasAGameWinner ? sc.GetWinnerGameTeamID() : sc.GetWinnerOfLastRound();

            bool isDuo = GameManager.s_instance.GetLastTeamModeGame() == Enum.Runtime.TeamModeEnum.Duo;

            List<ITeamable> teams = new();
            List<Color> colors = new();
            List<Color> emissiveColors = new();
            List<Color> scoreboardColors = new();

            foreach (var p in _players)
            {
                ITeamable team = p.GetComponent<ITeamable>();
                int uniqueId = team.GetUniqueID();
                Material mat = GameManager.s_instance.GetPlayerMaterialWithUniqueID(uniqueId);
                Material emissiveMat = GameManager.s_instance.GetPlayerEmissiveMaterialWithUniqueID(uniqueId);
                Material scoreboardMat = GameManager.s_instance.GetScoreboardEmissiveMaterialWithPlayerUniqueID(uniqueId);

                teams.Add(team);
                colors.Add(mat.color);
                emissiveColors.Add(emissiveMat.GetColor("_EmissionColor"));
                scoreboardColors.Add(scoreboardMat.GetColor("_PlayerColor"));
            }

            Dictionary<int, int> scores = ScoreManager.s_instance.GetPlayersScore();

            RoundResultManager.s_instance.SetupManager(_players, hasAGameWinner, winnerTeamId, isDuo, teams, colors, emissiveColors, scoreboardColors, scores);
        }

        #endregion


        #region Private

        private List<GameObject> _players;

        #endregion
    }
}