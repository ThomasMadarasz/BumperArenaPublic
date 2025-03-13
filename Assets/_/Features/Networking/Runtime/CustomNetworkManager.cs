using Core.Runtime;
using Data.Runtime;
using Enum.Runtime;
using Interfaces.Runtime;
using LocalPlayer.Runtime;
using Mirror;
using SceneManager.runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimelineVolume.Runtime;
using UnityEngine;
using Utils.Runtime;
using Voting.Runtime;

namespace Networking.Runtime
{
    public class CustomNetworkManager : NetworkManager
    {
        #region Exposed

        [SerializeField] private string _roundResultSceneName;
        [SerializeField] private string _mapVoteSceneName;

        [SerializeField] private GameObject _playerLobbyPrefab;
        [SerializeField] private NetworkPlayer _netPlayerPrefab;

        [SerializeField] private GameEvent _onGameStart;
        [SerializeField] private GameEvent _onRoundResultStart;
        [SerializeField] private GameEvent _onMapVoteStart;
        [SerializeField] private GameEventT _onLoadSceneRequired;
        [SerializeField] private GameEvent _onAllPlayerAsSceneLoaded;
        [SerializeField] private GameEvent _onSetupRoundResultScene;
        [SerializeField] private GameEvent _onStartSpline;
        [SerializeField] private GameEvent _onManagersReady;
        [SerializeField] private GameEvent _onStopMainMenuMusic;
        [SerializeField] private GameEvent _onStartMainMenuMusic;
        [SerializeField] private GameEvent _onStopMusic;
        [SerializeField] private GameEvent _backToMainMenu;
        [SerializeField] private GameEvent _onPlayerJoinLobby;
        [SerializeField] private GameEvent _onPlayerLeaveLobby;

        [SerializeField] private GameEventT _playerFeedbacks;

        [SerializeField] private DurationData _durationLoadScene;

        [SerializeField][BoxGroup("Disconnection")] private GameEventT _onFadeIn;
        [SerializeField][BoxGroup("Disconnection")] private GameEvent _onFadeOut;

        [HideInInspector] public int m_playerCount { get { return _players.Count; } }

        [SerializeField] private int _timeOnLoadingScreen;

        #endregion


        #region Main

        public override void OnStartServer()
        {
            base.OnStartServer();
            _playersIndexAndLocalIndex = new();
            _onLoadSceneRequired.RegisterListener(LoadScene);
            _backToMainMenu.RegisterListener(BackToMainMenu);
        }

