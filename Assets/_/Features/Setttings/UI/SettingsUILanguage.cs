using Settings.Runtime;
using TMPro;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Settings.UI
{
    public class SettingsUILanguage : MenuBase
    {
        #region Exposed

        [SerializeField] private TMP_Dropdown _languagesDropDown;


        [SerializeField] private SettingsUIManager _manager;

        #endregion


        #region Unity API

        private void Awake()
        {
            _languagesDropDown.ClearOptions();
            List<string> languagesTxt = new List<string>();

            languagesTxt.Add("English");
            languagesTxt.Add("Français");

            _languagesDropDown.AddOptions(languagesTxt);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshUI();

            EventSystem.current.SetSelectedGameObject(null);

            EventSystem.current.SetSelectedGameObject(_languagesDropDown.gameObject);
        }

        #endregion


        #region Main

        private void RefreshUI()
        {
            SettingsData settings = SettingsManager.s_instance.m_settingsData;
            int i = 0;
            if (settings.m_languageKey == "fr") i = 1;

            _languagesDropDown.SetValueWithoutNotify(i);
        }

        public void BackToSelectionMenu()
        {
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion


        #region Event

        public void OnLanguageChanged(int value)
        {
            if (value == 0) SettingsManager.s_instance.ChangeLanguage("en");
            else if (value == 1) SettingsManager.s_instance.ChangeLanguage("fr");
        }

        #endregion


        #region UI Navigation

        public override void OnCancel_Performed(int playerId)
        {
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion
    }
}