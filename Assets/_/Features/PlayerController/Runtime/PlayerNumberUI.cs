using Archi.Runtime;
using TMPro;
using UnityEngine;

namespace PlayerController.Runtime
{
    public class PlayerNumberUI : CBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _txt;
        [SerializeField] private GameObject _UI;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update()
        {
            if (!_uiDisplayed) return;

            transform.rotation = Camera.main.transform.rotation;
        }

        #endregion


        #region Main

        private void Setup()
        {
            _UI.SetActive(false);
        }

        public void SetText(string txt)
        {
            _uiDisplayed = true;
            _txt.text = txt;
            _UI.SetActive(true);
        }

        public void SetColor(Color color)
        {
            _txt.color = color;
        }

        #endregion


        #region Private

        private bool _uiDisplayed;

        #endregion
    }
}