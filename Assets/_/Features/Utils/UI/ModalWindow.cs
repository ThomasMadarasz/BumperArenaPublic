using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Utils.Runtime;
using System;
using UnityEngine.EventSystems;

namespace Utils.UI
{
    public class ModalWindow : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _questionText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [SerializeField] private Button _yesBtn;
        [SerializeField] private Button _noBtn;

        [SerializeField] private GameObject _ui;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            _ui.SetActive(false);

            EventSystem.current.sendNavigationEvents = false;
            EventSystem.current.SetSelectedGameObject(null);            

            Invoke(nameof(EnableNavigationEvent), 0.1f);
            Invoke(nameof(SetSelectedObject), 0.11f);
        }

        private void DestroyModal()
        {
            Destroy(gameObject);
        }

        private void UpdateTimerUI(float time)
        {
            _timerText.text = $"{Mathf.RoundToInt(time)}";
        }

        public void ExternalSetup(string question, float time, Action validateAction, Action unvalidateAction)
        {
            _questionText.text = question;
            _timerText.text = time.ToString();

            this._unvalidateAction = unvalidateAction;
            this._validateAction = validateAction;

            _timer = new(time, UnvalidateAction);
            _timer.OnValueChanged += UpdateTimerUI;

            _yesBtn.onClick.AddListener(() =>
            {
                ValidateAction();
            });

            _noBtn.onClick.AddListener(() =>
            {
                UnvalidateAction();
            });

            _ui.SetActive(true);
            _timer.Start();
        }


        public void ValidateAction()
        {
            _timer.Stop();
            _validateAction?.Invoke();
            DestroyModal();
        }

        public void UnvalidateAction()
        {
            _timer.Stop();
            _unvalidateAction?.Invoke();
            DestroyModal();
        }
        private void EnableNavigationEvent()=> EventSystem.current.sendNavigationEvents = true;

        private void SetSelectedObject() => EventSystem.current.SetSelectedGameObject(_yesBtn.gameObject);

        #endregion


        #region Private

        private UnscaledTimer _timer;
        private Action _unvalidateAction;
        private Action _validateAction;

        #endregion
    }
}