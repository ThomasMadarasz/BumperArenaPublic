using Mirror;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace Stats.Runtime
{
    public class StatsManager : MonoBehaviour
    {
        #region Exposed

        [BoxGroup("Network")][SerializeField] private GameObject _netUI;
        [BoxGroup("Network")][SerializeField] private TextMeshProUGUI _serverReceivedTxt;
        [BoxGroup("Network")][SerializeField] private TextMeshProUGUI _serverSendTxt;

        [BoxGroup("Network")][SerializeField] private TextMeshProUGUI _clientReceivedTxt;
        [BoxGroup("Network")][SerializeField] private TextMeshProUGUI _clientSendTxt;

        [BoxGroup("Local")][SerializeField] private TextMeshProUGUI _fpsTxt;

        #endregion


        #region Unity API

        private void Update()
        {
            // calculate results every second
            if (NetworkTime.localTime >= _intervalStartTime + 1)
            {
                if (NetworkClient.active) UpdateClient();
                if (NetworkServer.active) UpdateServer();

                _intervalStartTime = NetworkTime.localTime;

                if (_networkActive && !NetworkClient.active)
                {
                    _networkActive = false;
                    _netUI.SetActive(false);
                }
                else if (!_networkActive && NetworkClient.active)
                {
                    _networkActive = true;
                    _netUI.SetActive(true);

                    Transport transport = Transport.active;
                    if (transport != null)
                    {
                        transport.OnClientDataReceived += OnClientReceive;
                        transport.OnClientDataSent += OnClientSend;
                        transport.OnServerDataReceived += OnServerReceive;
                        transport.OnServerDataSent += OnServerSend;
                    }
                }
            }

            if (Time.realtimeSinceStartup >= _intervalStartTimeLocal + 1)
            {
                UpdateFPS();
                _intervalStartTimeLocal = Time.realtimeSinceStartup;
            }
        }

        #endregion


        #region Main

        private void UpdateFPS()
        {
            int fps = Mathf.RoundToInt(1f / Time.deltaTime);
            _fpsTxt.text = $"{fps} Fps";
        }

        private void UpdateClient()
        {
            _clientReceivedPacketsPerSecond = _clientIntervalReceivedPackets;
            _clientReceivedBytesPerSecond = _clientIntervalReceivedBytes;
            _clientSentPacketsPerSecond = _clientIntervalSentPackets;
            _clientSentBytesPerSecond = _clientIntervalSentBytes;

            _clientIntervalReceivedPackets = 0;
            _clientIntervalReceivedBytes = 0;
            _clientIntervalSentPackets = 0;
            _clientIntervalSentBytes = 0;

            _clientReceivedTxt.text = $"Recv: {_clientReceivedPacketsPerSecond} msgs @ {Utils.PrettyBytes(_clientReceivedBytesPerSecond)}/s";
            _clientSendTxt.text = $"Send: {_clientSentPacketsPerSecond} msgs @ {Utils.PrettyBytes(_clientSentBytesPerSecond)}/s";
        }

        private void UpdateServer()
        {
            _serverReceivedPacketsPerSecond = _serverIntervalReceivedPackets;
            _serverReceivedBytesPerSecond = _serverIntervalReceivedBytes;
            _serverSentPacketsPerSecond = _serverIntervalSentPackets;
            _serverSentBytesPerSecond = _serverIntervalSentBytes;

            _serverIntervalReceivedPackets = 0;
            _serverIntervalReceivedBytes = 0;
            _serverIntervalSentPackets = 0;
            _serverIntervalSentBytes = 0;

            _serverReceivedTxt.text = $"Recv: {_serverReceivedPacketsPerSecond} msgs @ {Utils.PrettyBytes(_serverReceivedBytesPerSecond)}/s";
            _serverSendTxt.text = $"Send: {_serverSentPacketsPerSecond} msgs @ {Utils.PrettyBytes(_serverSentBytesPerSecond)}/s";
        }

        #endregion


        #region Utils & Tools

        private void OnClientReceive(ArraySegment<byte> data, int channelId)
        {
            ++_clientIntervalReceivedPackets;
            _clientIntervalReceivedBytes += data.Count;
        }

        private void OnClientSend(ArraySegment<byte> data, int channelId)
        {
            ++_clientIntervalSentPackets;
            _clientIntervalSentBytes += data.Count;
        }

        private void OnServerReceive(int connectionId, ArraySegment<byte> data, int channelId)
        {
            ++_serverIntervalReceivedPackets;
            _serverIntervalReceivedBytes += data.Count;
        }

        private void OnServerSend(int connectionId, ArraySegment<byte> data, int channelId)
        {
            ++_serverIntervalSentPackets;
            _serverIntervalSentBytes += data.Count;
        }

        #endregion


        #region Private

        private bool _networkActive;

        private float _intervalStartTimeLocal;

        // update interval
        private double _intervalStartTime;

        // ---------------------------------------------------------------------

        // CLIENT (public fields for other components to grab statistics)
        // long bytes to support >2GB
        private int _clientIntervalReceivedPackets;
        private long _clientIntervalReceivedBytes;
        private int _clientIntervalSentPackets;
        private long _clientIntervalSentBytes;

        // results from last interval
        // long bytes to support >2GB
        private int _clientReceivedPacketsPerSecond;
        private long _clientReceivedBytesPerSecond;
        private int _clientSentPacketsPerSecond;
        private long _clientSentBytesPerSecond;

        // ---------------------------------------------------------------------

        // SERVER (public fields for other components to grab statistics)
        // capture interval
        // long bytes to support >2GB
        private int _serverIntervalReceivedPackets;
        private long _serverIntervalReceivedBytes;
        private int _serverIntervalSentPackets;
        private long _serverIntervalSentBytes;

        // results from last interval
        // long bytes to support >2GB
        private int _serverReceivedPacketsPerSecond;
        private long _serverReceivedBytesPerSecond;
        private int _serverSentPacketsPerSecond;
        private long _serverSentBytesPerSecond;

        #endregion
    }
}