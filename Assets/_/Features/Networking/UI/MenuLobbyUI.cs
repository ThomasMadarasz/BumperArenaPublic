using Archi.Runtime;
using Networking.Runtime;
using UnityEngine;

namespace Networking.UI.Runtime
{
    public class MenuLobbyUI : CBehaviour
    {
        #region Exposed

        [SerializeField] private GameObject _mainMenuUI;

        //[SerializeField] private LobbyEntryUI _lobbyEntryPrefab;

        [SerializeField] private GameObject _networkManager;

        #endregion


        #region Unity API

        //private void Start() => Setup();

        #endregion


        #region Main

        public void CreateLobby()
        {
            DontDestroyOnLoad(Instantiate(_networkManager));
            //SteamLobbyManager.s_instance.HostLobby();

            CustomNetworkManager.singleton.StartHost();
        }

        //public void FindLobby()
        //{
        //    _mainMenuUI.SetActive(false);
        //    ClearLobbyList();
        //    SteamLobbyManager.s_instance.GetLobbiesList();
        //}

        //private void Setup()
        //{
        //    SteamLobbyManager.s_instance.m_onGetLobbyData += UpdateLobbyData;
        //}

        //private void UpdateLobbyData(List<CSteamID> lobbyIds, ulong lobbyId)
        //{
        //    for (int i = 0; i < lobbyIds.Count; i++)
        //    {
        //        if (lobbyIds[i].m_SteamID == lobbyId)
        //        {
        //            LobbyEntryUI entry = Instantiate(_lobbyEntryPrefab);

        //            entry.Setup((CSteamID)lobbyIds[i].m_SteamID);

        //            entry.transform.localScale = Vector3.one;

        //            _lobbies.Add(entry);
        //        }
        //    }
        //}

        //private void ClearLobbyList()
        //{
        //    foreach (var item in _lobbies)
        //    {
        //        Destroy(item.gameObject);
        //    }

        //    _lobbies.Clear();
        //}

        public void BackToMainMenu()
        {
            _mainMenuUI.SetActive(true);
        }

        //public void RefreshLobbiesList()
        //{
        //    ClearLobbyList();
        //    SteamLobbyManager.s_instance.GetLobbiesList();
        //}

        #endregion


        #region Private

        //private List<LobbyEntryUI> _lobbies = new();

        #endregion
    }
}