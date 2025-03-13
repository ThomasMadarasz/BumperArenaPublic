using Archi.Runtime;
using Networking.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.UI.Runtime
{
    public class PlayerManagerUI : CBehaviour
    {
        #region Exposed

        [SerializeField] private PlayerManagerUIEntry _playerUIPrefab;
        [SerializeField] private Transform _parentUI;

        #endregion


        #region Unity API

        private void OnEnable() => Setup();

        private void OnDisable()
        {
            foreach (var item in playersUis)
            {
                Destroy(item.gameObject);
            }

            playersUis.Clear();
        }

        #endregion


        #region Main

        private void Setup()
        {
            CustomNetworkManager manager = CustomNetworkManager.singleton as CustomNetworkManager;
            List<GameObject> playersGos = manager.GetPlayersInLobby();

            List<PlayerLobbyNetwork> players = new();
            playersGos.ForEach(p => players.Add(p.GetComponent<PlayerLobbyNetwork>()));

            foreach (var player in players)
            {
                PlayerManagerUIEntry ui = Instantiate(_playerUIPrefab, _parentUI);
                //ui.Setup(player);

                playersUis.Add(ui);
            }
        }

        public void CloseMenu() => gameObject.SetActive(false);

        #endregion


        #region Private

        private List<PlayerManagerUIEntry> playersUis = new();

        #endregion
    }
}