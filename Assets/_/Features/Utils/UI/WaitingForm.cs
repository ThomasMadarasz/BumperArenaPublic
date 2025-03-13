using System.Collections;
using UnityEngine;

namespace Utils.UI
{
    public class WaitingForm : MonoBehaviour
    {
        #region Exposed

        public static WaitingForm s_instance;

        [SerializeField] private float _showHideTime;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _graphics;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;
        }

        public void Show()
        {
            if (_isDisplayed) return;
            _isDisplayed = true;
            StopAllCoroutines();
            StartCoroutine(nameof(FadeIn));
        }

        public void Hide()
        {
            if (!_isDisplayed) return;
            StopAllCoroutines();
            StartCoroutine(nameof(FadeOut));
        }

        #endregion


        #region Utils & Tools

        private IEnumerator FadeIn()
        {
            _canvasGroup.alpha = 0;
            _graphics.SetActive(true);

            while (_canvasGroup.alpha < 1)
            {
                _canvasGroup.alpha += Time.deltaTime / _showHideTime;
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= Time.deltaTime / _showHideTime;
                yield return null;
            }

            _graphics.SetActive(false);
            _isDisplayed = false;
        }

        #endregion


        #region Private

        private bool _isDisplayed;

        #endregion
    }
}