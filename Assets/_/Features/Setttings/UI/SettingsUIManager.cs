using SceneManager.runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Settings.UI
{
    public class SettingsUIManager : MenuBase
    {
        #region Exposed

        [SerializeField] private GameObject _quitBtn;
        [SerializeField] private GameObject _backBtn;
        [SerializeField] private GameObject _confirmationScreen;

        [SerializeField] private GameObject _firstSelectedGO;

        [SerializeField] private string _discordInviteLink;

        [SerializeField][BoxGroup("Events")] private GameEvent _onSettingsMenuDisplayed;
        [SerializeField][BoxGroup("Events")] private GameEvent _onSettingsMenuHidden;
        [SerializeField][BoxGroup("Events")] private GameEvent _onSubSettingsMenuDisplayed;
        [SerializeField][BoxGroup("Events")] private GameEvent _onSubSettingsMenuHidden;

        [SerializeField][BoxGroup("Menu")] private GameObject _choiceMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _controlsMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _audioMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _graphicsMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _languageMenu;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            _onSettingsMenuDisplayed.Raise();
            base.OnEnable();

            if (SceneLoader.s_instance.PauseMenuIsLoaded())
            {
                _quitBtn.SetActive(false);
                _backBtn.SetActive(true);
            }
            else
            {
                _quitBtn.SetActive(true);
                _backBtn.SetActive(false);
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _onSettingsMenuHidden.Raise();
        }

        #endregion


        #region Main

        public void BackToSelectionMenu(GameObject caller)
        {
            caller.SetActive(false);
            this.EnableInput();
            _choiceMenu.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);

            _onSubSettingsMenuHidden?.Raise();
        }

        public void OpenControls()
        {
            _onSubSettingsMenuDisplayed?.Raise();
            _choiceMenu.SetActive(false);
            this.DisableInput();
            _controlsMenu.SetActive(true);
        }

        public void OpenAudio()
        {
            _onSubSettingsMenuDisplayed?.Raise();
            _choiceMenu.SetActive(false);
            this.DisableInput();
            _audioMenu.SetActive(true);
        }

        public void OpenGraphics()
        {
            _onSubSettingsMenuDisplayed?.Raise();
            _choiceMenu.SetActive(false);
            this.DisableInput();
            _graphicsMenu.SetActive(true);
        }

        public void OpenLanguage()
        {
            _onSubSettingsMenuDisplayed?.Raise();
            _choiceMenu.SetActive(false);
            this.DisableInput();
            _languageMenu.SetActive(true);
        }

        public void OpenDiscord()
        {
            if (string.IsNullOrWhiteSpace(_discordInviteLink)) return;
            Application.OpenURL(_discordInviteLink);
        }

        #endregion


        #region UI navigation

        public override void OnCancel_Performed(int playerId)
        {
            if (SceneLoader.s_instance.PauseMenuIsLoaded())
            {
                SceneLoader.s_instance.UnloadSettingsMenu();
                return;
            }
                ShowQuitGameMenu();
        }

        #endregion


        #region Utils & Tools

        public void HideQuitGameMenu()
        {
            _confirmationScreenDisplayed = false;
            _confirmationScreen.SetActive(false);

            this.EnableInput();
            _choiceMenu.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);
        }

        public void ShowQuitGameMenu()
        {
            if (!_confirmationScreenDisplayed)
            {
                this.DisableInput();
                _confirmationScreenDisplayed = true;
                _confirmationScreen.SetActive(true);

                _choiceMenu.SetActive(false);
            }
        }

        public void BackToLastMenu()
        {
            SceneLoader.s_instance.UnloadSettingsMenu();
        }

        #endregion


        #region Private

        private bool _confirmationScreenDisplayed;

        #endregion
    }
}