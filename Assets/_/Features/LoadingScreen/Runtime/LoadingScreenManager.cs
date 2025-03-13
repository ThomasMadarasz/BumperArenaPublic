using Data.Runtime;
using ScriptableEvent.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UINavigation.Runtime;
using Networking.Runtime;
using System.Linq;
using Utils.Translation;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using Voting.Runtime;
using Utils.Runtime;
using UnityEngine.Video;

namespace LoadingScreen.Runtime
{
    public class LoadingScreenManager : MenuBase
    {
        #region Exposed

        public static LoadingScreenManager s_instance;

        [SerializeField][BoxGroup("Game Loading")] private CanvasGroup _loadingSceneCanvasGrp;

        [SerializeField][BoxGroup("Common")] private DurationData _data;

        [SerializeField][BoxGroup("Common")] private GameEventT _onFadeIn;
        [SerializeField][BoxGroup("Common")] private GameEvent _onFadeOut;
        [SerializeField][BoxGroup("Common")] private GameEvent _onManagersReady;

        [SerializeField][BoxGroup("Game Loading")] private Transform _playersParent;
        [SerializeField][BoxGroup("Game Loading")] private GameObject _playerUIPrefab;

        [SerializeField][BoxGroup("Other scene Loading")] private CanvasGroup _otherSceneCanvasGrp;
        [SerializeField][BoxGroup("Other scene Loading")] private TextMeshProUGUI _loadingScreenTxt;

        [SerializeField][BoxGroup("Mini Game Overview")] private GameObject _howToPlayObj;
        [SerializeField][BoxGroup("Mini Game Overview")] private GameObject _controlsObj;
        [SerializeField][BoxGroup("Mini Game Overview")] private GameObject _startBtn;
        [SerializeField][BoxGroup("Mini Game Overview")] private LocalizeStringEvent _showTxt;
        [SerializeField][BoxGroup("Mini Game Overview")] private LocalizedString _showHowToPlayString;
        [SerializeField][BoxGroup("Mini Game Overview")] private LocalizedString _showControlsString;
        [SerializeField][BoxGroup("Mini Game Overview")] private VideoPlayer _videoPlayer;

        [SerializeField] private TextMeshProUGUI _howToPlayTxt;
        [SerializeField] private TextMeshProUGUI _mapNameTxt;
        [SerializeField] private TextMeshProUGUI _modeNameTxt;
        [SerializeField] private Image _mapImage;
        [SerializeField] private Image _holdStartRoundImg;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            _onFadeIn.RegisterListener(LaunchFadeIn);
            _onFadeOut.RegisterListener(LaunchFadeOut);
            _onManagersReady.RegisterListener(OnManagersReady);
        }

        private void OnManagersReady()
        {
            _managersReady = true;
            if (!_startBtn.activeInHierarchy && _fadeInFinished) _startBtn.SetActive(true);
        }

        private void LaunchFadeIn(object players)
        {
            _startBtn.SetActive(false);
            _canPerformSubmit = false;
            _isReady = false;
            Clear();

            if (players != null)
            {
                List<Networking.Runtime.NetworkPlayer> test = (List<Networking.Runtime.NetworkPlayer>)players;
                test.ForEach(x => InstantiatePrefab(x));

                SetHowToPlayAndMapName();
                _canPerformSubmit = true;
            }
            else StartCoroutine(nameof(LoadingTextRoutine));

            _canvasGrp = players == null ? _otherSceneCanvasGrp : _loadingSceneCanvasGrp;

            StartCoroutine(FadeIn(_data.m_duration, _data.m_delay));
        }
        private void LaunchFadeOut() => StartCoroutine(FadeOut(_data.m_duration, _data.m_delay));

        #endregion


        #region Utils & Tools

        private IEnumerator LoadingTextRoutine()
        {
            string str = _loadingScreenTxt.text;
            int i = 0;

            while (true)
            {
                yield return new WaitForSeconds(0.3f);

                if (i == 3)
                {
                    _loadingScreenTxt.text = str;
                    i = 0;
                    continue;
                }
                else
                {
                    _loadingScreenTxt.text += ".";
                    i++;
                };
            }
        }

        private void SetHowToPlayAndMapName()
        {
            CustomNetworkManager manager = CustomNetworkManager.singleton as CustomNetworkManager;
            string mapName = manager.GetCurrentSceneDataName();

            ModeData _modeData = VotingManager.s_instance.GetModeData();

            GameModeData modeData = _modeData.m_gameModesData.FirstOrDefault(x => x.m_maps.Count(map => map.m_logicName == mapName) > 0);
            if (modeData == null)
            {
                Debug.LogError($"Not 'How to play' translation found for map : {mapName}");
                return;
            }

            MapData mapData = modeData.m_maps.FirstOrDefault(x => x.m_logicName == mapName);

            _mapImage.sprite = mapData.m_backgroundImage;
            _mapNameTxt.text = mapData.m_displayName;
            _howToPlayTxt.text = TranslationManager.Translate(modeData.m_howToPlay);
            _modeNameTxt.text = TranslationManager.Translate(modeData.m_name);
            _videoPlayer.clip = modeData.m_clip;
        }

