using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CameraSwap.Runtime
{
    public class CameraSwapManager : MonoBehaviour
    {
        #region Exposed

        public static CameraSwapManager s_instance;

        [SerializeField] private Material _fadeInMat;
        [SerializeField] private Material _fadeOutMat;
        [SerializeField] private Image _image;
        [SerializeField] private Canvas _canvas;

        [SerializeField] private GameObject[] _cameras;

        [SerializeField] private float _duration;
        [SerializeField] private float _maxOffset;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance = null) s_instance = this;
            else
            {
                Destroy(s_instance);
                s_instance = this;
            }

            foreach (var item in _cameras)
            {
                item.SetActive(false);
            }

            ResetMaterials();

            _cameras[0].SetActive(true);
            _canvas.worldCamera = _cameras[_currentIndex].GetComponentInChildren<Camera>();
        }

        public void ChangeToNextCamera()
        {
            ChangeCamera(1);
        }

        public void ChangeToPreviousCamera()
        {
            ChangeCamera(-1);
        }

        public void ChangeToCamera(int index, Action callback)
        {
            _lastIndex = _currentIndex;

            _currentIndex = index;
            StartCoroutine(nameof(FadeIn), callback);
        }

        #endregion


        #region Utils & Tools

        private void ChangeCamera(int value)
        {
            _lastIndex = _currentIndex;

            _currentIndex += value;
            _currentIndex %= _cameras.Length;
            if (_currentIndex < 0) _currentIndex = _cameras.Length - 1;

            StartCoroutine(nameof(FadeIn));
        }

        private IEnumerator FadeIn(Action callback)
        {
            _elapsedTime = 0;
            _fadeInMat.SetFloat(_offsetPropertyName, 0);
            _image.material = _fadeInMat;
            _offsetValue = 0;

            while (_offsetValue < _maxOffset)
            {
                _elapsedTime += Time.deltaTime;
                float ratio = Mathf.Clamp01(_elapsedTime / _duration);
                _offsetValue = Mathf.Lerp(0, _maxOffset, ratio);
                _fadeInMat.SetFloat(_offsetPropertyName, _offsetValue);
                yield return null;
            }

            ChangeActiveCamera(callback);
        }

        private void ChangeActiveCamera(Action callback)
        {
            _cameras[_lastIndex].SetActive(false);
            _canvas.worldCamera = _cameras[_currentIndex].GetComponentInChildren<Camera>();
            _cameras[_currentIndex].SetActive(true);

            StartCoroutine(nameof(FadeOut), callback);
        }

        private IEnumerator FadeOut(Action callback)
        {
            _elapsedTime = 0;
            _fadeOutMat.SetFloat(_offsetPropertyName, 0);
            _image.material = _fadeOutMat;
            _offsetValue = 0;

            while (_offsetValue < _maxOffset)
            {
                _elapsedTime += Time.deltaTime;
                float ratio = Mathf.Clamp01(_elapsedTime / _duration);
                _offsetValue = Mathf.Lerp(0, _maxOffset, ratio);
                _fadeOutMat.SetFloat(_offsetPropertyName, _offsetValue);
                yield return null;
            }

            ResetMaterials();

            callback.Invoke();
        }

        private void ResetMaterials()
        {
            _fadeInMat.SetFloat(_offsetPropertyName, 0);
            _fadeOutMat.SetFloat(_offsetPropertyName, _maxOffset);

            _image.material = _fadeInMat;
            _image.gameObject.SetActive(true);
        }

        #endregion


        #region Private

        private string _offsetPropertyName = "_Offset";

        private int _lastIndex;
        private int _currentIndex;

        private float _offsetValue = 0;
        private float _elapsedTime;

        #endregion
    }
}