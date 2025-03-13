using Inputs.Runtime;
using ScriptableEvent.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Feedbacks.Runtime
{
    public class RumbleManager : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onPauseMenuDisplayed;
        [SerializeField] private GameEvent _onPauseMenuHidden;

        #endregion

        #region Unity API

        private void Awake()
        {
            _onPauseMenuDisplayed.RegisterListener(OnPauseMenuDisplayed);
            _onPauseMenuHidden.RegisterListener(OnPauseMenuHidden);
        }

        private void OnDestroy()
        {
            _onPauseMenuDisplayed.UnregisterListener(OnPauseMenuDisplayed);
            _onPauseMenuHidden.UnregisterListener(OnPauseMenuHidden);

            _gamepad?.ResetHaptics();
        }

        private void OnDisable()
        {
            _gamepad?.ResetHaptics();
        }

        #endregion


        #region Main

        public void Setup()
        {
            foreach (var d in GetComponent<InputsReader>().GetCurrentDevices())
            {
                if (d is Gamepad)
                {
                    _gamepad = (Gamepad)d;
                    break;
                }
            }
        }

        public Gamepad GetCurrentGamepad() => _gamepad;

        public void Rumble(RumbleData data)
        {
            if (_gamepad == null) return;
            StopAllCoroutines();
            StartCoroutine(RumbleRoutine(data.m_lowFrequency, data.m_highFrequency, data.m_duration));
        }

        private IEnumerator RumbleRoutine(float lowFrequency, float highFrequency, float duration)
        {
            if (_gamepad != null)
            {
                _gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
                yield return new WaitForSeconds(duration);
                _gamepad.SetMotorSpeeds(0, 0);
            }
        }

        #endregion


        #region Utils & Tools

        private void OnPauseMenuDisplayed()
        {
            _gamepad?.PauseHaptics();
        }

        private void OnPauseMenuHidden()
        {
            _gamepad?.ResumeHaptics();
        }

        #endregion


        #region Private

        private Gamepad _gamepad;

        #endregion
    }
}