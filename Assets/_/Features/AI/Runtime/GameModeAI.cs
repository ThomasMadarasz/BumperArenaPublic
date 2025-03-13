using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces.Runtime;
using GameModes.Runtime;
using Mirror;
using ScriptableEvent.Runtime;

namespace AI.Runtime
{
    [RequireComponent(typeof(PlayerAI))]
    public abstract class GameModeAI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onGameSceneStarted;
        [SerializeField] private GameEvent _onRoundFinished;

        public bool m_isRoundStarted;

        #endregion


        #region Unity API

        [ServerCallback]
        private void Awake() => Setup();

        private void OnEnable() => m_isRoundStarted = false;

        private void OnDestroy()
        {
            _onGameSceneStarted.UnregisterListener(SetupRound);
            _onRoundFinished.UnregisterListener(SetSetupToFalse);
        }

        #endregion


        #region Main

        protected virtual void Setup()
        {
            _playerAi = GetComponent<PlayerAI>();
            _onGameSceneStarted.RegisterListener(SetupRound);
            _onRoundFinished.RegisterListener(SetSetupToFalse);
        }

        protected virtual void SetupRound() 
        {
            m_isRoundStarted = false;
            _isSetup = true;
        }

        private void SetSetupToFalse()
        {
            _isSetup = false;
        }

        #endregion


        #region Private

        protected PlayerAI _playerAi;

        protected bool _isSetup;

        protected ITeamable _teamable
        {
            get { return _playerAi.i_teamable; }
        }

        protected List<ITeamable> _teamables
        {
            get { return _playerAi.i_teamables; }
        }
        protected List<ITeamable> _enemyTeamables
        {
            get { return _playerAi.i_enemyTeamables; }
        }
        protected List<ITeamable> _allyTeamables
        {
            get { return _playerAi.i_allyTeamables; }
        }

        protected GameMode _currentGameModeManager
        {
            get { return _playerAi.i_currentGameModeManager; }
        }

        #endregion
    }
}