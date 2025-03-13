using Mirror;
using Networking.Runtime;
using UnityEngine;
using Archi.Runtime;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using ScriptableEvent.Runtime;
using Core.Runtime;

namespace Networking.UI.Runtime
{
    public class LobbyUI : CNetBehaviour
    {
        //#region Exposed

        //public static LobbyUI s_instance;

        //[SerializeField] private string _mapVoteSceneName;

        //[SerializeField] private GameEventT _onLoadSceneRequired;

        //[SerializeField] private GameObject _playerManagerUI;
        //[SerializeField] private GameObject _playerStatusUI;
        //[SerializeField] private GameObject _startButton;
        //[SerializeField] private GameObject _inviteButton;

        //[SerializeField] private Button _publicBtn;
        //[SerializeField] private Button _privateBtn;
        //[SerializeField] private Button _friendsBtn;
        //[SerializeField] private Button _nextStatusBtn;
        //[SerializeField] private Button _previousStatusBtn;

        //[SerializeField] private Transform _playerStatusParent;

        //#endregion


        //#region Unity API

        //private void Awake() => Setup();

        //public override void OnStartServer()
        //{
        //    _publicBtn.interactable = true;
        //    _privateBtn.interactable = true;
        //    _friendsBtn.interactable = true;

        //    _nextStatusBtn.interactable = true;
        //    _previousStatusBtn.interactable = true;

        //    UpdateInviteFriendsButton();
        //}

        //#endregion


        //#region Main

        //private void Setup()
        //{
        //    s_instance = this;
        //    _startButton.SetActive(false);
        //    _inviteButton.SetActive(false);

        //    string status = SteamMatchmaking.GetLobbyData(SteamLobbyManager.s_instance.GetCurrentLobbySteamID(), SteamDataKey.m_lobbyStatus);
        //    int enumValue = Convert.ToInt32(status);
        //    _lobbyType = (ELobbyType)enumValue;
        //    _statusIndex = _lobbyType == ELobbyType.k_ELobbyTypePrivate ? 0 : _lobbyType == ELobbyType.k_ELobbyTypeFriendsOnly ? 1 : 2;

        //    _publicBtn.interactable = false;
        //    _privateBtn.interactable = false;
        //    _friendsBtn.interactable = false;

        //    _nextStatusBtn.interactable = false;
        //    _previousStatusBtn.interactable = false;

        //    UpdateRoomStatusButtons(_lobbyType);
        //    UpdateInviteFriendsButton();
        //}

        //public void InviteSteamFriends()
        //{
        //    if (!_inviteButton.activeInHierarchy) return;
        //    SteamFriends.ActivateGameOverlayInviteDialog(SteamLobbyManager.s_instance.GetLobbyId());
        //}

        //[ServerCallback]
        //public void StartGame()
        //{
        //    if (!AllPlayerReady()) return;

        //    SceneData data = new SceneData()
        //    {
        //        m_sceneName = _mapVoteSceneName,
        //        m_teamMode = Enum.Runtime.TeamModeEnum.Unknown
        //    };

        //    _onLoadSceneRequired.Raise(data);
        //}

        //[ServerCallback]
        //public void ChangeRoomStatus(string type)
        //{
        //    ELobbyType lobbyType = GetLobbyType(type);
        //    _lobbyType = lobbyType;

        //    CSteamID lobbyId = SteamLobbyManager.s_instance.GetCurrentLobbySteamID();
        //    SteamMatchmaking.SetLobbyType(lobbyId, lobbyType);

        //    SteamLobbyManager.s_instance.UpdateLobbyDataStatus((int)lobbyType);

        //    Debug.Log($"Room status changed to : {lobbyType}");
        //}

        //[ServerCallback]
        //public void TogglePlayerManageMenu() => _playerManagerUI.SetActive(!_playerManagerUI.activeInHierarchy);

        //public void LeaveRoom()
        //{
        //    if (isServer) SteamLobbyManager.s_instance.DisconnectHost();
        //    else SteamLobbyManager.s_instance.DisconnectClient(string.Empty);
        //}

        //public void AddUIForPlayer(PlayerLobbyNetwork player)
        //{
        //    GameObject go = Instantiate(_playerStatusUI, _playerStatusParent);
        //    _playersUis.Add(player, go);

        //    // Set default color for unready, because default value of bool is false, and hook on syncvar not called if the value of object not changed
        //    UpdatePlayerStatus(player, false);
        //}

