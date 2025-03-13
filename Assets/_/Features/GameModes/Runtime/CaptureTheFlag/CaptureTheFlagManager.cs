using System.Collections.Generic;
using UnityEngine;
using Core.Runtime;
using ScriptableEvent.Runtime;
using Interfaces.Runtime;
using PlayerController.Parameters.Runtime;
using Feedbacks.Runtime;
using ScreenShakes.Runtime;

namespace GameModes.Runtime
{
    public class CaptureTheFlagManager : GameMode
    {
        #region Exposed

        [SerializeField] private CaptureTheFlagData _data;
        [SerializeField] private FlagBase[] _flagBases = new FlagBase[2];
        [SerializeField] private GameEventT2 _onPlayerDie;

        [SerializeField] private GameEventT _onPlayerTakeFlag;
        [SerializeField] private GameEventT _onFlagReset;

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
            base.OnDestroy();
            _onPlayerDie.UnregisterListener(OnPlayerDie);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _scoreManager = ScoreManager.s_instance;
            _onPlayerDie.RegisterListener(OnPlayerDie);
        }

        public void PlayerStoleFlag(ITeamable teamable, IFeedback player)
        {
            int playerId = teamable.GetPlayerID();
            if (_playersWithFlag.Contains(playerId))
            {
                Debug.Log("Warning : Player already has a flag and try to get a new one");
                return;
            }

            _playersWithFlag.Add(playerId);
            teamable.SetSpeedMultiplier(_data.m_speedMultiplier);
            teamable.SetAccelerationMultiplier(_data.m_accelerationMultiplier);

            int playerFeedbackId = player.GetID();
            FeedbackParameters parameters = new FeedbackParameters()
            { m_id = playerFeedbackId, m_params = new object[1] { teamable.GetTeamID() - 1 } };
            _onPlayerTakeFlag?.Raise(parameters);
        }

        public void PlayerScored(int playerId, ITeamable teamable, IFeedback player)
        {
            if (!_playersWithFlag.Contains(playerId))
            {
                Debug.Log("Warning : Player has no flag but try to score");
                return;
            }
            _scoreManager.ScorePoint(teamable.GetTeamID());
            _playersWithFlag.Remove(playerId);

            _flagBases[teamable.GetTeamID() - 1].Goal();

            ScreenShakeParameters ssParameters = new ScreenShakeParameters(_ssData);
            _onScreenShake.Raise(ssParameters);

            int playerFeedbackId = player.GetID();
            FeedbackParameters parameters = new FeedbackParameters()
            { m_id = playerFeedbackId, m_params = new object[1] { teamable.GetTeamID() - 1 } };
            _onFlagReset?.Raise(parameters);
        }

        protected override void OnCollisionWithPlayer(object parameters)
        {
            CollisionParameters cp = (CollisionParameters)parameters;

            CollisionParameters[] collisionParameters = base.GetParameters(cp);

            if (collisionParameters == null) return;

            ITeamable[] teamables = new ITeamable[] { collisionParameters[0].m_ownerTeamable, collisionParameters[1].m_ownerTeamable };

            int ownerId = teamables[0].GetPlayerID();
            int otherId = teamables[1].GetPlayerID();

            int ownerTeamId = teamables[0].GetTeamID();
            int otherTeamId = teamables[1].GetTeamID();

            if (ownerTeamId == otherTeamId) return;

            if (_playersWithFlag.Contains(ownerId) && !_playersWithFlag.Contains(otherId))
            {
                _flagBases[otherTeamId - 1].ResetFlag(false);
                teamables[0].ResetSpeedMultipliers();
                _playersWithFlag.Remove(ownerId);

                int playerFeedbackId = collisionParameters[0].m_feedback.GetID();
                FeedbackParameters param = new FeedbackParameters() { m_id = playerFeedbackId, m_params = new object[1] { otherTeamId - 1 } };
                _onFlagReset?.Raise(param);
            }
            else if (_playersWithFlag.Contains(otherId) && !_playersWithFlag.Contains(ownerId))
            {
                _flagBases[ownerTeamId - 1].ResetFlag(false);
                teamables[1].ResetSpeedMultipliers();
                _playersWithFlag.Remove(otherId);

                int playerFeedbackId = collisionParameters[1].m_feedback.GetID();
                FeedbackParameters param = new FeedbackParameters() { m_id = playerFeedbackId, m_params = new object[1] { ownerTeamId - 1 } };
                _onFlagReset?.Raise(param);
            }
        }

        private void OnPlayerDie(object obj, object other)
        {
            object[] args = (object[])obj;
            ITeamable teamable = (ITeamable)args[1];

            int playerId = teamable.GetPlayerID();

            if (_playersWithFlag.Contains(playerId))
            {
                IFeedback feedback = (args[3] as GameObject).GetComponent<IFeedback>();

                _flagBases[teamable.GetTeamID() - 1].ResetOppositeFlag(false);

                _playersWithFlag.Remove(playerId);
                teamable.ResetSpeedMultipliers();

                FeedbackParameters param = new FeedbackParameters() { m_id = feedback.GetID(), m_params = new object[1] { teamable.GetTeamID() - 1 } };
                _onFlagReset?.Raise(param);
            }
        }

        public override Transform[] GetObjectivesTransforms()
        {
            Transform[] transforms = new Transform[2];
            transforms[_flagBases[0].i_teamID - 1] = _flagBases[0].transform;
            transforms[_flagBases[1].i_teamID - 1] = _flagBases[1].transform;
            return transforms;
        }

        public override List<int> GetPlayersWithObjective() => _playersWithFlag;

        #endregion


        #region Utils

        public bool IsPlayerWithFlag(int playerId) => _playersWithFlag.Contains(playerId);


        #endregion


        #region Private

        private List<int> _playersWithFlag = new List<int>();

        #endregion
    }
}