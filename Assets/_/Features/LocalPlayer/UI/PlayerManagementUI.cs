using LocalPlayer.Runtime;
using System.Collections.Generic;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.Localization;
using TMPro;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace LocalPlayer.UI.Runtime
{
    public class PlayerManagementUI : MenuBase
    {
        #region Exposed

        [SerializeField] private MenuBase _mainMenu;

        [SerializeField] private Transform _localPlayerUIParent;
        [SerializeField] private LPlayerConfigUI _localPlayerUIPrefab;

        [SerializeField] private GameObject _inputsObj;
        [SerializeField] private GameObject _infoTxtObj;
        [SerializeField] private GameObject[] _ObjsToDisable;

        [SerializeField] private LocalizeStringEvent _playText;
        [SerializeField] private LocalizeStringEvent _managePlayersText;
        [SerializeField] private HorizontalLayoutGroup _hlg;

        [SerializeField] private Button _addPlayerButton;
        [SerializeField] private Button _removePlayerButton;
        [SerializeField] private Button _removeAllPlayersButton;

        [SerializeField] private LocalizedString _playSoloString = new LocalizedString();
        [SerializeField] private LocalizedString _playMultiString = new LocalizedString();
        [SerializeField] private LocalizedString _addPlayersString = new LocalizedString();
        [SerializeField] private LocalizedString _managePlayersString = new LocalizedString();

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            LocalPlayerManager.s_instance.m_onLocalPlayerAdded -= OnLocalPlayerAdded;
            LocalPlayerManager.s_instance.m_onLocalPlayerRemoved -= OnLocalPlayerRemoved;
            LocalPlayerManager.s_instance.m_onLocalPlayerColorChanged -= OnLocalPlayerColorChanged;
            LocalPlayerManager.s_instance.m_onReorderedPlayers -= OnReorderedPlayers;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _mainMenu.DisableInput();
            LocalPlayerManager.s_instance.EnablePlayerModification();
            ShowHideInputs(false);
            ShowHideInfo(uis.Count > 1);
            foreach (var item in _ObjsToDisable) item.SetActive(false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            LocalPlayerManager.s_instance.DisablePlayerModification();

            foreach (var kvp in uis)
            {
                Destroy(kvp.Value.gameObject);
            }

            uis.Clear();

            _mainMenu.EnableInput();
        }

        #endregion


        #region Main

        private void Setup()
        {
            LocalPlayerManager.s_instance.m_onLocalPlayerAdded += OnLocalPlayerAdded;
            LocalPlayerManager.s_instance.m_onLocalPlayerRemoved += OnLocalPlayerRemoved;
            LocalPlayerManager.s_instance.m_onLocalPlayerColorChanged += OnLocalPlayerColorChanged;
            LocalPlayerManager.s_instance.m_onReorderedPlayers += OnReorderedPlayers;

            _addPlayerButton.onClick.AddListener(LocalPlayerManager.s_instance.OnClickAdvancedSubmit_Performed);
            _removePlayerButton.onClick.AddListener(LocalPlayerManager.s_instance.OnClickAdvancedCancel_Performed);
            _removeAllPlayersButton.onClick.AddListener(LocalPlayerManager.s_instance.OnClickRemoveAllPlayers);
        }

        #endregion


        #region Utils & Tools

        private void OnReorderedPlayers(List<KeyValuePair<int, int>> ids)
        {
            List<LPlayerConfigUI> oldUis = new();

            foreach (var kvp in ids)
            {
                int oldID = kvp.Key;
                oldUis.Add(uis[oldID]);
            }

            uis.Clear();

            for (int i = 0; i < ids.Count; i++)
            {
                int newID = ids[i].Value;
                uis.Add(newID, oldUis[i]);

                uis[newID].UpdateText();
            }
        }

        private void OnLocalPlayerColorChanged(int playerID, Color color)
        {
            uis[playerID].UpdatePlayerColor(color);
        }

        private void OnLocalPlayerAdded(int playerID, Enum.Runtime.DeviceType type)
        {
            if (!gameObject.activeInHierarchy) return;
            LPlayerConfigUI ui = Instantiate(_localPlayerUIPrefab, _localPlayerUIParent);
            LPlayer player = LocalPlayerManager.s_instance.GetPlayerByIndex(playerID);
            ui.Setup(player, type);

            uis.Add(playerID, ui);

            uis[playerID].UpdatePlayerColor(player.m_materialColor);
            ShowHideInputs(true);
            if (uis.Count > 1) ShowHideInfo(true);

            _hlg.padding.right = 25;

        }

        private void OnLocalPlayerRemoved(int playerID)
        {
            if (!gameObject.activeInHierarchy) return;
            Destroy(uis[playerID].gameObject);
            uis.Remove(playerID);
            if (uis.Count <= 1) ShowHideInfo(false);
            if (uis.Count <= 0)
            {
                ShowHideInputs(false);
                _hlg.padding.right = 0;
            }
        }

        private void ShowHideInputs(bool enable) => _inputsObj.SetActive(enable);

        private void ShowHideInfo(bool enable)
        {
            _infoTxtObj.SetActive(enable);
            if (enable)
            {
                _playText.StringReference = _playMultiString;
                _managePlayersText.StringReference = _managePlayersString;
            }
            else
            {
                _playText.StringReference = _playSoloString;
                _managePlayersText.StringReference = _addPlayersString;
            }
        }

        public void ShowHideInfo()
        {
            ShowHideInfo(LocalPlayerManager.s_instance.m_localPlayerCount > 1);
        }

        #endregion


        #region UI Navigation

        public override void OnWestButton_Performed(int playerID)
        {
            LocalPlayerManager.s_instance.ValidatePlayers(playerID);
            _mainMenu.OnWestButton_Performed(playerID);
            foreach (var item in _ObjsToDisable) item.SetActive(true);
        }

        public override void OnCancel_Performed(int playerID)
        {
            LocalPlayerManager.s_instance.RemovePlayer(playerID);
        }

        public override void OnNorthButton_Performed(int playerID)
        {
            LocalPlayerManager.s_instance.RemoveAllPlayers(playerID);
            ShowHideInputs(false);
            ShowHideInfo(false);
        }

        #endregion


        #region Private

        private Dictionary<int, LPlayerConfigUI> uis = new();

        #endregion
    }
}