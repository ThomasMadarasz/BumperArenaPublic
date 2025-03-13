using Archi.Runtime;
using Mirror;
using ScriptableEvent.Runtime;
using UnityEngine;
using Utils.Runtime;

namespace Traps.Runtime
{
    public class SwitchBumper : CBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEvent _onRoundEnd;

        [SerializeField] private float _killZoneMinRadius;
        [SerializeField] private float _killZoneMaxRadius;
        [SerializeField] private float _transitionDuration;
        [SerializeField] private float _modeDuration;

        [SerializeField] private CapsuleCollider _killZone;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        [ServerCallback]
        private void Setup()
        {
            _modeTimer = new(_modeDuration, OnModeDurationTimerOver);
            _transitionTimer = new(_transitionDuration, OnTransitionTimerOver);
            _transitionTimer.OnValueChanged += OnTransitionTimerChanged;

            _onRoundStart.RegisterListener(OnRoundStart);
            _onRoundEnd.RegisterListener(OnRoundEnd);
        }

        private void OnRoundStart()
        {
            _modeTimer.Start();
            _killZone.gameObject.SetActive(_onKillMode);
        }

        private void OnRoundEnd()
        {
            _modeTimer.Stop();
            _transitionTimer.Stop();
        }

        #endregion


        #region Utils & Tools

        private void OnModeDurationTimerOver()
        {
            _onKillMode = !_onKillMode;
            _killZone.gameObject.SetActive(_onKillMode);
            _transitionTimer.Start();
        }

        private void OnTransitionTimerOver()
        {
            _modeTimer.Start();
        }

        private void OnTransitionTimerChanged(float value)
        {
            float size;
            float remainingTime = _transitionDuration - value;
            float ratio = remainingTime / _transitionDuration;

            if (_onKillMode)
            {
                size = Mathf.Lerp(_killZoneMinRadius, _killZoneMaxRadius, ratio);
            }
            else
            {
                size = Mathf.Lerp(_killZoneMaxRadius, _killZoneMinRadius, ratio);
            }

            _killZone.radius = size;
        }

        #endregion


        #region Debug
        #endregion


        #region Private

        private bool _onKillMode;

        private Timer _modeTimer;
        private Timer _transitionTimer;

        #endregion
    }
}