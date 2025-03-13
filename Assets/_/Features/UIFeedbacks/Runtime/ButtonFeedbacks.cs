using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFeedbacks.Runtime
{
    [RequireComponent(typeof(Button))]
    public class ButtonFeedbacks : SelectedFeedbacks
    {
        #region Exposed
        #endregion


        #region Unity API
        private void Reset()
        {
            base.Reset();
            _button = GetComponent<Button>();
        }

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            base.Setup();
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(base.ResetFeedbacks);
        }

        #endregion


        #region Utils & Tools


        #endregion


        #region Debug
        #endregion


        #region Private

        private Button _button;

        #endregion
    }
}