        public override void OnClientConnect()
        {
            _onPlayerJoinLobby?.Raise();
            base.OnClientConnect();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            //if (SceneLoader.s_instance.GetCurrentActiveScene() != _lobbySceneName) return;
            if (!IsInMenu()) return;

            if (!_localPlayerInitialized)
            {
                _localPlayerInitialized = true;

                foreach (var id in LocalPlayerManager.s_instance.GetSavedPlayersIndex())
                {
                    RegisterPlayer(conn, false, true, id);
                }

                int bots = maxConnections - _players.Count;

                for (int i = 0; i < bots; i++)
                {
                    AddBot();
                }

                SceneData data = new SceneData()
                {
                    m_sceneName = _mapVoteSceneName,
                    m_teamMode = Enum.Runtime.TeamModeEnum.Unknown
                };

                _onStopMainMenuMusic.Raise();
                _onLoadSceneRequired.Raise(data);
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _onPlayerLeaveLobby?.Raise();
            _onLoadSceneRequired.UnregisterListener(LoadScene);
            _backToMainMenu.UnregisterListener(BackToMainMenu);

            base.OnServerDisconnect(conn);

            //if (!IsInMenu()) return;

            //_playersInLobby.Remove(_playersInLobby.FirstOrDefault(x => x.Key.m_connectionId == conn.connectionId).Key);

            //foreach (var player in _playersInLobby)
            //{
            //    SetPlayerPositionInLobby(player.Key, player.Value);
            //}

            //SteamLobbyManager.s_instance.UpdateLobbyDataPlayerCount(_playersInLobby.Count, this.maxConnections);
            //if (_playersInLobby.Count == 0) SteamLobbyManager.s_instance.UpdateLobbyDataStatus((int)ELobbyType.k_ELobbyTypeInvisible);
        }

        public override void OnClientDisconnect()
        {
            GameManager.s_instance.ResetManager();
            ScoreManager.s_instance.ResetManager();
            VotingManager.s_instance.ResetManager();

            _players.ForEach(x => Destroy(x.gameObject));

            _onStartMainMenuMusic.Raise();

            base.OnClientDisconnect();
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            VolumeReset.s_instance.ResetVolume();

            if (IsInMenu()) return;
            if (_playersInGame == null) InstantiateInGamePlayers();

            if (SceneLoader.s_instance.GetCurrentActiveScene() == _roundResultSceneName)
                EndRoundManager.s_instance.LoadResult(_playersInGame);

            if (!_managerSetup && _teamMode != TeamModeEnum.Unknown)
            {
                _managerSetup = true;
                GameManager.s_instance.SetupManagers(_playersInGame, _teamMode);
            }

            if (_sceneType == SceneType.Game)
            {
                GameManager.s_instance.SetupManagersOnNewRound(_playersInGame, _teamMode);
                Invoke(nameof(ManagersReady), 0.5f);
            }

            //else if (_sceneType == SceneType.Vote)
            //    VotingManager.s_instance.PickupRandomMode(_teamMode, _players.Count);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            //if (SceneLoader.s_instance.GetCurrentActiveScene() == _lobbySceneName) return;
            if (IsInMenu()) return;

            VolumeReset.s_instance.ResetVolume();

            if (_roundLaunched) _roundLaunched = false;

            if (_players.FirstOrDefault(x => x.m_connectionId == conn.connectionId).isServer)
            {
                _numberOfPlayerHasGameSceneLoaded += _players.Count(x => x.m_IsAI);
                _numberOfPlayerHasGameSceneLoaded += _players.Count(x => !x.m_IsAI && x.m_IsLocalPlayer && x.m_localPlayerIndex != 1);
            }

            _numberOfPlayerHasGameSceneLoaded++;
            if (_numberOfPlayerHasGameSceneLoaded == _players.Count)
            {
                _onAllPlayerAsSceneLoaded?.Raise();

                //disable player feedbacks
                _playerFeedbacks?.Raise(false);

                if (_sceneType == SceneType.Vote)
                {
                    List<int> materialsIndex = new();

                    foreach (var item in _playersIndexAndLocalIndex.Where(x => x.Value != -1))
                    {
                        materialsIndex.Add(_playersMaterialsIndex[item.Key]);
                    }

                    VotingManager.s_instance.PickupRandomMode(_teamMode, _players.Count, _playersIndexAndLocalIndex, materialsIndex);
                }


                if (_sceneType == SceneType.Game)
                {
                    //enable player feedbacks
                    //_playerFeedbacks?.Raise(true);

                    //_loadingScreenTimer = new(_timeOnLoadingScreen, OnLoadingScreenTimerOver);
                    //_loadingScreenTimer.OnValueChanged += OnLoadingScreenTimerChanged;
                    //_loadingScreenTimer.Start();
                }
                else
                {
                    if (_roundLaunched) return;
                    _roundLaunched = true;

                    if (_sceneType == SceneType.Result) _onSetupRoundResultScene?.Raise();

                    _netSceneManager.OnSceneLoaded(LaunchGame);
                }
            }

            if (_players.Count(x => !x.m_isReady) == 0)
            {
                if (_roundLaunched) return;
                _roundLaunched = true;

                _players.ForEach(x => x.m_isReady = false);
                _netSceneManager.OnSceneLoaded(LaunchGame);
            }
        }

        private void ManagersReady()
        {
            _onManagersReady?.Raise();
        }

        private void LoadScene(object data)
        {
            SceneData sceneData = data as SceneData;

            _currentSceneData = sceneData;
            _teamMode = sceneData.m_teamMode;

            if (sceneData.m_sceneName == _roundResultSceneName) _sceneType = SceneType.Result;
            else if (sceneData.m_sceneName == _mapVoteSceneName) _sceneType = SceneType.Vote;
            else _sceneType = SceneType.Game;

            //if (SceneLoader.s_instance.GetCurrentActiveScene() == _lobbySceneName)
            if (IsInMenu())
            {
                //_playersMaterialsIndex = LobbyManager.s_instance.GetPlayerMaterialAndIndex();
                //_availableMaterials = LobbyManager.s_instance.GetAvailableMaterials();

                _playersMaterialsIndex = LocalPlayerManager.s_instance.GetPlayerMaterialAndIndex();
                _availableMaterials = LocalPlayerManager.s_instance.GetAvailableMaterials();
            }

            foreach (var p in _players)
            {
                p.m_isReady = p.m_alwaysReady ? true : false;
            }

            _netSceneManager = new(this, _players, _durationLoadScene, _sceneType == SceneType.Game);
            _netSceneManager.LoadScene(sceneData.m_sceneName);
        }

        public string GetCurrentSceneDataName() => _currentSceneData.m_sceneName;

        public void RegisterPlayer(NetworkPlayer player) => _players.Add(player);

        public void UnregisterPlayer(NetworkPlayer player) => _players.Remove(player);

        public void PlayerIsReady()
        {
            if (_roundLaunched) return;
            if (_players.Count(x => !x.m_isReady) > 0) return;
            _roundLaunched = true;
            //_loadingScreenTimer.Stop();
            _onStartSpline.Raise();
            _netSceneManager.OnSceneLoaded(LaunchGame);
        }

        public void SetAllPlayerReady()
        {
            _players.ForEach(x => x.m_isReady = true);
        }

        private void RegisterPlayer(NetworkConnectionToClient conn, bool isAI, bool isLocalPlayer, int localPlayerIndex)
        {
            //if (SceneLoader.s_instance.GetCurrentActiveScene() != _lobbySceneName) return;
            if (!IsInMenu()) return;

            if (_playerPositionsInLobby == null)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerPosition");
                _playerPositionsInLobby = new Vector3[objs.Length];

                for (int i = 0; i < objs.Length; i++)
                {
                    _playerPositionsInLobby[i] = objs[i].transform.position;
                }
            }

            NetworkPlayer playerNet = Instantiate(_netPlayerPrefab);

            _players.Add(playerNet);

            playerNet.m_IsAI = isAI;
            playerNet.m_IsLocalPlayer = isLocalPlayer;
            if (isLocalPlayer) playerNet.m_localPlayerIndex = localPlayerIndex;

            if (!isAI && localPlayerIndex == 1)
            {
                playerNet.m_connectionId = conn.connectionId;
                NetworkServer.AddPlayerForConnection(conn, playerNet.gameObject);
            }

            if (isAI) playerNet.SetupAI();
            if (isLocalPlayer && localPlayerIndex != 1) playerNet.SetupLocalPlayer(localPlayerIndex);

            GameObject lobbyPlayer = Instantiate(_playerLobbyPrefab);

            _playersInLobby.Add(playerNet, lobbyPlayer);
            SetPlayerPositionInLobby(playerNet, lobbyPlayer);

            lobbyPlayer.GetComponent<PlayerLobbyNetwork>().SetPlayerIndexAndAI(playerNet.m_playerIndex, playerNet.m_localPlayerIndex, isAI);

            GameObject owner;
            if (isAI || (isLocalPlayer && localPlayerIndex != 1)) owner = _players.FirstOrDefault(x => x.isServer).gameObject;
            else owner = playerNet.gameObject;

            //IPlayerGraphic graphics = lobbyPlayer.GetComponent<IPlayerGraphic>();
            //if (isAI)
            //{
            //    graphics.UseRandomMeshes();
            //    _botCustomisationsData.Add(graphics.GetRandomCustomisationData());
            //}

            if (!isAI)
            {
                //graphics.SetLocalPlayerID(localPlayerIndex - 1);
                _playersIndexAndLocalIndex.Add(playerNet.m_localPlayerIndex, playerNet.m_playerIndex);
            }

            NetworkServer.Spawn(lobbyPlayer, owner);
        }

