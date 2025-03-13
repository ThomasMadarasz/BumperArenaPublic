using Archi.Runtime;
using Audio.Runtime;
using Data.Runtime;
using ScriptableEvent.Runtime;
using UnityEngine;
using Utils.Runtime;

namespace Traps.Runtime
{
    public class Hammer : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private float _cooldown;
        [SerializeField] private float _onGroundDuration;
        [SerializeField] private float _movementDownDuration;
        [SerializeField] private float _movementUpDuration;

        [SerializeField] private Transform _mesh;

        [SerializeField] private ParticleSystem[] _hitEffects;

        [SerializeField] private GameEvent _onRoundFinished;

        [SerializeField] private SFXData _onHitSfx;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onRoundFinished.UnregisterListener(OnRoundFinished);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onRoundFinished.RegisterListener(OnRoundFinished);

            _movementDownTimer = new(_movementDownDuration, OnMovementDownTimerOver);
            _movementDownTimer.OnValueChanged += OnMovementDownTimerChanged;

            _onGroundTimer = new(_onGroundDuration, OnGroundTimerOver);
            _movementUpTimer = new(_movementUpDuration, OnMouvementUpTimerOver);

            _cooldownTimer = new(_cooldown, OnCooldownTimerOver);
            _movementUpTimer.OnValueChanged += OnMovementUpTimerChanged;

            _upQuat = Quaternion.Euler(Vector3.zero);
            _downQuat = Quaternion.Euler(0, 0, 90);
        }

        public void DownHammer()
        {
            if (_roundFinished) return;
            if (_onCooldown) return;
            _onCooldown = true;
            _movementDownTimer.Start();
        }

        #endregion


        #region Utils & Tools

        private void OnMovementDownTimerOver()
        {
            PlayHitEffect();
            AudioManager.s_instance.PlaySfx(_onHitSfx._sfx.GetRandom(), false);
            _onGroundTimer.Start();
        }

        private void OnMouvementUpTimerOver()
        {
            _cooldownTimer.Start();
        }

        private void OnGroundTimerOver()
        {
            _movementUpTimer.Start();
        }

        private void OnCooldownTimerOver()
        {
            _onCooldown = false;
        }

        private void OnMovementDownTimerChanged(float value)
        {
            if (value < 0) value = 0;

            float remainingTime = _movementDownDuration - value;
            float ratio = Mathf.Clamp01(remainingTime / _movementDownDuration);

            Quaternion quat = Quaternion.Lerp(_upQuat, _downQuat, ratio);

            SetRotation(quat);
        }

        private void OnMovementUpTimerChanged(float value)
        {
            float remainingTime = _movementDownDuration - value;
            float ratio = remainingTime / _movementDownDuration;

            Quaternion quat = Quaternion.Lerp(_downQuat, _upQuat, ratio);

            SetRotation(quat);
        }

        private void OnRoundFinished()
        {
            _roundFinished = true;
        }

        private void SetRotation(Quaternion quat)
        {
            _mesh.transform.localRotation = quat;
        }

        private void PlayHitEffect()
        {
            foreach (var effect in _hitEffects)
            {
                effect.Play();
            }
        }

        #endregion


        #region Private

        private Timer _movementDownTimer;
        private Timer _movementUpTimer;

        private Timer _cooldownTimer;
        private Timer _onGroundTimer;

        private bool _onCooldown;
        private bool _roundFinished;

        private Quaternion _downQuat;
        private Quaternion _upQuat;

        #endregion
    }
}