        //public void RemoveUIForPlayer(PlayerLobbyNetwork player)
        //{
        //    Destroy(_playersUis[player]);
        //    _playersUis.Remove(player);
        //}

        //public void UpdatePlayerName(PlayerLobbyNetwork player, string name)
        //{
        //    _playersUis[player].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
        //}

        //public void UpdatePlayerStatus(PlayerLobbyNetwork player, bool ready)
        //{
        //    _playersUis[player].GetComponentInChildren<Image>().color = ready ? Color.green : Color.red;

        //    UpdateStartButton();
        //}

        //public void UpdatePlayerColor(PlayerLobbyNetwork player, Color color)
        //{
        //    _playersUis[player].GetComponentInChildren<TMPro.TextMeshProUGUI>().color = color;
        //}

        //[ServerCallback]
        //public void NextStatus() => ChangeRoomStatusWithIndex(true);

        //[ServerCallback]
        //public void PreviousStatus() => ChangeRoomStatusWithIndex(false);

        //[ServerCallback]
        //public void AddBot()
        //{
        //    var manager = CustomNetworkManager.singleton as CustomNetworkManager;
        //    manager.AddBot();
        //}

        //#endregion


        //#region Utils & Tools

        //private ELobbyType GetLobbyType(string type)
        //{
        //    if (type.Equals("Public", System.StringComparison.InvariantCultureIgnoreCase))
        //        return ELobbyType.k_ELobbyTypePublic;

        //    else if (type.Equals("Friends", System.StringComparison.InvariantCultureIgnoreCase))
        //        return ELobbyType.k_ELobbyTypeFriendsOnly;

        //    else
        //        return ELobbyType.k_ELobbyTypePrivate;
        //}

        //[ServerCallback]
        //private void UpdateStartButton()
        //{
        //    if (!isServer) return;

        //    _startButton.SetActive(AllPlayerReady());
        //}

        //private bool AllPlayerReady() => _playersUis.Keys.Count(x => !x.IsReady()) == 0;

        //private void OnRoomStatusChanged(ELobbyType oldValue, ELobbyType newValue)
        //{
        //    UpdateRoomStatusButtons(newValue);
        //    UpdateInviteFriendsButton();
        //}

        //private void UpdateInviteFriendsButton()
        //{
        //    if (_lobbyType == ELobbyType.k_ELobbyTypePublic || _lobbyType == ELobbyType.k_ELobbyTypePrivate)
        //    {
        //        _inviteButton.SetActive(true);
        //        return;
        //    }

        //    _inviteButton.SetActive(false);

        //    if (isServer && _lobbyType == ELobbyType.k_ELobbyTypeFriendsOnly) _inviteButton.SetActive(true);
        //}

        //private void UpdateRoomStatusButtons(ELobbyType lobbyType)
        //{
        //    ColorBlock block = _publicBtn.colors;
        //    block.normalColor = Color.white;
        //    block.selectedColor = Color.white;
        //    block.disabledColor = Color.white;

        //    _publicBtn.colors = block;
        //    _privateBtn.colors = block;
        //    _friendsBtn.colors = block;

        //    block.normalColor = Color.yellow;
        //    block.selectedColor = Color.yellow;
        //    block.disabledColor = Color.yellow;

        //    switch (lobbyType)
        //    {
        //        case ELobbyType.k_ELobbyTypePrivate:
        //            _privateBtn.colors = block;
        //            break;
        //        case ELobbyType.k_ELobbyTypeFriendsOnly:
        //            _friendsBtn.colors = block;
        //            break;
        //        case ELobbyType.k_ELobbyTypePublic:
        //            _publicBtn.colors = block;
        //            break;
        //    }
        //}

        //private void ChangeRoomStatusWithIndex(bool isNext)
        //{
        //    if (isNext)
        //    {
        //        if (_statusIndex == 2) return;
        //        _statusIndex++;
        //    }
        //    else
        //    {
        //        if (_statusIndex == 0) return;
        //        _statusIndex--;
        //    }

        //    _lobbyType = (ELobbyType)_statusIndex;
        //}

        //#endregion


        //#region Private

        //private Dictionary<PlayerLobbyNetwork, GameObject> _playersUis = new();

        //private List<bool> _playerColorAvailability;

        //private int _statusIndex;

        //[SyncVar(hook = nameof(OnRoomStatusChanged))] private ELobbyType _lobbyType = ELobbyType.k_ELobbyTypeInvisible;

        //#endregion
    }
}