using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Networking.UI.Runtime
{
    public class PingUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _txt;
        [SerializeField] private PingConfig[] _config;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            CancelInvoke();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _configs = _config.OrderBy(x => x.m_maxPingValue).ToList();
            UpdatePing();
            InvokeRepeating(nameof(UpdatePing), 1, 1);
        }

        private void UpdatePing()
        {
            double ping = GetPing();
            Color color = GetColor(ping);

            _txt.text = $"{ping} ms";
            _txt.color = color;
        }

        #endregion


        #region Utils & Tools

        private double GetPing() => Math.Round(NetworkTime.rtt * 1000);

        private Color GetColor(double ping)
        {
            int intPing = Convert.ToInt32(ping);

            foreach (var conf in _configs)
            {
                if (intPing <= conf.m_maxPingValue) return conf.m_color;
            }

            return _configs.LastOrDefault().m_color;
        }

        #endregion


        #region Private

        private List<PingConfig> _configs;

        #endregion
    }

    [Serializable]
    public class PingConfig
    {
        public int m_maxPingValue;
        public Color m_color;
    }
}