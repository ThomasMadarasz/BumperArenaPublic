using Archi.Runtime;
using UnityEngine;
using Utils.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Feedbacks.Runtime;
using Data.Runtime;
using Audio.Runtime;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class BoostPack : CBehaviour
    {
        #region Exposed

        [SerializeField][Min(0)] private float _cooldown;
        [SerializeField][Min(1)] private int _boostQuantity;

        [SerializeField] private GameEventT _onBoostPackIntake;
        [SerializeField] private GameEvent _onRoundStarted;

        [SerializeField] private Renderer _renderer;
        [SerializeField] private ParticleSystem _vfx;

        [SerializeField] private SFXData _onTakeBoostSfx;

        [SerializeField] private Animator _animator;

        #endregion


        #region Unity API

        private void Awake()
        {
            _onRoundStarted.RegisterListener(OnRoundstarted);

            _mpb = new();
            _timer = new(_cooldown, OnTimerOver);
            _timer.OnValueChanged += OnTimerValueChanged;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_roundStarted) return;
            if (_onCooldown) return;

            IBoostable boostable = other.GetComponent<IBoostable>();
            if (boostable == null) return;

            if (!boostable.CanReceiveBoost()) return;

            AddBoostToPlayer(boostable, other.gameObject);
        }

        private void OnDestroy()
        {
            _onRoundStarted.UnregisterListener(OnRoundstarted);
            _timer.Stop();
        }

        #endregion


        #region Main

        private void AddBoostToPlayer(IBoostable boostable, GameObject go)
        {
            _onCooldown = true;
            boostable.AddBoost(_boostQuantity);
            _timer.Start();

            int id = go.GetComponent<IFeedback>().GetID();

            bool isFullBoost = _boostQuantity > 1;
            int availableBoost = boostable.GetAvailableBoostChargeCount();

            _vfx.gameObject.SetActive(false);
            _vfx.Stop();

            FeedbackParameters param = new() { m_id = id, m_params = new object[2] { isFullBoost, availableBoost } };
            _onBoostPackIntake.Raise(param);

            AudioManager.s_instance.PlaySfx(_onTakeBoostSfx._sfx.GetRandom(), false);
        }

        #endregion


        #region Utils & Tools

        private void OnRoundstarted()
        {
            _roundStarted = true;
            if (_animator != null) _animator.Play("anim_BoosterMovingUp");
        }

        private void OnTimerOver()
        {
            _vfx.gameObject.SetActive(true);
            _vfx.Play();
            _onCooldown = false;
        }

        public float GetRemainingTime() => _remainingTime;

        private void OnTimerValueChanged(float value)
        {
            float val = _remainingTime / _cooldown;
            val /= 2;
            val = 0.5f - val;

            _remainingTime = value;
            _renderer.GetPropertyBlock(_mpb, 0);
            _mpb.SetFloat("_UVCoordinate", val);
            _renderer.SetPropertyBlock(_mpb, 0);
        }

        #endregion


        #region Private

        private Timer _timer;
        private MaterialPropertyBlock _mpb;
        private bool _onCooldown;
        private bool _roundStarted;
        private float _remainingTime;

        #endregion
    }
}