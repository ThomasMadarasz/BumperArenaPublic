using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace Feedbacks.Runtime
{
    public class SpecificGamepadFeedbacks : MonoBehaviour
    {
        #region Exposed
        #endregion


        #region Unity API

        private void OnDestroy()
        {
            _gamepad?.SetLightBarColor(Color.white);
        }

        private void OnDisable()
        {
            _gamepad?.SetLightBarColor(Color.white);
        }

        #endregion


        #region Main

        public void Setup(Gamepad gamepad)
        {
            if (gamepad is DualShockGamepad)
            {
                _gamepad = (DualShockGamepad)gamepad;
            }
        }

        public void UpdateLightBarColor(Color color)
        {
            if (_gamepad == null) return;
            _gamepad.SetLightBarColor(color);
        }

        #endregion

        #region Private

        private DualShockGamepad _gamepad;

        #endregion
    }
}