        #endregion


        #region Utils & Tools

        private void BackToMainMenu()
        {
            _onStopMusic.Raise();
            DisplayLoadingScreenAndStopHost();
        }

        private async void DisplayLoadingScreenAndStopHost()
        {
            _onFadeIn?.Raise(null);

            float duration = _durationLoadScene.m_duration + _durationLoadScene.m_delay;

            int delay = Mathf.RoundToInt(duration * 1000);
            await Task.Delay(delay);

            VolumeReset.s_instance.ResetVolume();

            StopHost();
        }

        private bool IsInMenu()
        {
            return SceneLoader.s_instance.GetCurrentActiveScene().Contains("MainMenu");
        }

        public bool IsGameScene() => _sceneType == SceneType.Game;

        public void AddBot()
        {
            if (_players.Count == maxConnections) return;
            RegisterPlayer(null, true, false, 0);
        }

        private void OnLoadingScreenTimerOver()
        {
            if (_roundLaunched) return;
            _roundLaunched = true;
            _onStartSpline.Raise();
            _netSceneManager.OnSceneLoaded(LaunchGame);
        }

        private void OnLoadingScreenTimerChanged(float value)
        {
            int remainingTime = Mathf.RoundToInt(value);
            if (remainingTime == _remainingLoadingScreenTime) return;

            _remainingLoadingScreenTime = remainingTime;
            _players.ForEach(x => x.m_loadingScreenRemainingTime = value);
        }

