using System.Collections;
using TMPro;
using UnityEngine;

namespace Utils.UI
{
    public class FPSInfos : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private float _updatingTime;
        [SerializeField] private TextMeshProUGUI _fpsText;

        #endregion


        #region Unity API

        private void Awake()
        {
            StartCoroutine(nameof(CalculateFPS));
        }

        #endregion


        #region Main

        private IEnumerator CalculateFPS()
        {
            while (true)
            {
                _fps = (int)(1f / Time.unscaledDeltaTime);
                UpdateUI();
                yield return new WaitForSeconds(_updatingTime);
            }
        }

        private void UpdateUI()
        {
            _fpsText.text = $"{_fps} FPS";
        }

        #endregion


        #region Private

        private int _fps;

        #endregion
    }
}