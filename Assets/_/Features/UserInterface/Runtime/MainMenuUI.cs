using Data.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using SceneManager.runtime;
using UINavigation.Runtime;
using Utils.Runtime;
using CameraSwap.Runtime;
using LocalPlayer.Runtime;
using LoadingScreen.Runtime;
using ScriptableEvent.Runtime;
using MoreMountains.Feedbacks;

namespace UserInterface.Runtime
{
    public class MainMenuUI : MenuBase
    {
        #region Exposed

        [BoxGroup("Socials")][SerializeField] private string _discordInviteLink;
        [BoxGroup("Socials")][SerializeField] private string _youtubeChannelLink;
        [BoxGroup("Socials")][SerializeField] private string _remotePlayLink;

        [BoxGroup("Data")][SerializeField] private PlayerSteamData _playerSteamData;

        [SerializeField][BoxGroup("Scenes")] private string _settingsSceneName;
        [SerializeField][BoxGroup("Scenes")] private MMF_Player _settingsSceneFeedbacks;
        [SerializeField][BoxGroup("Scenes")] private string[] _mainMenuScenes;

        [SerializeField][BoxGroup("Menu")] private GameObject _playMenu;
        [SerializeField][BoxGroup("Menu")] private MMF_Player _playMenuFeedbacks;
        [SerializeField][BoxGroup("Menu")] private GameObject _progressionMenu;
        [SerializeField][BoxGroup("Menu")] private MMF_Player _progressionMenuFeedbacks;
        [SerializeField][BoxGroup("Menu")] private GameObject _garageMenu;
        [SerializeField][BoxGroup("Menu")] private MMF_Player _garageMenuFeedbacks;
        [SerializeField][BoxGroup("Menu")] private GameObject _socialsUI;
        [SerializeField][BoxGroup("Menu")] private GameObject _localPlayerManager;
        [SerializeField][BoxGroup("Menu")] private GameObject _howToPlayMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _howToPlayButton;
        [SerializeField][BoxGroup("Menu")] private GameObject _topMenu;

        [SerializeField][BoxGroup("Events")] private GameEvent _onSubSettingsMenuDisplayed;
        [SerializeField][BoxGroup("Events")] private GameEvent _onSubSettingsMenuHidden;

        [SerializeField] private Button _playButton;
        [SerializeField] private Image _holdPlayImg;
        [SerializeField] private GameEvent _onFadeOut;


        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            if (_onSubSettingsMenuDisplayed != null) _onSubSettingsMenuDisplayed.UnregisterListener(OnSubSettinsMenuDisplayed);
            if (_onSubSettingsMenuHidden != null) _onSubSettingsMenuHidden.UnregisterListener(OnSubSettinsMenuHidden);
        }

        #endregion


        #region Main

        public void TogglePlayerManager()
        {
            if (_isHoldingPlay) return;
            _localPlayerManager.SetActive(!_localPlayerManager.activeInHierarchy);
            if (!_localPlayerManager.activeInHierarchy) UINavigationManager.s_instance.EnableMenu(this);
            else UINavigationManager.s_instance.DisableMenu(this);
        }

        public void ToggleHowToPlay()
        {
            if (_isHoldingPlay) return;
            _howToPlayMenu.SetActive(!_howToPlayMenu.activeInHierarchy);
            if (!_howToPlayMenu.activeInHierarchy)
            {
                EnableInput();
                _playMenu.SetActive(true);
                _socialsUI.SetActive(true);
                _howToPlayButton.SetActive(true);
            }
            else
            {
                DisableInput();
                _playMenu.SetActive(false);
                _socialsUI.SetActive(false);
                _howToPlayButton.SetActive(false);
            }
        }

        private void Setup()
        {
            if (_onSubSettingsMenuDisplayed != null) _onSubSettingsMenuDisplayed.RegisterListener(OnSubSettinsMenuDisplayed);
            if (_onSubSettingsMenuHidden != null) _onSubSettingsMenuHidden.RegisterListener(OnSubSettinsMenuHidden);

            int index = Utility.m_rng.Next(0, _mainMenuScenes.Length);
            SceneLoader.s_instance.LoadSceneAsync(_mainMenuScenes[index], OnSubMainMenuOpen);

            if (LoadingScreenManager.s_instance.IsLoadingScreenDisplayed())
                _onFadeOut.Raise();
        }

