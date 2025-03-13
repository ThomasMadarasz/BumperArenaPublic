using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Data.Runtime;
using ScriptableEvent.Runtime;

namespace Networking.Runtime
{
    public class SteamLobbyManager : MonoBehaviour
    {
        #region Exposed

        public static SteamLobbyManager s_instance;

        [SerializeField]
        [Range(10, 50)]
        private int _maxLobbyDataToLoad;

        [HideInInspector] public Action m_onGetLobbyList;
        //[HideInInspector] public Action<List<CSteamID>, ulong> m_onGetLobbyData;

        [SerializeField] private GameObject _networkManager;

        //[SerializeField] private ELobbyType _defaultLobbyTypeOnCreated;

        [SerializeField] private GameEvent _onJoinLobby;
        [SerializeField] private GameEvent _onDisconnected;
        [SerializeField] private GameEventT _openModalInformation;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        //private void Start() => GetPlayerData();

        #endregion


        #region Main

        private void Setup()
        {
            //if (!SteamManager.Initialized) return;
            if (s_instance == null) s_instance = this;
            else Destroy(gameObject);

            //c_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            //c_joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            //c_lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

            //c_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
            //c_lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
        }

        //public void JoinLobby(CSteamID lobbyId)
        //{
        //    if (_onTryToJoinLobby) return;
        //    _onTryToJoinLobby = true;

        //    WaitingForm.s_instance.Show();

        //    DontDestroyOnLoad(Instantiate(_networkManager));
        //    SteamMatchmaking.JoinLobby(lobbyId);
        //}

        public void JoinLobby()
        {
            DontDestroyOnLoad(Instantiate(_networkManager));
        }

        //public void HostLobby()
        //{
        //    SteamMatchmaking.CreateLobby(_defaultLobbyTypeOnCreated, CustomNetworkManager.singleton.maxConnections);
        //}

        //public void GetLobbiesList()
        //{
        //    if (_lobbyIds.Count > 0) _lobbyIds.Clear();

        //    SteamMatchmaking.AddRequestLobbyListResultCountFilter(_maxLobbyDataToLoad);
        //    SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault);
        //    SteamMatchmaking.RequestLobbyList();
        //}

        public void ConnectedOnServer()
        {
            _onJoinLobby?.Raise();
        }

        public void DisconnectedFromServer()
        {
            // Client connected on steam lobby,force disconnect
            //if (SteamMatchmaking.GetNumLobbyMembers(new CSteamID(_currentLobbyId)) > 0)
            //{
            //    SteamMatchmaking.LeaveLobby(new CSteamID(_currentLobbyId));
            //}

            //c_lobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);

            //Debug.Log("Disconnected from server");

            _onDisconnected?.Raise();
        }

        //[ServerCallback]
        //public void UpdateLobbyDataPlayerCount(int playerCount, int maxPlayer)
        //{
        //    string txt = $"{playerCount}/{maxPlayer}";
        //    SteamMatchmaking.SetLobbyData(new CSteamID(_currentLobbyId), SteamDataKey.m_lobbyKeyPlayerCount, txt);
        //}

        //public void UpdateLobbyDataStatus(int status)
        //{
        //    SteamMatchmaking.SetLobbyData(new CSteamID(_currentLobbyId), SteamDataKey.m_lobbyStatus, status.ToString());
        //}

        [ClientCallback]
        public void DisconnectClient(string quitMessage = "")
        {
            //SteamMatchmaking.LeaveLobby(SteamUser.GetSteamID());
            CustomNetworkManager.singleton.StopClient();

            //if (!string.IsNullOrWhiteSpace(quitMessage))
            //{
            //    _openModalInformation?.Raise(quitMessage);
            //}
        }

        [ServerCallback]
        public void DisconnectHost()
        {
            //CSteamID lobbyId = SteamLobbyManager.s_instance.GetCurrentLobbySteamID();
            //CSteamID hostId = SteamUser.GetSteamID();

            //int numMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);

            //for (int i = 0; i < numMembers; i++)
            //{
            //    CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);

            //    if (memberID != hostId)
            //    {
            //        SteamMatchmaking.LeaveLobby(memberID);
            //    }
            //}

            //SteamMatchmaking.LeaveLobby(SteamUser.GetSteamID());
            CustomNetworkManager.singleton.StopHost();

            _onTryToJoinLobby = false;

            Debug.Log("You stopped the game server");
        }

        #endregion


        #region Callbacks

        //private void OnGetLobbyList(LobbyMatchList_t result)
        //{
        //    m_onGetLobbyList?.Invoke();

        //    for (int i = 0; i < result.m_nLobbiesMatching; i++)
        //    {
        //        CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
        //        _lobbyIds.Add(lobbyId);
        //        SteamMatchmaking.RequestLobbyData(lobbyId);
        //    }
        //}

        //private void OnGetLobbyData(LobbyDataUpdate_t result)
        //{
        //    m_onGetLobbyData?.Invoke(_lobbyIds, result.m_ulSteamIDLobby);
        //}

        //private void OnLobbyEntered(LobbyEnter_t callback)
        //{
        //    _currentLobbyId = callback.m_ulSteamIDLobby;

        //    if (NetworkServer.active) return;

        //    CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        //    CustomNetworkManager.singleton.networkAddress = SteamMatchmaking.GetLobbyData(lobbyId, SteamDataKey.m_lobbyKeyHostAddress);
        //    CustomNetworkManager.singleton.StartClient();

        //    c_lobbyDataUpdated.Unregister();
        //    c_lobbyDataUpdated.Dispose();
        //    c_lobbyDataUpdated = null;
        //}

        //private void OnLobbyCreated(LobbyCreated_t callback)
        //{
            //if (callback.m_eResult != EResult.k_EResultOK) return;

            //CustomNetworkManager.singleton.StartHost();

            //TODO Recup le lvl du joueur

        //    c_lobbyDataUpdated.Unregister();
        //    c_lobbyDataUpdated.Dispose();
        //    c_lobbyDataUpdated = null;

        //    SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), SteamDataKey.m_lobbyKeyHostAddress, _playerSteamData._steamID.ToString());
        //    SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), SteamDataKey.m_lobbyKeyHostName, $"(Lvl ??) {_playerSteamData._playerName}");
        //    SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), SteamDataKey.m_lobbyKeyPlayerCount, "1/4");
        //    SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), SteamDataKey.m_lobbyStatus, ((int)_defaultLobbyTypeOnCreated).ToString());
        //}

        //private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        //{
        //    DontDestroyOnLoad(Instantiate(_networkManager));
        //    SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        //}

        #endregion


        #region Utisl & Tools

        public ulong GetCurrentLobbyId() => _currentLobbyId;

        //public CSteamID GetCurrentLobbySteamID() => new CSteamID(_currentLobbyId);

        private void GetPlayerData() => _playerSteamData = PlayerData.s_instance.m_playerSteamData;

        //public CSteamID GetLobbyId() => new CSteamID(_currentLobbyId);

        #endregion


        #region Private

        private ulong _currentLobbyId;

        private PlayerSteamData _playerSteamData;

        //private List<CSteamID> _lobbyIds = new();

        //protected Callback<LobbyCreated_t> c_lobbyCreated;
        //protected Callback<GameLobbyJoinRequested_t> c_joinRequest;
        //protected Callback<LobbyEnter_t> c_lobbyEntered;

        //protected Callback<LobbyMatchList_t> c_lobbyList;
        //protected Callback<LobbyDataUpdate_t> c_lobbyDataUpdated;

        private bool _onTryToJoinLobby;

        #endregion
    }
}