using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UIFeedbacks.Runtime
{
    [RequireComponent(typeof(Toggle))]
    public class CustomToggle : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Toggle _toggle;

        [SerializeField] private UnityEvent _onTogglePassesTrue;
        [SerializeField] private UnityEvent _onTogglePassesFalse;

        #endregion


        #region Unity API

        private void Reset()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void Awake() => Setup();

        private void OnEnable()
        {
            if (_toggle.isOn) _onTogglePassesTrue.Invoke();
            else _onTogglePassesFalse.Invoke();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _toggle.onValueChanged.AddListener(delegate
            {
                OnValueChanged(_toggle);
            });
        }

        private void OnValueChanged(Toggle toggle)
        {
            if (toggle.isOn == true) _onTogglePassesTrue.Invoke();
            else _onTogglePassesFalse.Invoke();
        }

        public void PlayFeedbackValue(bool value)
        {
            if (value) _onTogglePassesTrue.Invoke();
            else _onTogglePassesFalse.Invoke();
        }

        #endregion
    }
}