using LocalPlayer.Runtime;
using TMPro;
using UnityEngine;
using Utils.Translation;

namespace LocalPlayer.UI.Runtime
{
    public class LocalPlayerNumberUI : MonoBehaviour
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
            if (Camera.main == null) return;
            transform.rotation = Camera.main.transform.rotation;
        }

        #endregion


        #region Main

        private void Setup()
        {
            //LPlayer player = GetComponentInParent<LPlayer>();
            //player.m_onColorChanged += UpdateColor;
            //player.m_onPlayerIDChanged += UpdateText;
        }

        private void UpdateColor(Color color)
        {
            _txt.color = color;
        }

        public void UpdateText(int ID)
        {
            _uiDisplayed = true;
            string playerLetter = TranslationManager.Translate("PlayerNumberKey");
            _txt.text = $"{playerLetter}{ID}";
            //_UI.SetActive(true);
        }

        #endregion


        #region Private

        private bool _uiDisplayed;

        #endregion
    }
}