        public void OpenDiscord()
        {
            if (string.IsNullOrWhiteSpace(_discordInviteLink)) return;
            Application.OpenURL(_discordInviteLink);
        }

        public void OpenYoutube()
        {
            if (string.IsNullOrWhiteSpace(_youtubeChannelLink)) return;
            Application.OpenURL(_youtubeChannelLink);
        }

        public void OpenRemotePlay()
        {
            if (string.IsNullOrWhiteSpace(_remotePlayLink)) return;
            Application.OpenURL(_remotePlayLink);
        }

        public void ToggleSettingsTab()
        {
            if (_settingsScreenDisplayed) return;
            if (_onTransition) return;
            _onTransition = true;
            ChangeTabPage(TabPageMenu.Settings);
            _settingsScreenDisplayed = true;
        }

        public void TogglePlayTab()
        {
            if (_onTransition) return;
            _onTransition = true;
            ChangeTabPage(TabPageMenu.Play);
        }

        public void ToggleGarageTab()
        {
            if (_onTransition) return;
            _onTransition = true;
            ChangeTabPage(TabPageMenu.Garage);
        }

        //public void ToggleProgressionTab()
        //{
        //    if (_onTransition) return;
        //    _onTransition = true;
        //    ChangeTabPage(TabPageMenu.Progression);
        //}

        #endregion


        #region Utils & Tools

        private void OnSubSettinsMenuDisplayed()
        {
            DisableInput();
        }

        private void OnSubSettinsMenuHidden()
        {
            EnableInput();
        }

        private void OnSubMainMenuOpen() => _subMainMenuOpen = true;

        public override void DisableInput()
        {
            base.DisableInput();
            ShowHideTopMenu(false);
        }

        public override void EnableInput()
        {
            base.EnableInput();
            ShowHideTopMenu(true);
        }

        private void ShowHideTopMenu(bool visible)
        {
            _topMenu.SetActive(visible);
        }

        private void ChangeToNextTabPage()
        {
            if (_onTransition) return;
            _onTransition = true;
            _currentTabPageIndex++;
            if (_currentTabPageIndex >= 4) _currentTabPageIndex = 1;

            ChangeTabPage((TabPageMenu)_currentTabPageIndex);
        }

        private void ChangeToPreviousTabPage()
        {
            if (_onTransition) return;
            _onTransition = true;
            _currentTabPageIndex--;
            if (_currentTabPageIndex <= 0) _currentTabPageIndex = 3;

            ChangeTabPage((TabPageMenu)_currentTabPageIndex);
        }

        private void ChangeTabPage(TabPageMenu tabPage)
        {
            if (_isHoldingPlay) return;
            if (_currentDisplayedMenu == tabPage)
            {
                _onTransition = false;
                return;
            }
            HideTabPageMenu(_currentDisplayedMenu);
            _currentDisplayedMenu = tabPage;
            ShowTabPageMenu(_currentDisplayedMenu);

            CameraSwapManager.s_instance.ChangeToCamera((int)_currentDisplayedMenu - 1, OnTransitionFinished);
            if (_currentDisplayedMenu == TabPageMenu.Garage) LocalPlayerManager.s_instance.UpdatePlayerPositionForGarage();
            else LocalPlayerManager.s_instance.UpdatePlayerPositionForBasePosition();
        }

        private void OnTransitionFinished()
        {
            _onTransition = false;
        }

        private void HideTabPageMenu(TabPageMenu tabPage)
        {
            switch (tabPage)
            {
                case TabPageMenu.Play:

                    _playMenu.SetActive(false);
                    _howToPlayButton.SetActive(false);
                    _socialsUI.SetActive(false);
                    _playMenuFeedbacks.Direction = MMFeedbacks.Directions.BottomToTop;
                    _playMenuFeedbacks.PlayFeedbacks();
                    break;
                case TabPageMenu.Garage:
                    _garageMenu.SetActive(false);
                    _garageMenuFeedbacks.Direction = MMFeedbacks.Directions.BottomToTop;
                    _garageMenuFeedbacks.PlayFeedbacks();
                    break;
                //case TabPageMenu.Progression:
                //    _progressionMenu.SetActive(false);
                //    _progressionMenuFeedbacks.Direction = MMFeedbacks.Directions.BottomToTop;
                //    _progressionMenuFeedbacks.PlayFeedbacks();
                //    break;
                case TabPageMenu.Settings:
                    SceneLoader.s_instance.UnloadSceneAsync(_settingsSceneName, OnSettingsSceneUnloaded);
                    _settingsSceneFeedbacks.Direction = MMFeedbacks.Directions.BottomToTop;
                    _settingsSceneFeedbacks.PlayFeedbacks();
                    break;
            }
        }

