using UINavigation.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Settings.UI
{
    public class QuitGameUI : MenuBase
    {
        #region Exposed

        [SerializeField] private SettingsUIManager _manager;
        [SerializeField] private GameObject _firstSelectedGO;

        #endregion


        #region Unity APi

        protected override void OnEnable()
        {
            base.OnEnable();

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);
        }

        #endregion


        #region Main

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void BackToSettings()
        {
            _manager.HideQuitGameMenu();
        }

        #endregion


        #region UI Navigation
        public override void OnCancel_Performed(int playerId)
        {
            BackToSettings();
        }

        #endregion
    }
}