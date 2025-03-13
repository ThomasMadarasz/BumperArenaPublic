using Data.Runtime;
using Mirror;
using ScriptableEvent.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Runtime
{
    public class NetworkPlayer : NetworkBehaviour
    {
        #region Exposed

        [SyncVar] public int m_connectionId;

        [SyncVar(hook = nameof(OnReadyStatusChanged))] public bool m_isReady;
        [SyncVar] public bool m_alwaysReady;
        [SyncVar] public bool m_IsAI;
        [SyncVar] public bool m_IsLocalPlayer;
        [SyncVar] public string m_playerName;
        [SyncVar] public int m_playerIndex;
        [SyncVar] public int m_localPlayerIndex = -1;

        [SyncVar(hook = nameof(OnLoadingScreenTimerChanged))] public float m_loadingScreenRemainingTime;

        [SerializeField] private GameEventT _onFadeIn;
        [SerializeField] private GameEvent _onFadeOut;
        [SerializeField] private GameEventT _onLoadingScreenTimerChanged;

        [HideInInspector] public Action<int, bool> m_onStatusChanged;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            DontDestroyOnLoad(this.gameObject);
            _manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }

        public override void OnStartAuthority()
        {
            gameObject.name = "LocalGamePlayer";
            Cmd_SetPlayerName(PlayerData.s_instance.m_playerSteamData._playerName);
        }

        //public override void OnStartClient()
        //{
        //    _manager.RegisterPlayer(this);
        //}

        public override void OnStopClient()
        {
            _manager.UnregisterPlayer(this);
            Destroy(gameObject);
        }

        public void SetupAI()
        {
            m_alwaysReady = true;

            gameObject.name = $"AI_{m_playerIndex}";
            // TODO translate
            m_playerName = $"AI {m_playerIndex}";
        }

        public void SetupLocalPlayer(int localIndex)
        {
            gameObject.name = $"Local_{localIndex}";
            // TODO translate
            m_playerName = $"Player {localIndex}";
        }

        public void DisplayLocalLoadingScreen(List<NetworkPlayer> players)
        {
            if (!isOwned || m_IsAI || m_localPlayerIndex != 1) return;
            _onFadeIn.Raise(players);
        }

        #endregion


        #region Utils & Tools

        private void OnLoadingScreenTimerChanged(float oldValue, float newValue)
        {
            if (m_IsAI) return;
            _onLoadingScreenTimerChanged.Raise(newValue);
        }

        private void OnReadyStatusChanged(bool oldValue, bool newValue)
        {
            if (m_IsAI) return;
            m_onStatusChanged?.Invoke(m_localPlayerIndex, newValue);
        }

        #endregion


        #region Rpc

        [ClientRpc]
        public void Rpc_DisplayLoadingScreen(List<NetworkPlayer> players)
        {
            if (!isOwned || m_IsAI || m_localPlayerIndex !=1) return;
            _onFadeIn.Raise(players);
        }

        [ClientRpc]
        public void Rpc_HideLoadingScreen()
        {
            if (!isOwned || m_IsAI || m_localPlayerIndex != 1) return;
            _onFadeOut.Raise();
        }

        #endregion


        #region Command

        [Command]
        public void Cmd_SetReady(bool value)
        {
            if (m_alwaysReady) m_isReady = true;
            else m_isReady = value;

            _manager.PlayerIsReady();
        }

        [Command]
        public void Cmd_SetAlwaysReady(bool value) => m_alwaysReady = value;

        [Command]
        private void Cmd_SetPlayerName(string name)
        {
            m_playerName = name;
        }

        #endregion


        #region Private

        private CustomNetworkManager _manager;

        #endregion
    }
}