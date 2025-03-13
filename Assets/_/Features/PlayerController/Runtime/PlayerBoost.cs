using UnityEngine;
using Archi.Runtime;
using Utils.Runtime;
using PlayerController.Data.Runtime;
using ScriptableEvent.Runtime;
using Inputs.Runtime;
using Interfaces.Runtime;
using Feedbacks.Runtime;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(PlayerProperties))]
    public class PlayerBoost : CNetBehaviour, IBoostable
    {
        #region Exposed

        [SerializeField] private GameEvent _onGameStarted;
        [SerializeField] private GameEventT _onGaugeBoostChanged;
        [SerializeField] private GameEvent _onFeedbacksInitialized;
        [SerializeField] private GameEventT _resetFeedback;
        [SerializeField] private GameEventT _onBoostStarted;
        [SerializeField] private GameEventT _onBoostFinished;

        [SerializeField] private GameEvent _onRoundStarted;
        [SerializeField] private GameEvent _onRoundFinished;

        [SerializeField] private Material _boostMat;
        [SerializeField] private Renderer _boostRenderer;
        [SerializeField] private RumbleData _onTakeBoostData;

        [SerializeField] private float _noBoostSoundTime;

        public bool m_isBoostAvailable
        {
            get { return CanBoost(); }
        }

        public IBoostable m_boostable
        {
            get { return this; }
        }

        #endregion


        #region Unity API

        private void Awake()
        {
            _p = GetComponent<PlayerProperties>();
            _playerFeedback = GetComponent<PlayerFeedbacks>();
            _boostRenderer.material = Instantiate(_boostMat);

            _noBoostSfxTimer = new(_noBoostSoundTime, OnNoBoostTimerOver);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            SetValues(_p.m_boostData);

            _onRoundStarted.RegisterListener(OnRoundStarted);
            _onRoundFinished.RegisterListener(OnRoundFinished);
            _onFeedbacksInitialized.RegisterListener(SetupFeedbacks);
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            _inputsReader = _p.m_inputs;
            _inputsReader.m_onBoostPerformed += TryToBoost;

            IKillable killable = GetComponent<IKillable>();
            if (killable != null)
            {
                killable.m_onPlayerDie += ResetValues;
                killable.m_onPlayerRespawn += OnRespawn;
            }
        }

        private void OnDestroy()
        {
            _recoveryTimer.Stop();

            _onRoundStarted.UnregisterListener(OnRoundStarted);
            _onRoundFinished.UnregisterListener(OnRoundFinished);
            _onFeedbacksInitialized.UnregisterListener(SetupFeedbacks);

            _inputsReader.m_onBoostPerformed -= TryToBoost;

            IKillable killable = GetComponent<IKillable>();
            if (killable != null)
            {
                killable.m_onPlayerDie -= ResetValues;
                killable.m_onPlayerRespawn -= OnRespawn;
            }
        }

        #endregion


        #region Main

        private void SetupFeedbacks()
        {
            _feedbackId = GetComponent<IFeedback>().GetID();

            _availableBoostCount = GetAvailableBoostChargeCount();

            float matValue = _currentBoost / _p.m_boostData.m_maxValue;
            matValue /= 2;

            FeedbackParameters param = new() { m_id = _feedbackId, m_params = new object[2] { _availableBoostCount, matValue } };
            _resetFeedback?.Raise(param);
            _onGaugeBoostChanged?.Raise(param);
        }


        public void TryToBoost()
        {
            if (CanBoost()) Boost();
            else
            {
                if (_noBoostSoundUsed) return;
                _noBoostSoundUsed = true;

                _noBoostSfxTimer.Start();

                _playerFeedback.PlayNoBoostSfx();
            }
        }
        private void Boost()
        {
            _p.i_isBoosted = true;
            _isOnCooldown = true;
            _currentBoost -= _cost;
            _boostTimer.Start();
            _coolTimer.Start();

            FeedbackParameters boostParam = new() { m_id = _feedbackId };
            _onBoostStarted.Raise(boostParam);

            _availableBoostCount = GetAvailableBoostChargeCount();

            float matValue = _currentBoost / _p.m_boostData.m_maxValue;
            matValue /= 2;

            FeedbackParameters param = new() { m_id = _feedbackId, m_params = new object[2] { _availableBoostCount, matValue } };
            _onGaugeBoostChanged?.Raise(param);
        }

        private void IncrementBoostValue(float amount)
        {
            _currentBoost = Mathf.Clamp(_currentBoost + amount, 0, _p.m_boostData.m_maxValue);

            int count = GetAvailableBoostChargeCount();
            _availableBoostCount = count;

            float matValue = _currentBoost / _p.m_boostData.m_maxValue;
            matValue /= 2;

            FeedbackParameters param = new() { m_id = _feedbackId, m_params = new object[2] { GetAvailableBoostChargeCount(), matValue } };
            _onGaugeBoostChanged?.Raise(param);
        }

        private void SetValues(PlayerBoostData data)
        {
            _maxBoostCharge = data.m_maxBoostsNumber;

            _boostEnable = true;
            _currentBoost = _p.m_boostData.m_defaultValue;
            _cost = (float)data.m_maxValue / (float)data.m_maxBoostsNumber;
            _boostRecoveryValue = (float)_cost / 100f * (float)data.m_naturalRecoveryInPercent;
            _boostTimer = new(data.m_duration, OnBoostTimerOver);
            _coolTimer = new(data.m_cooldown, OnCooldownTimerOver);
            _recoveryTimer = new(data.m_naturalRecoveryCooldownInSeconds, OnRecoveryTimerOver, true);
        }

        public void AddBoost(int boost)
        {
            IncrementBoostValue(_cost * boost);
            _playerFeedback.PlayRumble(_onTakeBoostData);
        }

        #endregion


        #region Utils & Tools

        private void OnNoBoostTimerOver()
        {
            _noBoostSoundUsed = false;
        }

        private void OnRoundStarted() => _recoveryTimer.Start();
        private void OnRoundFinished() => _recoveryTimer.Stop();

        private void OnBoostTimerOver()
        {
            _p.i_isBoosted = false;
            FeedbackParameters boostParam = new() { m_id = _feedbackId };
            _onBoostFinished.Raise(boostParam);
        }

        private void OnCooldownTimerOver() => _isOnCooldown = false;

        private void OnRecoveryTimerOver() => IncrementBoostValue(_boostRecoveryValue);

        private void ResetValues()
        {
            _boostTimer.Stop();
            _coolTimer.Stop();
            _recoveryTimer.Stop();
            _noBoostSfxTimer.Stop();

            _p.i_isBoosted = false;
            _isOnCooldown = false;
            _boostEnable = true;
            _noBoostSoundUsed = false;

            _currentBoost = _p.m_boostData.m_defaultValue;

            float matValue = _currentBoost / _p.m_boostData.m_maxValue;
            matValue /= 2;

            FeedbackParameters param = new() { m_id = _feedbackId, m_params = new object[2] { GetAvailableBoostChargeCount(), matValue } };
            _onGaugeBoostChanged?.Raise(param);
        }

        private void OnRespawn()
        {
            _recoveryTimer.Start();
        }

        public void DisableBoost() => _boostEnable = false;

        public void EnableBoost() => _boostEnable = true;

        private bool CanBoost()
        {
            if (!_boostEnable) return false;
            if (!_p.i_isAlive || !_p.i_isGameStarted || _p.i_isGameOver || _p.i_isBumped || _p.i_isInvincible) return false;
            if (!_isOnCooldown && _currentBoost >= _cost) return true;
            return false;
        }

        public int GetAvailableBoostChargeCount()
        {
            float currentBoost = _currentBoost;
            currentBoost = currentBoost - (currentBoost % _cost);

            return Mathf.RoundToInt(currentBoost / _cost);
        }

        public bool CanReceiveBoost()
        {
            return GetAvailableBoostChargeCount() < _maxBoostCharge;
        }

        #endregion


        #region Private

        private PlayerProperties _p;
        private InputsReader _inputsReader;
        private PlayerFeedbacks _playerFeedback;

        private bool _isOnCooldown;
        private bool _boostEnable = true;
        private bool _noBoostSoundUsed;

        private int _availableBoostCount;
        private int _feedbackId;
        private int _maxBoostCharge;

        private float _currentBoost;
        private float _cost;
        private float _boostRecoveryValue;

        private Timer _boostTimer;
        private Timer _coolTimer;
        private Timer _recoveryTimer;
        private Timer _noBoostSfxTimer;

        #endregion
    }
}