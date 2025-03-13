using UnityEngine;
using Utils.Runtime;
using ScriptableEvent.Runtime;
using ScreenShakes.Runtime;

namespace GameModes.Runtime
{
    public class AirHockeyManager : GameMode
    {
        #region Exposed

        [SerializeField] private AirHockeyData _data;
        [SerializeField] private Puck _puck;
        [SerializeField] private Goal[] _goals = new Goal[2];

        [SerializeField] private ScreenShakeData _ssData;

        [SerializeField] private GameEventT _onScreenShake;

        #endregion


        #region Unity API

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        protected override void OnDestroy()
        {
            _respawnPuckTimer.Stop();
            base.OnDestroy();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _respawnPuckTimer = new(_data.m_respawnPuckTime, OnRespawnPuckTimerOver);
        }

        public void Score(int teamId)
        {
            _scoreManager.ScorePoint(teamId);
            _respawnPuckTimer.Start();

            ScreenShakeParameters ssParameters = new ScreenShakeParameters(_ssData);
            _onScreenShake.Raise(ssParameters);
        }

        #endregion


        #region Utils & Tools

        private void OnRespawnPuckTimerOver()
        {
            _puck.gameObject.SetActive(true);
            _puck.Spawn();
        }

        protected override void OnGameSceneLoaded()
        {
            base.OnGameSceneLoaded();
        }

        protected override void OnRoundStart()
        {
            base.OnRoundStart();
            _puck.Spawn();
        }

        protected override void OnRoundFinished()
        {
            base.OnRoundFinished();
            _puck.Disappear();
            _puck.Freeze();
        }

        public override Transform[] GetObjectivesTransforms()
        {
            Transform[] transforms = new Transform[3];
            transforms[0] = _puck.transform;
            transforms[_goals[0].i_teamId] = _goals[0].transform;
            transforms[_goals[1].i_teamId] = _goals[1].transform;
            return transforms;
        }

        public override Rigidbody GetPhysicObjectiveRigidbody() => _puck.rigidBody;

        public override bool IsGameRunning() => _respawnPuckTimer.IsTimeOver();

        #endregion


        #region Private

        private Timer _respawnPuckTimer;

        #endregion
    }
}