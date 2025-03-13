using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Networking.UI.Runtime
{
    public class PlayerManagerUIEntry : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _playerNameTxt;
        [SerializeField] private Button _addFriendsBtn;
        [SerializeField] private Button _kickBtn;

        #endregion


        #region Main

        //public void Setup(PlayerLobbyNetwork playerNet)
        //{
        //    _playerNet = playerNet;

        //    _playerId = new CSteamID(playerNet.GetNetworkPlayerSteamID());
        //    _playerNameTxt.text = playerNet.GetNetworkPlayerName();

        //    _addFriendsBtn.onClick.AddListener(AddSteamFriends);
        //    _kickBtn.onClick.AddListener(Kick);
        //}

        //[ServerCallback]
        //private void AddSteamFriends()
        //{
        //    Debug.Log("Add friends");
        //    SteamFriends.RequestUserInformation(_playerId, false);
        //    SteamFriends.SetPlayedWith(_playerId);
        //}

        //[ServerCallback]
        //private void Kick()
        //{
        //    Debug.Log("Kick");
        //    _playerNet.Rpc_ForceDisconnectPlayer(_playerNet.netId);
        //    gameObject.SetActive(false);
        //}

        #endregion


        #region Private

        //private CSteamID _playerId;
        //private PlayerLobbyNetwork _playerNet;

        #endregion
    }
}