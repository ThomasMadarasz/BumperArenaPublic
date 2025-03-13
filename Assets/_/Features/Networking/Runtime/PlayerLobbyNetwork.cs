using Archi.Runtime;
using Data.Runtime;
using Mirror;
using System;
using System.Collections;
using UnityEngine;

namespace Networking.Runtime
{
    public class PlayerLobbyNetwork : CNetBehaviour
    {
        #region Exposed

        [HideInInspector] public Action<bool> m_onReadyStatusChanged;
        [HideInInspector] public Action<string> m_onPlayerNameChanged;
        [HideInInspector] public Action<int> m_onPlayerMaterialIndexChanged;
        [HideInInspector] public Action m_onAIMateriaChanged;

        #endregion


        #region Unity API

        public override void OnStartAuthority() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            _playerSteamData = PlayerData.s_instance.m_playerSteamData;
            StartCoroutine(nameof(SetupEnumerator));

            //Cmd_GetInitialColor();
            //Cmd_SetPlayerName(GetPlayerName());
            //Cmd_SetPlayerSteamId(GetSteamId());
        }

        public void SetPlayerIndexAndAI(int index, int localPlayerIndex, bool isAI)
        {
            _isAI = isAI;
            _localPlayerIndex = localPlayerIndex;
            _playerIndex = index;
            _isReady = true;
        }

        private IEnumerator SetupEnumerator()
        {
            while (_playerIndex == -1)
            {
                yield return new WaitForSeconds(0.05f);
            }

            //if (_isAI) Cmd_GetColorForAI();
            //else Cmd_GetInitialColor();

            string playerName = string.Empty;

            // TODO translate

            if (_isAI) playerName = "AI";
            else if (_localPlayerIndex == 1) playerName = GetPlayerName();
            else playerName = $"Player {_localPlayerIndex}";

            //Cmd_SetPlayerName(playerName);
        }

        public string GetPlayerName()
        {
            if (_playerSteamData == null) Setup();
            return _playerSteamData._playerName;
        }

        public void ChangeReadyStatus()
        {
            //Cmd_SetStatus(!_isReady);
        }

        public void GetNextColor()
        {
            //Cmd_GetColor(true);
        }

        public void GetPreviousColor()
        {
            //Cmd_GetColor(false);
        }

        public ulong GetSteamId()
        {
            if (_playerSteamData == null) Setup();
            return _playerSteamData._steamID;
        }

        #endregion


        #region Utils & Tools

        public bool IsReady() => _isReady;

        private void OnPlayerReadystatusChanged(bool oldValue, bool newValue)
        {
            m_onReadyStatusChanged?.Invoke(newValue);
        }

        private void OnPlayerNameChanged(string oldValue, string newValue)
        {
            m_onPlayerNameChanged?.Invoke(newValue);
        }

        private void OnPlayerMaterialIndexChanged(int oldValue, int newValue)
        {
            m_onPlayerMaterialIndexChanged?.Invoke(newValue);
        }

        public string GetNetworkPlayerName() => _playerName;

        public ulong GetNetworkPlayerSteamID() => _playerSteamId;

        #endregion


        #region Command

        //[Command]
        //private void Cmd_SetPlayerName(string playerName)
        //{
        //    _playerName = playerName;
        //}

        //[Command]
        //private void Cmd_SetPlayerSteamId(ulong id)
        //{
        //    _playerSteamId = id;
        //}

        //[Command]
        //private void Cmd_SetStatus(bool value)
        //{
        //    _isReady = value;
        //}

        //[Command]
        //private void Cmd_GetColor(bool isNext)
        //{
        //    int newMatIndex;

        //    if (isNext)
        //        newMatIndex = LobbyManager.s_instance.GetNextAvailableMaterialIndex(_playerMaterialIndex, _playerIndex);
        //    else
        //        newMatIndex = LobbyManager.s_instance.GetPreviousAvailableMaterialIndex(_playerMaterialIndex, _playerIndex);

        //    _playerMaterialIndex = newMatIndex;
        //}

        //[Command]
        //private void Cmd_GetInitialColor()
        //{
        //    _playerMaterialIndex = LobbyManager.s_instance.GetFirstAvailableMaterialIndex(_playerIndex);
        //}

        //[Command]
        //private void Cmd_GetColorForAI()
        //{
        //    m_onAIMateriaChanged?.Invoke();
        //}

        #endregion


        #region Private

        private PlayerSteamData _playerSteamData;

        [SyncVar(hook = nameof(OnPlayerReadystatusChanged))] private bool _isReady;
        [SyncVar(hook = nameof(OnPlayerNameChanged))] private string _playerName;
        [SyncVar(hook = nameof(OnPlayerMaterialIndexChanged))] private int _playerMaterialIndex = -1;
        [SyncVar] private ulong _playerSteamId = 0;

        private int _playerIndex = -1;
        private int _localPlayerIndex = -1;
        private bool _isAI;

        #endregion
    }
}