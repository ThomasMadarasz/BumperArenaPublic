using System.Collections.Generic;
using UnityEngine;
using ScriptableEvent.Runtime;
using Interfaces.Runtime;
using Utils.Runtime;
using PlayerController.Parameters.Runtime;
using Feedbacks.Runtime;

namespace GameModes.Runtime
{
    public class CrownManager : GameMode
    {
        #region Exposed

        [SerializeField] private CrownData _data;
        [SerializeField] private CrownTrigger _crown;

        [SerializeField] private GameEventT _onTakeCrown;
        [SerializeField] private GameEventT _onCrownImmunityFinished;
        [SerializeField] private GameEventT _onLostCrown;

        #endregion


        #region Unity API

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _immunityTimer.Stop();
            _scoreTimer.Stop();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _scoreTimer = new(_data.m_scoreTimer, OnScoreTimerOver, true);
            _immunityTimer = new(_data.m_immunityTimer, OnImmunityTimerOver);
        }

        public void GiveCrown(ITeamable teamable, IFeedback feedback)
        {
            _crownPlayer = teamable;
            _crownOwnerFeedback = feedback;

            _crownPlayer.SetSpeedMultiplier(_data.m_speedMultiplier);
            _crownPlayer.SetAccelerationMultiplier(_data.m_accelerationMultiplier);
            _scoreTimer.Start();

            _immunityTimer.Start();

            FeedbackParameters param = new() { m_id = _crownOwnerFeedback.GetID(), m_params = new object[1] { _data.m_immunityTimer } };
            _onTakeCrown?.Raise(param);
        }

        #endregion


        #region Events

        protected override void OnCollisionWithPlayer(object parameters)
        {
            CollisionParameters cp = (CollisionParameters)parameters;

            CollisionParameters[] collisionParameters = base.GetParameters(cp);

            if (collisionParameters == null) return;

            ITeamable[] teamables = new ITeamable[] { collisionParameters[0].m_ownerTeamable, collisionParameters[1].m_ownerTeamable };

            if (_crownPlayer == null || !_immunityTimer.IsTimeOver()) return;


            int ownerId = teamables[0].GetTeamID();
            int otherId = teamables[1].GetTeamID();
            int currentId = _crownPlayer.GetTeamID();

            if (currentId == ownerId) SetNewCrownOwner(teamables[1], teamables[0], collisionParameters[1].m_feedback);
            else if (currentId == otherId) SetNewCrownOwner(teamables[0], teamables[1], collisionParameters[0].m_feedback);
        }

        private void SetNewCrownOwner(ITeamable ownerTeamable, ITeamable loserTeamable, IFeedback feedback)
        {
            if (_crownPlayer != loserTeamable) return;
            _crownPlayer = ownerTeamable;

            FeedbackParameters param = new() { m_id = _crownOwnerFeedback.GetID() };
            _onLostCrown?.Raise(param);

            _crownOwnerFeedback = feedback;

            _crownPlayer.SetSpeedMultiplier(_data.m_speedMultiplier);
            _crownPlayer.SetAccelerationMultiplier(_data.m_accelerationMultiplier);
            loserTeamable.ResetSpeedMultipliers();

            _crownPlayer.SetImmuneToCollisions(true);
            _immunityTimer.Start();
            _scoreTimer.ResetTimer();

            param = new() { m_id = _crownOwnerFeedback.GetID(), m_params = new object[1] { _data.m_immunityTimer } };
            _onTakeCrown?.Raise(param);
        }

        private void OnScoreTimerOver()
        {
            _scoreManager.ScorePoint(_crownPlayer);
            if (_scoreManager.GetTeamableScore(_crownPlayer) >= _data.m_scoreToReachToWinRound) _gameManager.FinishRound();
        }

        private void OnImmunityTimerOver()
        {
            _crownPlayer.SetImmuneToCollisions(false);

            FeedbackParameters param = new() { m_id = _crownOwnerFeedback.GetID() };
            _onCrownImmunityFinished?.Raise(param);
        }

        public override Transform[] GetObjectivesTransforms()
        {
            Transform[] transforms = new Transform[1];
            transforms[0] = _crown.transform;
            return transforms;
        }

        public override List<int> GetPlayersWithObjective()
        {
            List<int> list = new List<int>();
            if (_crownPlayer != null) list.Add(_crownPlayer.GetPlayerID());
            return list;
        }

        protected override void OnRoundFinished()
        {
            OnImmunityTimerOver();
            _crownPlayer.ResetSpeedMultipliers();
            _crownPlayer = null;
            if(_scoreTimer != null) _scoreTimer.Stop();
            if (_immunityTimer != null) _immunityTimer.Stop();
            base.OnRoundFinished();
        }

        #endregion


        #region Private

        private ITeamable _crownPlayer = null;

        private IFeedback _crownOwnerFeedback;

        private Timer _scoreTimer;
        private Timer _immunityTimer;

        #endregion
    }
}