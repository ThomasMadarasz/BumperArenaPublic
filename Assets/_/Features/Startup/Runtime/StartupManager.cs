using Archi.Runtime;
using SceneManager.runtime;
using ScriptableEvent.Runtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Startup.Runtime
{
    public class StartupManager : CBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onSteamDataLoaded;
        [SerializeField] private GameEvent _onSkipStartup;
        [SerializeField] private StartupImageData[] _startupImagesToDisplay;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _image;
        [SerializeField] private float _delay = .1f;
        [SerializeField] private StartupImageData _lastScreen;
        [SerializeField] private GameObject _lastScreenText;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Start() => LaunchFadeIn();

        private void OnDestroy()
        {
            _onSkipStartup.UnregisterListener(OnSkipStartup);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _lastScreenText.SetActive(false);

            _onSteamDataLoaded.RegisterListener(OnAllDataLoaded);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Manager", LoadSceneMode.Additive);
            _index = 0;

            _canvasGroup.alpha = 0;
            _canvasGroup.gameObject.SetActive(true);

            _onSkipStartup.RegisterListener(OnSkipStartup);
        }

        private void LaunchFadeIn()
        {
            StartCoroutine(FadeIn(_startupImagesToDisplay[_index].m_fadeDuration, _startupImagesToDisplay[_index].m_image, _startupImagesToDisplay[_index].m_gameObjectToEnable, OnFadeInFinished));
        }

        private void OnFadeInFinished()
        {
            StartCoroutine(Wait(_startupImagesToDisplay[_index].m_duration, OnWaitingFinished));
        }

        private void OnWaitingFinished()
        {
            StartCoroutine(FadeOut(_startupImagesToDisplay[_index].m_fadeDuration, _startupImagesToDisplay[_index].m_gameObjectToEnable, OnFadeOutFinished));
        }

        private void OnFadeOutFinished()
        {
            _index++;
            if (_index > _startupImagesToDisplay.Length - 1)
            {
                StartCoroutine(FadeIn(_lastScreen.m_fadeDuration, _lastScreen.m_image, _lastScreen.m_gameObjectToEnable, OnLastScreenDisplayed));
            }
            else
            {
                // Launch next StartupImage
                LaunchFadeIn();
            }
        }

        private IEnumerator FadeIn(float duration, Sprite sprite,GameObject go, Action callback)
        {
            _image.sprite = sprite;
            _image.preserveAspect = true;

            if (go != null) go.SetActive(true);

            _canvasGroup.alpha = 0;
            _canvasGroup.gameObject.SetActive(true);

            while (_canvasGroup.alpha < 1)
            {
                _canvasGroup.alpha += Time.deltaTime / duration;
                yield return null;
            }

            yield return new WaitForSeconds(_delay);
            callback?.Invoke();
        }

        private IEnumerator Wait(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            yield return new WaitForSeconds(_delay);
            callback?.Invoke();
        }

        private IEnumerator FadeOut(float duration,GameObject go, Action callback)
        {
            _canvasGroup.alpha = 1;

            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= Time.deltaTime / duration;
                yield return null;
            }

            yield return new WaitForSeconds(_delay);

            if (go != null) go.SetActive(false);

            _canvasGroup.gameObject.SetActive(false);
            callback?.Invoke();
        }

        private void OnAllDataLoaded() => _allDataLoaded = true;

        private void OnLastScreenDisplayed()
        {
            _lastScreenDisplayed = true;
            _canSkipLastScreen = true;

            _lastScreenText.SetActive(true);
        }

        public void DisplayMainMenu()
        {
            if (!_lastScreenDisplayed && !_allDataLoaded) return;

            SceneLoader.s_instance.LoadMainMenu(UnloadStartup);
        }

        private void UnloadStartup()
        {
            SceneLoader.s_instance.UnloadStartupScene();
        }

        public void OnSkipStartup()
        {
            if (_canSkipLastScreen)
            {
                _canSkipLastScreen = false;
                DisplayMainMenu();
            }
        }

        #endregion


        #region Private

        private int _index;

        private bool _lastScreenDisplayed;
        private bool _allDataLoaded;
        private bool _canSkipLastScreen;

        #endregion
    }
}