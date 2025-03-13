using UnityEngine;
using TMPro;
using Networking.Runtime;
using System;

namespace Networking.UI.Runtime
{
    public class LobbyEntryUI : MonoBehaviour
    {
        //#region Exposed

        //[SerializeField] private TextMeshProUGUI _hostNameTxt;
        //[SerializeField] private TextMeshProUGUI _playerCountTxt;

        //#endregion


        //#region Main

        //public void Setup(CSteamID id)
        //{
        //    _lobbyId = id;

        //    string value = SteamMatchmaking.GetLobbyData(id, SteamDataKey.m_lobbyStatus);
        //    if (string.IsNullOrWhiteSpace(value)) gameObject.SetActive(false);
        //    else
        //    {
        //        if (Convert.ToInt32(value) == 3) gameObject.SetActive(false);
        //    }

        //    value = SteamMatchmaking.GetLobbyData(id, SteamDataKey.m_lobbyKeyHostName);
        //    _hostNameTxt.text = string.IsNullOrWhiteSpace(value) ? "Empty" : value;

        //    value = SteamMatchmaking.GetLobbyData(id, SteamDataKey.m_lobbyKeyPlayerCount);
        //    _playerCountTxt.text = string.IsNullOrWhiteSpace(value) ? "0/0" : value;
        //}

        //public void JoinLobby()
        //{
        //    SteamLobbyManager.s_instance.JoinLobby();
        //}

        //#endregion


        //#region Private

        //private CSteamID _lobbyId;

        //#endregion
    }
}