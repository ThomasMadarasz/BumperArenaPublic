using UnityEngine;
using Mirror;
using Archi.Runtime;
using Utils.Runtime;
using Feedbacks.Runtime;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(PlayerProperties))]
    public class PlayerRotation : CNetBehaviour
    {
        
        #region Unity API
        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }


        [ServerCallback]
        private void Update()
        {
            if (!_p || !CanRotate()) return;
            SetRotation();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _p = GetComponent<PlayerProperties>();
            _feedbacks = GetComponent<PlayerFeedbacks>();
        }

        private void SetRotation()
        {
            if (_p.i_isInvincible)
            {
                transform.rotation = _p.i_respawnRotation;
                return;
            }

            if (_p.i_isBumped && _p.i_isTorqueApplied)
            {
                ApplyTorque();
                return;
            }

            if (rigidBody.velocity.AxisToZero(Axis.X, Axis.Z) == Vector3.zero) return;

            float angle = Vector2.SignedAngle(rigidBody.velocity.ToVector2().normalized, transform.forward.ToVector2().normalized);

            if (Mathf.Abs(angle) <= 5)
                transform.rotation = Quaternion.LookRotation(rigidBody.velocity.AxisToZero(Axis.X, Axis.Z).normalized);
            else 
            {
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, rigidBody.velocity.AxisToZero(Axis.X, Axis.Z), Time.deltaTime * _p.m_controllerData.m_unlockedDuration, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }

        private void ApplyTorque()
        {
            if (_torquePower > 0) _torquePower -= _p.m_controllerData.m_torqueDeceleration * Time.deltaTime;
            else if (_torquePower < 0) _torquePower += _p.m_controllerData.m_torqueDeceleration * Time.deltaTime;

            transform.RotateAround(transform.position, Vector3.up, _torquePower * Time.deltaTime);
        }

        //Communique avec Car Collision :/

        public void TorquePower(float distance, float impact, float angle, float torqueFactor)
        {
            _torquePower = impact * distance * Mathf.Sin((angle) * Mathf.Deg2Rad) * torqueFactor;
        }

        #endregion


        #region Utils & Tools
        private bool CanRotate() => _p.i_isAlive && _p.i_isGameStarted && !_p.i_isGameOver;

        #endregion


        #region Private

        private PlayerProperties _p;
        private float _torquePower;
        private PlayerFeedbacks _feedbacks;

        #endregion
    }
}