        private void ShowTabPageMenu(TabPageMenu tabPage)
        {
            switch (tabPage)
            {
                case TabPageMenu.Play:
                    _playMenu.SetActive(true);
                    _howToPlayButton.SetActive(true);
                    _socialsUI.SetActive(true);
                    _playMenuFeedbacks.Direction = MMFeedbacks.Directions.TopToBottom;
                    _playMenuFeedbacks.PlayFeedbacks();
                    break;
                case TabPageMenu.Garage:
                    _garageMenu.SetActive(true);
                    _garageMenuFeedbacks.Direction = MMFeedbacks.Directions.TopToBottom;
                    _garageMenuFeedbacks.PlayFeedbacks();
                    break;
                //case TabPageMenu.Progression:
                //    _progressionMenu.SetActive(true);
                //    _progressionMenuFeedbacks.Direction = MMFeedbacks.Directions.TopToBottom;
                //    _progressionMenuFeedbacks.PlayFeedbacks();
                //    break;
                case TabPageMenu.Settings:
                    SceneLoader.s_instance.LoadSceneAsync(_settingsSceneName, null);
                    _settingsSceneFeedbacks.Direction = MMFeedbacks.Directions.TopToBottom;
                    _settingsSceneFeedbacks.PlayFeedbacks();
                    break;
            }

            _currentTabPageIndex = (int)tabPage;
        }

        private void OnSettingsSceneUnloaded() => _settingsScreenDisplayed = false;

        private void UpdateHoldPlayButton(float t) => _holdPlayImg.fillAmount = 1f - t;

        #endregion


        #region UI Navigation

        public override void OnNorthButton_Performed(int playerdId)
        {
            if (_currentDisplayedMenu != TabPageMenu.Play) return;
            ToggleHowToPlay();
        }

        public override void OnWestButton_Performed(int playerdId)
        {
            if (_currentDisplayedMenu != TabPageMenu.Play) return;
            TogglePlayerManager();
        }

        public override void OnSubmit_Started(int playerId)
        {
            if (!_subMainMenuOpen) return;
            if (_currentDisplayedMenu == TabPageMenu.Play)
            {
                if (_onGameStarted) return;
                if(_holdPlayTimer != null) _holdPlayTimer.Stop();
                _holdPlayTimer = new Timer(1f, null, false);
                _holdPlayTimer.Start();
                _holdPlayTimer.OnValueChanged += UpdateHoldPlayButton;
                _isHoldingPlay = true;
            }
        }

        public override void OnSubmit_Canceled(int playerId)
        {
            if (!_subMainMenuOpen) return;
            if (_currentDisplayedMenu == TabPageMenu.Play)
            {
                if (_onGameStarted || _holdPlayTimer == null) return;
                _holdPlayTimer.Stop();
                _holdPlayTimer.OnValueChanged -= UpdateHoldPlayButton;
                _holdPlayImg.fillAmount = 0;
                _isHoldingPlay = false;
                if (_holdPlayTimer.IsTimeOver())
                {
                    _onGameStarted = true;
                    _playButton.onClick.Invoke();
                }
            }
        }

        public override void OnLeftShoulder_Performed(int playerdId)
        {
            ChangeToPreviousTabPage();
        }

        public override void OnRightShoulder_Performed(int playerdId)
        {
            ChangeToNextTabPage();
        }

        #endregion


        #region Private

        private int _currentTabPageIndex = (int)TabPageMenu.Play;

        private bool _settingsScreenDisplayed;
        private bool _onTransition;
        private bool _onGameStarted;
        private bool _subMainMenuOpen;

        private bool _isHoldingPlay;

        private Timer _holdPlayTimer;

        private TabPageMenu _currentDisplayedMenu = TabPageMenu.Play;

        #endregion
    }

    public enum TabPageMenu
    {
        Unknown = 0,
        Play = 1,
        Garage = 2,
        //Progression = 3,
        Settings = 3
    }
}