using SceneManager.runtime;
using ScriptableEvent.Runtime;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UserInterface.Runtime
{
    public class PauseMenu : MenuBase
    {
        #region Exposed

        [SerializeField] private GameObject _confirmationUI;
        [SerializeField] private GameObject _buttons;
        [SerializeField] private GameObject _firstSelected;
        [SerializeField] private GameEvent _backToMainMenu;
        [SerializeField] private GameObject _firstSelectedInConfirmationMenu;
        [SerializeField] private GameObject _pauseMenu;

        [SerializeField] private GameEvent _onSettingsMenuDisplayed;
        [SerializeField] private GameEvent _onSettingsMenuHidden;

        [SerializeField] private GameEvent _onPauseMenuDisplayed;
        [SerializeField] private GameEvent _onPauseMenuHidden;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            _onPauseMenuDisplayed.Raise();
            base.OnEnable();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelected);

            Time.timeScale = 0f;
        }

        protected override void OnDisable()
        {
            Time.timeScale = 1f;
            base.OnDisable();
        }

        private void Awake()
        {
            _onSettingsMenuDisplayed.RegisterListener(OnSettingsDisplayed);
            _onSettingsMenuHidden.RegisterListener(OnSettingsHidden);
        }

        private void OnDestroy()
        {
            _onSettingsMenuDisplayed.UnregisterListener(OnSettingsDisplayed);
            _onSettingsMenuHidden.UnregisterListener(OnSettingsHidden);
        }

        #endregion


        #region Main

        public void Resume()
        {
            if (_settingsMenuRequested) return;
            _onPauseMenuHidden.Raise();
            SceneLoader.s_instance.UnloadPauseMenu();
        }

        public void OpenSettingsMenu()
        {
            if (_settingsMenuRequested) return;
            _settingsMenuRequested = true;
            EventSystem.current.SetSelectedGameObject(null);
            SceneLoader.s_instance.LoadSettingsMenu();
        }

        public void BackToMainMenu() => ToggleConfirmationUI();

        public void ConfirmBackInMainMenu()
        {
            _backToMainMenu?.Raise();
        }

        public void CancelBackInMainMenu() => ToggleConfirmationUI();

        private void ToggleConfirmationUI()
        {
            if (!_confirmationUI.activeInHierarchy && _settingsMenuRequested) return;
            _confirmationUI.SetActive(!_confirmationUI.activeInHierarchy);

            EventSystem.current.SetSelectedGameObject(null);

            if (_confirmationUI.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(_firstSelectedInConfirmationMenu);
                _buttons.SetActive(false);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(_firstSelected);
                _buttons.SetActive(true);
            }
        }

        #endregion


        #region Utils & Tools

        public override void DisableInput()
        {
            base.DisableInput();
            _pauseMenu.SetActive(false);
        }

        public override void EnableInput()
        {
            base.EnableInput();
            _pauseMenu.SetActive(true);
        }

        private void OnSettingsDisplayed()
        {
            DisableInput();
        }

        private void OnSettingsHidden()
        {
            EnableInput();
            _settingsMenuRequested = false;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelected);
        }

        public override void OnMenu_Performed(int playerId)
        {
            Resume();
        }

        public override void OnCancel_Performed(int playerId)
        {
            if (_confirmationUI.activeInHierarchy)
            {
                _confirmationUI.SetActive(false);
                _buttons.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstSelected);
            }
            else
            {
                Resume();
            }
        }

        #endregion


        #region Private

        private bool _settingsMenuRequested;

        #endregion
    }
}