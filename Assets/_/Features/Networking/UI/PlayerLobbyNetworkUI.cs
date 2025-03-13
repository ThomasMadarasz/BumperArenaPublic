using Mirror;
using UnityEngine;
using TMPro;
using Networking.Runtime;
using Sirenix.Utilities;
using Data.Runtime;

namespace Networking.UI.Runtime
{
    [RequireComponent(typeof(PlayerLobbyNetwork))]
    public class PlayerLobbyNetworkUI : NetworkBehaviour
    {
        #region Exposed

        [SerializeField] private PlayerLobbyNetwork _playerLobby;

        [SerializeField] private TextMeshProUGUI _playerNameTxt;

        [SerializeField] private GameObject[] _availableOnClientOnly;

        [SerializeField] private Canvas _canvas;

        [SerializeField] private CustomisationData _customisationData;

        #endregion


        #region Unity API

        private void Awake()
        {
            _canvas.worldCamera = Camera.main;

            _availableOnClientOnly.ForEach(x => x.SetActive(false));

            _playerLobby.m_onPlayerNameChanged += OnPlayerNameChanged;
            _playerLobby.m_onReadyStatusChanged += OnPlayerReadyStatusChanged;
            _playerLobby.m_onPlayerMaterialIndexChanged += OnPlayerColorChanged;

            _playerLobby.m_onAIMateriaChanged += OnAIMaterialChanged;

            //LobbyUI.s_instance.AddUIForPlayer(_playerLobby);
        }

        public override void OnStartAuthority()
        {
            _availableOnClientOnly.ForEach(x => x.SetActive(true));
        }

        private void OnDestroy()
        {
            //LobbyUI.s_instance.RemoveUIForPlayer(_playerLobby);
        }

        #endregion


        #region Main

        private void OnPlayerNameChanged(string value)
        {
            _playerNameTxt.text = value;

            //LobbyUI.s_instance.UpdatePlayerName(_playerLobby, value);
        }

        private void OnPlayerReadyStatusChanged(bool value)
        {
            //LobbyUI.s_instance.UpdatePlayerStatus(_playerLobby, value);
        }

        private void OnPlayerColorChanged(int value)
        {
            Material mat = _customisationData.m_materials[value];
            _playerNameTxt.color = mat.color;

            //LobbyUI.s_instance.UpdatePlayerColor(_playerLobby, mat.color);
        }

        private void OnAIMaterialChanged()
        {
            Material mat = _customisationData.m_AIMaterial;
            _playerNameTxt.color = mat.color;

            //LobbyUI.s_instance.UpdatePlayerColor(_playerLobby, mat.color);

            _availableOnClientOnly.ForEach(x => x.SetActive(false));
        }

        #endregion
    }
}