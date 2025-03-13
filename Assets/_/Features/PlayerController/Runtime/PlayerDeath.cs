using Archi.Runtime;
using Feedbacks.Runtime;
using Interfaces.Runtime;
using Networking.Runtime;
using ScriptableEvent.Runtime;
using System;
using UnityEngine;
using Utils.Runtime;
using Enum.Runtime;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(PlayerProperties))]
    public class PlayerDeath : CNetBehaviour, IKillable
    {
        #region Exposed

        public event Action m_onPlayerDie;
        public event Action m_onPlayerRespawn;

        [SerializeField] private GameEventT2 _onPlayerDie;
        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEventT _onRespawn;
        [SerializeField] private GameEvent _onAllPlayerAsSceneLoaded;
        [SerializeField] private GameEvent _onFeedbacksInitialized;
        [SerializeField] private GameEventT _onPlayerRespawnImmunityEnd;

        #endregion


        #region Unity API

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        private void Awake()
        {
            _feedback = GetComponent<IFeedback>();
            _team = GetComponent<ITeamable>();
        }

        private void OnDestroy()
        {
            _onAllPlayerAsSceneLoaded.UnregisterListener(ResetPlayerDeath);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _p = GetComponent<PlayerProperties>();
            _onAllPlayerAsSceneLoaded.RegisterListener(ResetPlayerDeath);
        }

        public void Kill(DeathType type)
        {
            if (_p.i_isGameOver || !_p.i_isGameStarted || _p.i_isInvincible || !_p.i_isAlive) return;

            collider.isTrigger = true;
            collider.enabled = false;
            _p.i_isAlive = false;

            if (type != DeathType.Ejected)
            {
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }

            // This event is visible in all classes

            int playerFeedbackId = _feedback.GetID();
            FeedbackParameters param = new FeedbackParameters()
            { m_id = playerFeedbackId, m_params = new object[0] };

            object[] args = new object[4] { this, _team, type, gameObject };
            _onPlayerDie.Raise(args, param);

            // This event is visible only for same instance of player of this class
            m_onPlayerDie?.Invoke();
        }

        public void Respawn(Transform tr, Quaternion rot)
        {
            if (_p.i_isAlive) return;

            collider.isTrigger = false;

            transform.position = tr.position;
            transform.rotation = rot;

            rigidBody.velocity = transform.forward * _p.m_controllerData.m_speedData.m_maxSpeed;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.ResetInertiaTensor();

            collider.enabled = true;

            _p.i_isInvincible = true;
            _p.i_respawnRotation = rot;

            _p.i_isAlive = true;

            object[] param = new object[3] { true, _p.m_isAnAI,false };
            FeedbackParameters parameters = new FeedbackParameters() { m_id = _feedback.GetID(), m_params = param };
            _onRespawn?.Raise(parameters);

            m_onPlayerRespawn?.Invoke();
        }

        private void ResetPlayerDeath()
        {
            collider.enabled = true;
            _p.i_isAlive = false;

            CustomNetworkManager manager = CustomNetworkManager.singleton as CustomNetworkManager;
            object[] param = new object[3] { !manager.IsGameScene(), _p.m_isAnAI ,true};
            FeedbackParameters parameters = new FeedbackParameters() { m_id = _feedback.GetID(), m_params = param };
            _onRespawn?.Raise(parameters);

            m_onPlayerDie?.Invoke();
        }

        public void Spawn(Transform tr, Quaternion rot)
        {
            Respawn(tr, rot);
        }

        public void MoveTo(Transform tr)
        {
            transform.position = tr.position;
            transform.rotation = tr.rotation;
        }

        public void UpdateColliderCollisionFor(Collider coll, bool ignore)
        {
            Physics.IgnoreCollision(collider, coll, ignore);
        }

        #endregion


        #region Utils & Tools

        public IPlayer GetPlayerPorperties() => _p;

        public ITeamable GetTeamable() => _team;

        public IFeedback GetFeedback() => _feedback;

        public void DisableInvincibility()
        {
            _p.i_isInvincible = false;

            FeedbackParameters parameters = new FeedbackParameters() { m_id = _feedback.GetID() };
            _onPlayerRespawnImmunityEnd.Raise(parameters);
        }

        public bool PlayerUseRespawnAccelerationBonus()
        {
            return _p.m_inputs.SubmitButtonPressed();
        }

        public float GetRespawnDirection(Transform t)
        {
            Vector2 direction = _p.m_inputs.GetClientMoveDirection();
            Vector3 forward = - t.forward;
            Vector3 right = - t.right;

            if (!_p.i_isOrientationWorld && direction.magnitude >= 0.2f) return -Vector3.SignedAngle(forward, right * direction.x + forward * Mathf.Sin(Mathf.Acos(direction.x)), Vector3.up);
            if (direction.magnitude >= 0.2f) return -Vector3.SignedAngle(forward, direction.ToVector3(), Vector3.up);
            else return 0;
        }

        #endregion


        #region Private

        private PlayerProperties _p;

        private IFeedback _feedback;
        private ITeamable _team;

        #endregion
    }
}