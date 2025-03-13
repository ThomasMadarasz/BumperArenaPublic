using Archi.Runtime;
using Inputs.Runtime;
using PlayerController.Data.Runtime;
using UnityEngine;
using Mirror;
using Utils.Runtime;
using System;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using LocalPlayer.Runtime;
using System.Collections.Generic;
using Feedbacks.Runtime;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(PlayerProperties))]
    public class PlayerMovements : CNetBehaviour, ISubjectToAForce, IMoveable
    {
        #region Exposed

        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private GameEvent _onRoundStarted;

        #endregion


        #region Unity API

        private void Awake()
        {
            _canMove = false;
            _p = GetComponent<PlayerProperties>();
            _inputsReader = _p.m_inputs;
            _turnStatePathHash = Animator.StringToHash("Base.Turn");
        }

        private void FixedUpdate()
        {
            if (!_animator) _animator = GetComponent<IPlayerGraphic>().GetInGameAnimator();
            if (!CanMove())
            {
                UpdateTurnAnim(.5f);
                return;
            }

            PlayerSpeedData data = GetSpeedData();

            Vector3 direction = GetDirection(data);
            float power = GetPower(data);


            float angle = Vector2.SignedAngle(direction.ToVector2().normalized, transform.forward.ToVector2().normalized);

            _animTurnTime = Mathf.Lerp(_animTurnTime, Mathf.Clamp01(angle / 90 / 2 + 0.5f), Time.deltaTime * 2);

            UpdateTurnAnim(_animTurnTime);

            AddMoveForce(data, direction, power);
        }

        private void Update()
        {
            _inputsReader.SetClientMoveDirection(_inputsReader.GetMoveDirection());

            if (!_canMove) return;

            if (isOwned || isLocalPlayer)
            {
                if (!NetworkClient.ready) return;
                _inputs = _inputsReader.GetMoveDirection();
            }
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            Setup();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SetDefaultValues();

            IKillable killable = GetComponent<IKillable>();
            if (killable != null)
            {
                killable.m_onPlayerDie += SetCanMoveFalse;
                killable.m_onPlayerDie += ResetExternalForce;
                killable.m_onPlayerDie += ResetExternalController;
                killable.m_onPlayerRespawn += ApplyDefaultMovement;
            }

            _onRoundFinished.RegisterListener(StopMovement);
            _onRoundStarted.RegisterListener(ApplyDefaultMovement);
        }

        private void OnDestroy()
        {
            _onRoundFinished.UnregisterListener(StopMovement);
            _onRoundStarted.UnregisterListener(ApplyDefaultMovement);
        }

        #endregion


        #region Main

        public void AddExternalForce(Vector3 force)
        {
            _externalForce = force;
        }

        private void Setup()
        {
            _feedbacks = GetComponent<PlayerFeedbacks>();

            if (!_p.m_isAnAI)
            {
                int deviceId = LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(_p.i_localPlayerID);

                _inputsReader.enabled = true;
                _inputsReader.ActivateInput();

                if (deviceId == -1 && LocalPlayerManager.s_instance.IsDefaultPlayer(_p.i_localPlayerID))
                {
                    _inputsReader.SetDevicesInCurrentActionMap(_inputsReader.GetAllDevices());
                }
                else
                {
                    _inputsReader.SetMainDevice(deviceId);
                    _inputsReader.SetDevicesInCurrentActionMap(new List<int>() { deviceId });
                }

                GetComponent<PlayerFeedbacks>().SetupGamepad();
            }
        }

        private void AddMoveForce(PlayerSpeedData data, Vector3 direction, float power)
        {
            Vector3 force = power * direction;

            Vector3 pos = transform.position + _p.m_controllerData.m_ForceApplicationPoint;

            //_p.m_isBoosted peut etre l'origine de problemes

            Vector3 totalForce = force + ((_externalForce) * Convert.ToInt32(!_p.i_isBoosted));

            ApplyForce(totalForce, pos, data.m_forceMode);

            if (_useSmoothSpeedClamp)
            {
                DecelerationProfile prof = GetDecelerationData();
                _decelerationRemainingTime -= Time.fixedDeltaTime;

                if (rigidBody.velocity.magnitude <= data.m_minSpeed) rigidBody.velocity = rigidBody.velocity.normalized * data.m_minSpeed;

                if (rigidBody.velocity.magnitude >= data.m_maxSpeed * _p.i_speedMultiplier)
                {
                    if (_decelerationRemainingTime <= 0) rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, data.m_maxSpeed * _p.i_speedMultiplier);
                    else
                    {
                        float evaluateTime = 1 - (_decelerationRemainingTime / prof.m_decelerationTime);
                        evaluateTime = Mathf.Clamp01(evaluateTime);

                        float curveValue = prof.m_curve.Evaluate(evaluateTime);

                        float speedToDecreaseFloat = rigidBody.velocity.magnitude - (data.m_maxSpeed * _p.i_speedMultiplier);
                        if (speedToDecreaseFloat < 0) speedToDecreaseFloat = 0;

                        Vector3 speedToDecrease = rigidBody.velocity.normalized * speedToDecreaseFloat;
                        speedToDecrease *= curveValue;
                        speedToDecrease *= evaluateTime;

                        rigidBody.velocity -= speedToDecrease;
                    }
                }
            }
            else
            {
                if (_externalForce == Vector3.zero)
                    ClampSpeed(data.m_minSpeed, data.m_maxSpeed * _p.i_speedMultiplier);
                else
                    ClampSpeed(data.m_minSpeed, data.m_maxSpeedWithExternalForce * _p.i_speedMultiplier);
            }
        }

        private void ApplyForce(Vector3 force, Vector3 pos, ForceMode mode)
        {
            if (force != Vector3.zero) rigidBody.AddForceAtPosition(force, pos, mode);
        }

        private void ClampSpeed(float minSpeed, float maxSpeed)
        {
            if (rigidBody.velocity.magnitude <= minSpeed) rigidBody.velocity = rigidBody.velocity.normalized * minSpeed;
            if (rigidBody.velocity.magnitude >= maxSpeed) rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);
        }

        #endregion


        #region Utils & Tools

        private void ResetExternalController()
        {
            _externalControllerData = null;
        }

        private float GetPower(PlayerSpeedData data)
        {
            float angleToFactor = SinAbs(_angle);

            float handlingFactor = (1 + (1 + (angleToFactor * data.m_handling))) * 10;

            float power = data.m_acceleration * handlingFactor * Time.fixedDeltaTime * _p.i_accelerationMultiplier;

            return power;
        }

        private Vector3 GetDirection(PlayerSpeedData data)
        {
            if (_p.i_isInvincible) return _p.i_respawnRotation.eulerAngles;
            Vector2 direction;
            _inputsDirection = GetInputDirection();

            Vector2 transformV2 = transform.forward.ToVector2();

            if (_inputsDirection != Vector2.zero) _angle = Vector2.SignedAngle(transformV2, _inputsDirection);

            if (_inputsDirection.magnitude < 0.1f)
            {
                _angle = 0;
                return transformV2.ToVector3().normalized;
            }
            else if (Mathf.Abs(_angle) < data.m_maxTurnAngle) direction = _inputsDirection;
            else
            {
                direction = AngleToVector2(_angle > 0 ? -data.m_maxTurnAngle + transform.eulerAngles.y : data.m_maxTurnAngle + transform.eulerAngles.y);
                _angle = data.m_maxTurnAngle;
            }

            //if (_p.i_isCollisionsImmune) direction = transformV2;

            return direction.ToVector3().normalized;
        }

        private Vector2 GetInputDirection()
        {
            if (_p.m_isAnAI) return _p.m_aiInputs;

            if (_p.i_isOrientationWorld)
            {
                if (_inputs.magnitude < 0.1f) return Vector2.zero;
                else return _inputs;
            }
            else
            {
                if (_inputs.magnitude < 0.1f) return Vector2.zero;
                else return (transform.right * _inputs.x + transform.forward * Mathf.Sin(Mathf.Acos(_inputs.x))).ToVector2();
            }
        }

        private Vector2 AngleToVector2(float angle) => new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));

        private float SinAbs(float angle) => Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad));

        private bool CanMove() => _p.i_isAlive && !_p.i_isBumped && _p.i_isGameStarted && !_p.i_isGameOver && _canMove;

        private void SetDefaultValues()
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.ResetInertiaTensor();

            _externalForce = Vector3.zero;
        }

        public void ChangeMoveData(object controller, object deceleration)
        {
            _externalControllerData = controller as PlayerControllerData;
            _externalDecelerationData = deceleration as PlayerDecelerationData;

            _useSmoothSpeedClamp = controller != null;
            if (_useSmoothSpeedClamp)
            {
                _decelerationRemainingTime = GetDecelerationData().m_decelerationTime;
            }

            if (controller == null)
                _feedbacks.RemoveVoidFeedback();
            else
                _feedbacks.ApplyVoidFeedback();
        }

        private PlayerSpeedData GetSpeedData()
        {
            PlayerControllerData controller = _externalControllerData == null ? _p.m_controllerData : _externalControllerData;

            return _p.i_isBoosted && !_p.i_isBumped ? controller.m_boostSpeedData : controller.m_speedData;
        }

        private DecelerationProfile GetDecelerationData()
        {
            return _p.i_isBoosted && !_p.i_isBumped ? _externalDecelerationData.m_boostProfile : _externalDecelerationData.m_normalProfile;
        }

        private void StopMovement()
        {
            _canMove = false;
            SetDefaultValues();
        }

        private void SetCanMoveFalse() => _canMove = false;

        private void ResetExternalForce() => _externalForce = Vector3.zero;

        private void ApplyDefaultMovement()
        {
            _canMove = true;
            rigidBody.velocity = transform.forward * _p.m_controllerData.m_speedData.m_maxSpeed;
        }

        private void UpdateTurnAnim(float value)
        {
            if (_animator && _animator.isActiveAndEnabled && _animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _turnStatePathHash)
            {
                _animator.Play(_turnStatePathHash, 0, value);
                _animator.speed = 0;
            }
        }

        #endregion


        #region Private

        private PlayerProperties _p;
        private InputsReader _inputsReader;
        private PlayerFeedbacks _feedbacks;

        private PlayerControllerData _externalControllerData;
        private PlayerDecelerationData _externalDecelerationData;

        private Vector2 _inputs;
        private Vector2 _inputsDirection;

        private Vector3 _externalForce;

        private float _angle;
        private float _decelerationRemainingTime;
        private float _animTurnTime;

        private bool _useSmoothSpeedClamp;
        private bool _canMove;

        private Animator _animator;
        private int _turnStatePathHash;

        #endregion
    }
}