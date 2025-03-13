using LocalPlayer.Runtime;
using ScriptableEvent.Runtime;
using System.Collections.Generic;
using UINavigation.Runtime;
using UnityEngine;

namespace Settings.UI
{
    public class SettingUIControls : MenuBase
    {
        #region Exposed

        [SerializeField] private LocalPlayerControlSettings _prefab;
        [SerializeField] private Transform _parent;

        [SerializeField] private SettingsUIManager _manager;

        [SerializeField] private GameObject _firstSelectedGO;

        [SerializeField] private GameEventT _onRumbleValueChanged;
        [SerializeField] private GameEventT _onOrientationValueChanged;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshUI();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var item in _playerSettings)
            {
                Destroy(item.gameObject);
            }

            _playerSettings.Clear();
        }

        #endregion


        #region Main

        private void RefreshUI()
        {
            List<KeyValuePair<int, object[]>> configs = LocalPlayerManager.s_instance.GetConfigurationForLocalPlayers();

            int i = 1;
            foreach (var cfg in configs)
            {
                var instance = Instantiate(_prefab, _parent);

                instance.SetPlayerText(i);
                Material mat = (Material)cfg.Value[2];
                instance.SetPlayerColor(mat.color);
                instance.RefreshRumble((bool)cfg.Value[0]);
                instance.RefreshControls((bool)cfg.Value[1]);

                _playerSettings.Add(instance);
                i++;
            }
        }

        public void BackToSelectionMenu()
        {
            int i = 1;
            foreach (var item in _playerSettings)
            {
                _onRumbleValueChanged.Raise(new object[2] { i, item.GetRumbleValue() });
                _onOrientationValueChanged.Raise(new object[2] { i, item.GetOrientationIsWorldValue() });

                LocalPlayerManager.s_instance.UpdateConfigurationForLocalPlayers(i, item.GetRumbleValue(), item.GetOrientationIsWorldValue());
                i++;
            }

            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion


        #region UI Navigation

        public override void OnCancel_Performed(int playerId)
        {
            BackToSelectionMenu();
        }

        public override void OnSubmit_Performed(int playerId)
        {
            _playerSettings[playerId - 1].PlayerValidate();
        }

        protected override void NavigateUp_Performed(int playerId)
        {
            _playerSettings[playerId - 1].PlayerNavigateUp();
        }

        protected override void NavigateDown_Performed(int playerId)
        {
            _playerSettings[playerId - 1].PlayerNavigateDown();
        }

        #endregion


        #region Private

        private List<LocalPlayerControlSettings> _playerSettings = new();

        #endregion
    }
}