        private void LaunchGame()
        {
            _numberOfPlayerHasGameSceneLoaded = 0;

            switch (_sceneType)
            {
                case SceneType.Game:
                    _onGameStart.Raise();
                    break;

                case SceneType.Vote:
                    _onMapVoteStart.Raise();
                    break;

                case SceneType.Result:
                    _onRoundResultStart.Raise();
                    break;
            }
        }

        private void InstantiateInGamePlayers()
        {
            _playersInGame = new();

            int id = 0;

            foreach (var player in _players.Where(x => !x.m_IsAI))
            {
                GameObject playerObj = Instantiate(playerPrefab);

                if (player.m_localPlayerIndex == 1) NetworkServer.Spawn(playerObj, player.connectionToClient);
                else NetworkServer.Spawn(playerObj, _players.FirstOrDefault(x => x.isServer).connectionToClient);

                _playersInGame.Add(playerObj);

                IPlayerGraphic graphics = playerObj.GetComponent<IPlayerGraphic>();
                graphics.SetLocalPlayerID(player.m_localPlayerIndex - 1);
                int materialIndex = _playersMaterialsIndex[player.m_localPlayerIndex];
                graphics.SetMaterialIndex(materialIndex);

                LocalPlayerData settings = LocalPlayerManager.s_instance.GetSettingsForPlayer(player.m_localPlayerIndex);

                IPlayer pl = playerObj.GetComponent<IPlayer>();
                pl.SetLocalPlayerID(player.m_localPlayerIndex,false);
                pl.SetWorldOrientation(settings.m_isWorldOrientation);
                pl.SetUseRumble(settings.m_useRumble);

                IFeedback feedback = playerObj.GetComponent<IFeedback>();
                feedback.SetID(id);
                feedback.UseRumble(settings.m_useRumble);

                IAI ai = playerObj.GetComponent<IAI>();
                ai.DisableAI();
                id++;
            }

            List<int> usedMatForBot = new List<int>();

            int botIndex = 0;

            foreach (var player in _players.Where(x => x.m_IsAI))
            {
                GameObject playerObj = Instantiate(playerPrefab);
                NetworkServer.Spawn(playerObj, _players.FirstOrDefault(x => x.isServer).connectionToClient);

                _playersInGame.Add(playerObj);

                int matIndex = _availableMaterials.Where(x => !usedMatForBot.Contains(x)).GetRandom();
                usedMatForBot.Add(matIndex);

                IPlayer pl = playerObj.GetComponent<IPlayer>();
                pl.SetLocalPlayerID(player.m_playerIndex,true);

                playerObj.GetComponent<IFeedback>().SetID(id);

                IPlayerGraphic graphics = playerObj.GetComponent<IPlayerGraphic>();
                graphics.UseRandomMeshes();
                //graphics.UseRandomMeshes(_botCustomisationsData[botIndex]);
                graphics.SetMaterialIndex(matIndex);

                IAI ai = playerObj.GetComponent<IAI>();
                ai.EnableAI();

                botIndex++;
                id++;
            }

            _playersInGame.ForEach(x => DontDestroyOnLoad(x));
        }

        private void SetPlayerPositionInLobby(NetworkPlayer playerNet, GameObject go)
        {
            int index = _playersInLobby.Values.ToList().IndexOf(go);
            //go.transform.position = _playerPositionsInLobby[index];

            playerNet.m_playerIndex = index;
        }

        public List<GameObject> GetPlayersInLobby() => _playersInLobby.Values.ToList();

        #endregion


        #region Private

        private List<NetworkPlayer> _players = new();
        private List<GameObject> _playersInGame;
        private Dictionary<NetworkPlayer, GameObject> _playersInLobby = new();

        private Dictionary<int, int> _playersMaterialsIndex;
        private Dictionary<int, int> _playersIndexAndLocalIndex;
        private List<int> _availableMaterials;

        private Vector3[] _playerPositionsInLobby;

        private List<object> _botCustomisationsData = new();

        private bool _managerSetup;
        private bool _roundLaunched;
        private bool _localPlayerInitialized;

        private Timer _loadingScreenTimer;

        private int _numberOfPlayerHasGameSceneLoaded;
        private int _remainingLoadingScreenTime;

        private SceneType _sceneType;

        private TeamModeEnum _teamMode;

        private NetworkSceneManager _netSceneManager;

        private SceneData _currentSceneData;

        #endregion
    }
}