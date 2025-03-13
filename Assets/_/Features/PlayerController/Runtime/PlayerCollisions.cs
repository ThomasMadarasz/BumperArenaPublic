using UnityEngine;
using Archi.Runtime;
using PlayerController.Data.Runtime;
using Utils.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using PlayerController.Parameters.Runtime;
using Feedbacks.Runtime;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(PlayerProperties), typeof(PlayerRotation))]
    public class PlayerCollisions : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private GameEventT _onCollisionWithPlayer;
        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private RumbleData _onCollisionData;

        #endregion


        #region Unity API

        private void Awake()
        {
            _feedback = GetComponent<IFeedback>();
            _onRoundFinished.RegisterListener(OnRoundFinished);
        }

        private void OnDestroy()
        {
            if (_bumpTimer != null) _bumpTimer.Stop();
            _onRoundFinished.UnregisterListener(OnRoundFinished);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_p.i_isAlive || !_p.i_isGameStarted || _p.i_isGameOver)
            {
                OnRoundFinished();
                return;
            }

            if (_p.i_isInvincible)
            {
                rigidBody.velocity = transform.forward * _p.m_controllerData.m_speedData.m_maxSpeed;
                return;
            }

            if (_p.i_isCollisionsImmune && collision.gameObject.CompareTag("Player"))
            {
                rigidBody.velocity = _previousVelocity;
                return;
            }

            AddForceCollision(collision);
            SetBumpedBy(collision);
        }

        private void Update()
        {
            _velocity = rigidBody.velocity;
            _previousVelocity = rigidBody.velocity;
        }

        #endregion


        #region Main

        private void Setup()
        {
            _p = GetComponent<PlayerProperties>();
            _data = _p.m_collData;
            _r = GetComponent<PlayerRotation>();
            _feedbacks = GetComponent<PlayerFeedbacks>();
        }

        private void AddForceCollision(Collision coll)
        {
            Vector2 normalDir = coll.contacts[0].normal.ToVector2();
            Vector2 centerToContactDir = coll.contacts[0].point.ToVector2() - transform.position.ToVector2();
            Vector3 contactPos = coll.contacts[0].point;
            float relativeVel = coll.relativeVelocity.magnitude;

            Vector2 forceDirection = Vector2.Reflect(centerToContactDir.normalized, normalDir.normalized);

            float power = Power(coll);

            if (power >= 0.1f) power = Mathf.Clamp(power, _p.m_collData.m_powerWithEnvironment, _p.m_collData.m_maxPower);

            Vector3 force = power * forceDirection.ToVector3();

            float bumpFactor = (force + _velocity).magnitude;
            float duration = _data.m_durationFactor * bumpFactor;
            Bumped(duration, bumpFactor);

            rigidBody.AddForceAtPosition(force * _data.m_power, contactPos, _data.m_forceMode);

            _r.TorquePower(centerToContactDir.magnitude, relativeVel, Vector2.SignedAngle(transform.forward.ToVector2(), contactPos.ToVector2() - coll.contacts[0].normal.ToVector2()), _data.m_torqueFactor);

            _p.i_isBumped = true;
        }

        private void Bumped(float duration, float bumpFactor)
        {
            _p.i_bumpDuration = duration;
            if (_bumpTimer != null)
            {
                _bumpTimer.Stop();
                _bumpTimer = null;
            }
            _bumpTimer = new(duration, OnBumpTimerOver);
            _bumpTimer.Start();

            _feedbacks.ApplyTorqueFeedback();
        }

        private void SetBumpedBy(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                _feedbacks.PlayBumpSfx();
                return;
            }

            ITeamable thisTeamable = GetComponent<ITeamable>();
            ITeamable otherTeamable = collision.gameObject.GetComponent<ITeamable>();

            CollisionParameters parameters = new CollisionParameters();
            parameters.m_ownerTeamable = thisTeamable;
            parameters.m_otherTeamable = otherTeamable;
            parameters.m_bumpTime = _p.i_bumpDuration;
            parameters.m_feedback = _feedback;

            _onCollisionWithPlayer.Raise(parameters);
            _feedbacks.PlayRumble(_onCollisionData);
            _feedbacks.PlayTorqueSfx();
        }

        #endregion


        #region Utils & Tools

        private void OnRoundFinished()
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            _p.i_isTorqueApplied = false;
            _p.i_isBumped = false;

            if (_bumpTimer != null) _bumpTimer.Stop();
        }

        private float Power(Collision coll)
        {
            if (coll.gameObject.CompareTag("Player"))
            {
                if (_p.i_isCollisionsImmune || _p.i_isInvincible) return 0;
                PlayerProperties collP = coll.gameObject.GetComponent<PlayerProperties>();

                _p.i_isTorqueApplied = true;

                if (collP.i_isCollisionsImmune || collP.i_isInvincible) return coll.relativeVelocity.magnitude;
                else if (_p.i_isBoosted && !collP.i_isBoosted) return coll.relativeVelocity.magnitude * _p.m_collData.m_dominantBumpFactor;
                else return coll.relativeVelocity.magnitude;
            }
            else
            {
                float multiplier = 1;

                IBouncable bouncable = coll.gameObject.GetComponent<IBouncable>();
                if (bouncable != null) multiplier = bouncable.GetBounceFactor();

                if (_p.i_isBumped && _p.i_isTorqueApplied)
                    return coll.relativeVelocity.magnitude * _p.m_collData.m_bounceFactor * multiplier;
                else
                    return _p.m_collData.m_powerWithEnvironment * multiplier;
            }

        }

        //Warning Changes here
        private void OnBumpTimerOver()
        {
            _p.i_isBumped = false;
            _p.i_isTorqueApplied = false;

            _feedbacks.RemoveTorqueFeedback();
        }

        #endregion


        #region Private

        private PlayerProperties _p;
        private PlayerRotation _r;
        private PlayerCollisionsData _data;
        private PlayerFeedbacks _feedbacks;

        private Vector3 _velocity;
        private Vector3 _previousVelocity;

        private IFeedback _feedback;

        private Timer _bumpTimer;

        #endregion
    }
}