        private void Clear()
        {
            foreach (var item in _playersUis)
            {
                Destroy(item.Value.gameObject);
            }
            _playersUis.Clear();
        }

        private void InstantiatePrefab(Networking.Runtime.NetworkPlayer p)
        {
            GameObject ui = Instantiate(_playerUIPrefab, _playersParent);
            ui.SetActive(false);

            PlayerLoadingUI playerUi = ui.GetComponent<PlayerLoadingUI>();
            playerUi.UpdatePlayerName(p.m_playerName);
            playerUi.UpdatePlayerIndex(p.m_playerIndex);

            _playersUis.Add(p.m_localPlayerIndex, ui);
        }

        private IEnumerator FadeIn(float duration, float delay)
        {
            _loadingScreenDisplayed = true;

            _canvasGrp.alpha = 0;
            _canvasGrp.gameObject.SetActive(true);

            while (_canvasGrp.alpha < 1)
            {
                _canvasGrp.alpha += Time.deltaTime / duration;
                yield return null;
            }

            yield return new WaitForSeconds(delay);

            _fadeInFinished = true;
            if (!_startBtn.activeInHierarchy && _managersReady) _startBtn.SetActive(true);
        }

        private IEnumerator FadeOut(float duration, float delay)
        {
            _canvasGrp.alpha = 1;

            while (_canvasGrp.alpha > 0)
            {
                _canvasGrp.alpha -= Time.deltaTime / duration;
                yield return null;
            }

            yield return new WaitForSeconds(delay);

            _canPerformSubmit = false;
            _canvasGrp.gameObject.SetActive(false);

            _loadingScreenDisplayed = false;

            StopCoroutine(nameof(LoadingTextRoutine));
        }

        public bool IsLoadingScreenDisplayed() => _loadingScreenDisplayed;

        public void SwitchHtpControls()
        {
            bool enable = _howToPlayObj.activeInHierarchy;
            _howToPlayObj.SetActive(!enable);
            _controlsObj.SetActive(enable);
            _showTxt.StringReference = enable ? _showHowToPlayString : _showControlsString;
        }

        private void UpdateHoldStartRoundButton(float t) => _holdStartRoundImg.fillAmount = 1f - t;

        #endregion


        #region UI Navigation

        public override void OnSubmit_Performed(int playerId)
        {
            //if (!_canPerformSubmit) return;
            //if (!_fadeInFinished) return;
            //if (!_managersReady) return;
            //if (_isReady) return;
            //_isReady = true;
            //_fadeInFinished = false;
            //_managersReady = false;

            //CustomNetworkManager manager = CustomNetworkManager.singleton as CustomNetworkManager;
            //manager.SetAllPlayerReady();
            //manager.PlayerIsReady();
        }

        public override void OnWestButton_Performed(int playerId)
        {
            if (!_canPerformSubmit) return;
            if (!_fadeInFinished) return;

            SwitchHtpControls();
        }
        public override void OnSubmit_Started(int playerId)
        {
            if (!_canPerformSubmit) return;
            if (!_fadeInFinished) return;
            if (!_managersReady) return;
            if (_isReady) return;
            if (_holdStartRoundTimer != null) _holdStartRoundTimer.Stop();
            _holdStartRoundTimer = new Timer(1f, null, false);
            _holdStartRoundTimer.Start();
            _holdStartRoundTimer.OnValueChanged += UpdateHoldStartRoundButton;
        }

        public override void OnSubmit_Canceled(int playerId)
        {
            if (!_canPerformSubmit) return;
            if (!_fadeInFinished) return;
            if (!_managersReady) return;
            if (_isReady) return;
            if (_holdStartRoundTimer == null) return;
            _holdStartRoundTimer.Stop();
            _holdStartRoundTimer.OnValueChanged -= UpdateHoldStartRoundButton;
            _holdStartRoundImg.fillAmount = 0;
            if (_holdStartRoundTimer.IsTimeOver())
            {
                LaunchRound();
            }
        }

        public void LaunchRound()
        {
            if (!_canPerformSubmit) return;
            if (!_fadeInFinished) return;
            if (!_managersReady) return;
            if (_isReady) return;
            _isReady = true;
            _fadeInFinished = false;
            _managersReady = false;

            CustomNetworkManager manager = CustomNetworkManager.singleton as CustomNetworkManager;
            manager.SetAllPlayerReady();
            manager.PlayerIsReady();
        }

        #endregion


        #region Private

        private Dictionary<int, GameObject> _playersUis = new();

        private CanvasGroup _canvasGrp;

        private bool _isReady;
        private bool _fadeInFinished;
        private bool _managersReady;
        private bool _canPerformSubmit;
        private bool _loadingScreenDisplayed;


        private Timer _holdStartRoundTimer;

        #endregion
    }
}