using Data.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Runtime
{
    public class NetworkSceneManager
    {
        public NetworkSceneManager(CustomNetworkManager manager, List<NetworkPlayer> players, DurationData data, bool isGameScene)
        {
            _isGameScene = isGameScene;
            _manager = manager;
            _players = players.Where(x => !x.m_IsAI).ToList();

            float duration = data.m_duration + data.m_delay;

            _totalDurationInMilliseconds = Mathf.RoundToInt(duration * 1000);
        }

        #region Main

        public async void LoadScene(string sceneToLoad)
        {
            if (string.IsNullOrWhiteSpace(sceneToLoad))
            {
                Debug.LogError("Scene to load is not correct");
                return;
            }

            DisplayLoadingScreen();

            await Task.Delay(_totalDurationInMilliseconds);

            _manager.ServerChangeScene(sceneToLoad);
        }

        public async void OnSceneLoaded(Action callback)
        {
            HideLoadingScreen();

            await Task.Delay(_totalDurationInMilliseconds);

            callback?.Invoke();
        }

        #endregion


        #region Utils & Tools

        private void DisplayLoadingScreen()
        {
            if (_isGameScene)
            {
                List<NetworkPlayer> p = _players.Where(x => x.m_localPlayerIndex == 1).ToList();
                //p.ForEach(p => p.Rpc_DisplayLoadingScreen(_players));
                p.ForEach(p => p.DisplayLocalLoadingScreen(_players));
            }

            else
                _players.Where(x => x.m_localPlayerIndex == 1).ToList().ForEach(p => p.Rpc_DisplayLoadingScreen(null));
        }

        private void HideLoadingScreen()
        {
            _players.Where(x => x.m_localPlayerIndex == 1).ToList().ForEach(p => p.Rpc_HideLoadingScreen());
        }

        #endregion


        #region Private

        private CustomNetworkManager _manager;
        private List<NetworkPlayer> _players;

        private int _totalDurationInMilliseconds;

        private bool _isGameScene;

        #endregion
    }
}