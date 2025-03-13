using System.Collections.Generic;
using UnityEngine;
using ScriptableEvent.Runtime;
using Archi.Runtime;
using PlayerController.Parameters.Runtime;
using Interfaces.Runtime;
using System.Linq;
using Core.Runtime;

namespace GameModes.Runtime
{
    public class GameMode : CNetBehaviour
    {
        #region Exposed

        public int m_gameModeId;

        [SerializeField] private GameEvent _onGameSceneLoaded;
        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] internal GameEvent _onRoundFinished;
        [SerializeField] private GameEventT _onCollisionWithPlayer;

        #endregion


        #region Unity API

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        protected virtual void OnDestroy()
        {
            _onGameSceneLoaded.UnregisterListener(OnGameSceneLoaded);
            _onRoundStart.UnregisterListener(OnRoundStart);
            _onRoundFinished.UnregisterListener(OnRoundFinished);
            _onCollisionWithPlayer.UnregisterListener(OnCollisionWithPlayer);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _scoreManager = ScoreManager.s_instance;
            _gameManager = GameManager.s_instance;

            _gameManager.SetCurrentGameModeID(m_gameModeId);

            _onGameSceneLoaded.RegisterListener(OnGameSceneLoaded);
            _onRoundStart.RegisterListener(OnRoundStart);
            _onRoundFinished.RegisterListener(OnRoundFinished);
            _onCollisionWithPlayer.RegisterListener(OnCollisionWithPlayer);
        }

        #endregion


        #region Virtual

        protected virtual void OnGameSceneLoaded()
        {

        }

        protected virtual void OnRoundStart()
        {

        }

        protected virtual void OnRoundFinished()
        {
            //_onGameSceneLoaded.UnregisterListener(OnGameSceneLoaded);
            //_onRoundStart.UnregisterListener(OnRoundStart);
            //_onRoundFinished.UnregisterListener(OnRoundFinished);
            //_onCollisionWithPlayer.UnregisterListener(OnCollisionWithPlayer);
        }

        protected virtual void OnCollisionWithPlayer(object parameters)
        {

        }

        protected virtual CollisionParameters[] GetParameters(CollisionParameters parameters)
        {
            CollisionParameters matchingCp = _collisions.FirstOrDefault(x => x.m_ownerTeamable == parameters.m_otherTeamable && x.m_otherTeamable == parameters.m_ownerTeamable);

            if (matchingCp == null)
            {
                Debug.Log("Add Collision To list");
                _collisions.Add(parameters);
                return null;
            }
            Debug.Log("Collision Match");
            return new CollisionParameters[] { parameters, matchingCp };
        }

        public virtual Transform[] GetObjectivesTransforms() => null;

        public virtual List<int> GetPlayersWithObjective() => null;

        public virtual Dictionary<ITeamable, List<Checkpoint>> GetCheckpointsState() => null;

        public virtual List<Checkpoint> GetCheckpoints() => null;

        public virtual Rigidbody GetPhysicObjectiveRigidbody() => null;

        public virtual bool IsGameRunning() => true;

        #endregion


        #region Private


        protected ScoreManager _scoreManager;
        protected GameManager _gameManager;

        private List<CollisionParameters> _collisions = new List<CollisionParameters>();

        #endregion
    }
}