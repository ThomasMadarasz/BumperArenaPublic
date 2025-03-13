using Archi.Runtime;
using Data.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalPlayer.Runtime
{
    public class LocalPlayerManager : CBehaviour
    {
        #region Exposed

        public static LocalPlayerManager s_instance;

        [SerializeField] private LPlayer _localPlayerPrefab;

        [SerializeField] private GameEvent _onJoinLobby;
        [SerializeField] private GameEvent _onDisconnected;
        [SerializeField] private GameEventT _onRumbleValueChanged;
        [SerializeField] private GameEventT _onOrientationValueChanged;

        [SerializeField] private CustomisationData _customisationData;

        [HideInInspector] public Action<int, Enum.Runtime.DeviceType> m_onLocalPlayerAdded;
        [HideInInspector] public Action<int> m_onLocalPlayerRemoved;
        [HideInInspector] public Action<int, Color> m_onLocalPlayerColorChanged;
        [HideInInspector] public Action<List<KeyValuePair<int, int>>> m_onReorderedPlayers;

        public int m_localPlayerCount => _localPlayers.Count;

        public List<int> GetPlayerIndex()
        {
            return _localPlayers.Select(x => x.m_localPlayerID).ToList();
        }

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            _onJoinLobby.RegisterListener(OnJoinLobby);
            _onDisconnected.RegisterListener(OnDisconnected);

            _onRumbleValueChanged.RegisterListener(OnLocalPlayerRumbleValueChanged);
            _onOrientationValueChanged.RegisterListener(OnLocalPlayerOrientationValueChanged);

            InitColors();
            CreateDefaultPlayer(false);
        }

        private void InitColors()
        {
            _playerColorAvailability.Clear();
            _playerColorIndex.Clear();

            foreach (var item in _customisationData.m_materials)
            {
                _playerColorAvailability.Add(true);
            }

            foreach (var item in _customisationData.m_materials)
            {
                _playerColorIndex.Add(-1);
            }
        }

        public int ChangeToNextColorFrom(int localPlayerID, int materialIndex)
        {
            int index = materialIndex;
            SetColorToAvailable(index);

            bool find = false;

            do
            {
                index++;
                index %= _customisationData.m_materials.Length;

                if (_playerColorAvailability[index])
                    find = true;

            } while (!find);

            _playerColorAvailability[index] = false;
            _playerColorIndex[index] = localPlayerID;

            m_onLocalPlayerColorChanged?.Invoke(localPlayerID, GetMaterialFromIndex(index).color);

            return index;
        }

        public int GetFirstAvailableMaterialIndex(int localPlayerID)
        {
            int index = 0;

            for (int i = 0; i < _customisationData.m_materials.Length; i++)
            {
                if (_playerColorAvailability[i])
                {
                    index = i;
                    break;
                }
            }
            _playerColorAvailability[index] = false;
            _playerColorIndex[index] = localPlayerID;

            return index;
        }

        public Material GetMaterialFromIndex(int index)
        {
            return _customisationData.m_materials[index];
        }

        public Color GetColorForPlayerID(int playerID)
        {
            return _localPlayers.FirstOrDefault(x => x.m_localPlayerID == playerID).m_materialColor;
        }

        public Dictionary<int, int> GetPlayerMaterialAndIndex()
        {
            Dictionary<int, int> colorIndex = new();

            for (int i = 0; i < _playerColorAvailability.Count; i++)
            {
                if (_playerColorAvailability[i]) continue;

                colorIndex.Add(_playerColorIndex[i], i);
            }

            return colorIndex;
        }

        public List<int> GetAvailableMaterials()
        {
            List<int> colors = new();

            for (int i = 0; i < _playerColorAvailability.Count; i++)
            {
                if (!_playerColorAvailability[i]) continue;

                colors.Add(i);
            }

            return colors;
        }

        public void SetColorToAvailable(int index)
        {
            _playerColorAvailability[index] = true;
            _playerColorIndex[index] = -1;
        }

        public LocalPlayerData GetSettingsForPlayer(int playerID)
        {
            return _savedPlayers.FirstOrDefault(x => x.m_localPlayerID == playerID);
        }

        public void OnPlayerPositionsSceneChanged()
        {
            UpdatePlayerPositions(false);
        }

        public void EnablePlayerModification()
        {
            foreach (var p in _localPlayers)
            {
                Destroy(p.gameObject);
            }

            _localPlayers.Clear();

            if (_configPlayer != null)
            {
                Destroy(_configPlayer.gameObject);
                _configPlayer = null;
            }

            InitColors();
            CreateConfigPlayer();

            _configPlayer.m_onAdvancedSubmitPerformed += OnAdvancedSubmit_Performed;
            _configPlayer.m_onAdvancedCancelPerformed += OnAdvancedCancel_Performed;
        }

        public void DisablePlayerModification()
        {
            _configPlayer.m_onAdvancedSubmitPerformed -= OnAdvancedSubmit_Performed;
            _configPlayer.m_onAdvancedCancelPerformed -= OnAdvancedCancel_Performed;

            if (_configPlayer != null)
            {
                Destroy(_configPlayer.gameObject);
                _configPlayer = null;
            }
        }

        private void AddPlayer(int playerID, int deviceID)
        {
            IAvancedInputsReader configUnput = _configPlayer.GetInputReader();

            InputDevice device = InputSystem.GetDeviceById(deviceID);

            IEnumerable<int> availableDevices = configUnput.GetAvailableDevices();
            List<InputDevice> devices = new();
            foreach (var item in availableDevices)
            {
                devices.Add(InputSystem.GetDeviceById(item));
            }

            devices = devices.Where(x => x.deviceId != deviceID).ToList();

            configUnput.SetAvailableDevice(devices.Select(x => x.deviceId));
            configUnput.SetDevicesInCurrentActionMap(devices.Select(x => x.deviceId));

            CreateLocalPlayer(device);
        }

        public void RemovePlayer(int playerID)
        {
            LPlayer player = GetPlayerByIndex(playerID);
            if (player.m_isConfigPlayer) return;

            SetColorToAvailable(player.m_materialIndex);

            IAvancedInputsReader input = player.GetInputReader();
            int deviceID = input.GetMainDeviceID();

            input.DeactivateInput();
            input.UnpairDevicesAndRemoveUser();

            _localPlayers.Remove(player);
            Destroy(player.gameObject);

            m_onLocalPlayerRemoved?.Invoke(playerID);

            IAvancedInputsReader configInput = _configPlayer.GetInputReader();
            IEnumerable<int> availableDevices = configInput.GetAvailableDevices();

            List<int> devices = availableDevices.ToList();
            devices.Add(deviceID);

            configInput.SetAvailableDevice(devices);
            configInput.SetDevicesInCurrentActionMap(devices);

            ReorderPlayer();
        }

        private void ReorderPlayer()
        {
            int i = 1;

            List<KeyValuePair<int, int>> ids = new();

            foreach (var pl in _localPlayers)
            {
                _playerColorIndex[pl.m_materialIndex] = -1;

                ids.Add(new(pl.m_localPlayerID, i));
                pl.m_localPlayerID = i;

                _playerColorIndex[pl.m_materialIndex] = i;
                i++;
            }

            m_onReorderedPlayers?.Invoke(ids);
        }

        public void RemoveAllPlayers(int playerID)
        {
            LPlayer player = GetPlayerByIndex(playerID);
            if (player == null || player.m_isConfigPlayer) return;

            List<int> ids = _localPlayers.Select(x => x.m_localPlayerID).ToList();
            //ids.ForEach(x => RemovePlayer(x));
            for (int i = 0; i < ids.Count; i++)
            {
                RemovePlayer(_localPlayers[0].m_localPlayerID);
            }
        }

        public void ValidatePlayers(int playerID)
        {
            LPlayer player = GetPlayerByIndex(playerID);
            if (player.m_isConfigPlayer && _localPlayers.Count == 0)
            {
                CreateDefaultPlayer(false);
                return;
            }

            UpdatePlayerPositions(false);

            foreach (var p in _localPlayers)
            {
                p.ValidatePlayerConfig();

                IPlayerCustomisation custom = p.GetComponent<IPlayerCustomisation>();
                custom.SetLocalPlayerID(p.m_localPlayerID);
                custom.ShowGraphics();
            }
        }

        public List<int> GetSavedPlayersIndex() => _savedPlayers.Select(x => x.m_localPlayerID).ToList();

        public int GetDeviceForLocalPlayer(int id)
        {
            if (_savedPlayers.Count == 0)
            {
                LPlayer lp = _localPlayers.FirstOrDefault(x => x.m_localPlayerID == id);
                IAvancedInputsReader input = lp.GetInputReader();
                return input.GetMainDeviceID();
            }
            return _savedPlayers.FirstOrDefault(x => x.m_localPlayerID == id).m_deviceID;
        }

        public bool IsDefaultPlayer(int id)
        {
            return _savedPlayers.FirstOrDefault(x => x.m_localPlayerID == id).m_isDefaultPlayer;
        }

        public List<GameObject> GetPlayersInMenu()
        {
            return _localPlayers.Select(x => x.gameObject).ToList();
        }

        #endregion


        #region Utils & Tools

        private void OnLocalPlayerRumbleValueChanged(object obj)
        {
            if (_localPlayers.Count == 0) return;
            object[] values = (object[])obj;
            int playerID = (int)values[0];
            bool useRumble = (bool)values[1];

            _localPlayers.FirstOrDefault(x => x.m_localPlayerID == playerID).ForceRumbleValue(useRumble);
        }

        private void OnLocalPlayerOrientationValueChanged(object obj)
        {
            if (_localPlayers.Count == 0) return;
            object[] values = (object[])obj;
            int playerID = (int)values[0];
            bool useWorldOrientation = (bool)values[1];

            _localPlayers.FirstOrDefault(x => x.m_localPlayerID == playerID).ForceOrientationValue(useWorldOrientation);
        }

        private void CreateConfigPlayer()
        {
            _configPlayer = Instantiate(_localPlayerPrefab);
            DontDestroyOnLoad(_configPlayer.gameObject);
            _configPlayer.gameObject.name = "ConfigPlayer";

            _configPlayer.m_localPlayerID = 0;
            _configPlayer.m_isConfigPlayer = true;
            _configPlayer.m_isDefaultPlayer = false;

            IAvancedInputsReader input = _configPlayer.GetInputReader();

            IEnumerable<int> availableDeviceId = GetAllConnectedDevices().Select(x => x.deviceId);
            input.SetAvailableDevice(availableDeviceId);
            input.SetDevicesInCurrentActionMap(availableDeviceId);
        }

        private void CreateDefaultPlayer(bool useColor)
        {
            LPlayer player1 = Instantiate(_localPlayerPrefab);
            DontDestroyOnLoad(player1.gameObject);
            player1.gameObject.name = "DefaultPlayer";

            player1.m_localPlayerID = 1;
            player1.m_isConfigPlayer = false;
            player1.m_isDefaultPlayer = true;

            IAvancedInputsReader input = player1.GetInputReader();

            IEnumerable<int> availableDeviceId = GetAllConnectedDevices().Select(x => x.deviceId);
            input.SetAvailableDevice(availableDeviceId);
            input.SetDevicesInCurrentActionMap(availableDeviceId);

            player1.ValidatePlayerConfig();

            _localPlayers.Add(player1);

            int materialId = 0;

            if (useColor) materialId = _playerColorIndex.IndexOf(player1.m_localPlayerID);
            else materialId = GetFirstAvailableMaterialIndex(player1.m_localPlayerID);


            player1.SetMaterialID(materialId);

            LocalPlayerData config = GetSettingsForPlayer(player1.m_localPlayerID);
            if (config != null) player1.SetConfig(config);


            IPlayerCustomisation custom = player1.GetComponent<IPlayerCustomisation>();
            custom.SetLocalPlayerID(player1.m_localPlayerID);
            custom.ShowGraphics();

            UpdatePlayerPositions(false);
        }

        private void CreateLocalPlayer(InputDevice device, int localPlayerID = -1)
        {
            PlayerInput pi = PlayerInput.Instantiate(_localPlayerPrefab.gameObject, pairWithDevice: device);
            PlayerInput.DontDestroyOnLoad(pi.gameObject);
            LPlayer newPlayer = pi.GetComponent<LPlayer>();

            newPlayer.m_isConfigPlayer = false;
            newPlayer.m_isDefaultPlayer = false;

            if (localPlayerID == -1) newPlayer.m_localPlayerID = GetAvailablePlayerID();
            else newPlayer.m_localPlayerID = localPlayerID;
            _localPlayers.Add(newPlayer);

            IAvancedInputsReader newPlayerInput = newPlayer.GetInputReader();
            newPlayerInput.SetMainDevice(device.deviceId);
            newPlayerInput.SetDevicesInCurrentActionMap(new List<int>() { device.deviceId });

            Enum.Runtime.DeviceType type = DeviceHelper.Runtime.DeviceHelper.GetDeviceType(device);

            int materialId = 0;

            if (localPlayerID == -1)
                materialId = GetFirstAvailableMaterialIndex(newPlayer.m_localPlayerID);
            else
                materialId = _playerColorIndex.IndexOf(localPlayerID);

            newPlayer.SetMaterialID(materialId);

            if (localPlayerID != -1)
            {
                newPlayer.SetConfig(GetSettingsForPlayer(localPlayerID));
            }

            Debug.Log("New player added : " + type.ToString());

            m_onLocalPlayerAdded?.Invoke(newPlayer.m_localPlayerID, type);
        }

        private InputDevice[] GetAllConnectedDevices()
        {
            return InputSystem.devices.ToArray();
        }

        public LPlayer GetPlayerByIndex(int id)
        {
            if (_configPlayer.m_localPlayerID == id) return _configPlayer;
            else return _localPlayers.FirstOrDefault(x => x.m_localPlayerID == id);
        }

        private int GetAvailablePlayerID()
        {
            List<int> AllIds = new List<int>() { 1, 2, 3, 4 };

            List<int> usedID = _localPlayers.Select(x => x.m_localPlayerID).ToList();

            List<int> availableIds = AllIds.Where(x => !usedID.Contains(x)).ToList();

            return availableIds.FirstOrDefault();
        }

        private void UpdatePlayerPositions(bool inGarage)
        {
            if (LocalPlayerPositionManager.s_instance == null) return;

            Transform[] pos = LocalPlayerPositionManager.s_instance.GetPositionFor(_localPlayers.Count, inGarage);

            for (int i = 0; i < _localPlayers.Count; i++)
            {
                _localPlayers[i].transform.position = pos[i].position;
                _localPlayers[i].transform.rotation = pos[i].rotation;
            }
        }

        public void UpdatePlayerPositionForGarage() => UpdatePlayerPositions(true);

        public void UpdatePlayerPositionForBasePosition() => UpdatePlayerPositions(false);

        private void OnJoinLobby()
        {
            _savedPlayers.Clear();

            foreach (var p in _localPlayers)
            {
                bool rumble = p.UseRmble();
                bool orientation = p.UseWorldOrientation();

                LocalPlayerData data = new(p.m_localPlayerID, -1, p.m_isDefaultPlayer, rumble, orientation);
                if (!p.m_isDefaultPlayer)
                {
                    IAvancedInputsReader input = p.GetInputReader();
                    int deviceID = input.GetMainDeviceID();

                    data.m_deviceID = deviceID;
                }

                _savedPlayers.Add(data);

                Destroy(p.gameObject);
            }

            _localPlayers.Clear();
        }

        private void OnDisconnected()
        {
            _localPlayers.Clear();

            foreach (var p in _savedPlayers)
            {
                if (p.m_isDefaultPlayer)
                {
                    CreateDefaultPlayer(true);
                }
                else
                {
                    InputDevice device = InputSystem.GetDeviceById(p.m_deviceID);
                    CreateLocalPlayer(device, p.m_localPlayerID);
                }
            }

            _savedPlayers.Clear();

            foreach (var p in _localPlayers)
            {
                IPlayerCustomisation custom = p.GetComponent<IPlayerCustomisation>();
                custom.SetLocalPlayerID(p.m_localPlayerID);
                custom.ShowGraphics();

                p.ValidatePlayerConfig();
            }
        }

        public List<KeyValuePair<int, object[]>> GetConfigurationForLocalPlayers()
        {
            List<KeyValuePair<int, object[]>> config = new();

            if (_localPlayers.Count == 0)
            {
                //in game : _savedPlayers

                foreach (var p in _savedPlayers)
                {
                    int materialID = _playerColorIndex.IndexOf(p.m_localPlayerID);
                    Material playerMat = GetMaterialFromIndex(materialID);
                    object[] values = new object[3] { p.m_useRumble, p.m_isWorldOrientation, playerMat };

                    KeyValuePair<int, object[]> localConfig = new(p.m_localPlayerID, values);
                    config.Add(localConfig);
                }
            }
            else
            {
                // in main menu : _localPlayers

                foreach (var p in _localPlayers)
                {
                    Material playerMat = GetMaterialFromIndex(p.m_materialIndex);
                    object[] values = new object[3] { p.UseRmble(), p.UseWorldOrientation(), playerMat };

                    KeyValuePair<int, object[]> localConfig = new(p.m_localPlayerID, values);
                    config.Add(localConfig);
                }

            }

            return config;
        }

        public void UpdateConfigurationForLocalPlayers(int playerId, bool useRumble, bool useWorldOrientation)
        {
            if (_localPlayers.Count == 0)
            {
                LocalPlayerData playerData = _savedPlayers.FirstOrDefault(x => x.m_localPlayerID == playerId);
                playerData.m_useRumble = useRumble;
                playerData.m_isWorldOrientation = useWorldOrientation;
            }
            else
            {
                LPlayer player = _localPlayers.FirstOrDefault(x => x.m_localPlayerID == playerId);
                player.ForceRumbleValue(useRumble);
                player.ForceOrientationValue(useWorldOrientation);
            }
        }

        #endregion


        #region Events

        private void OnAdvancedSubmit_Performed(int playerID, int deviceID)
        {
            if (_configPlayer.m_localPlayerID != playerID) return;

            bool deviceAlreadyUsed = false;

            foreach (var p in _localPlayers)
            {
                IAvancedInputsReader inputReader = p.GetInputReader();
                if (inputReader.GetMainDeviceID() == deviceID)
                {
                    deviceAlreadyUsed = true;
                    break;
                }
            }

            if (deviceAlreadyUsed) return;

            AddPlayer(playerID, deviceID);
        }

        public void OnClickAdvancedSubmit_Performed()
        {
            InputDevice keyboard = GetAllConnectedDevices().FirstOrDefault(x => x is not Gamepad && x is not Mouse);
            OnAdvancedSubmit_Performed(0, keyboard.deviceId);
        }
        public void OnClickAdvancedCancel_Performed()
        {
            foreach (LPlayer lp in _localPlayers)
            {
                InputDevice inputDevice = InputSystem.GetDeviceById(lp.GetInputReader().GetMainDeviceID());
                if (inputDevice is not Gamepad && inputDevice is not Mouse)
                {
                    RemovePlayer(lp.m_localPlayerID);
                    return;
                }
            }
        }


        public void OnClickRemoveAllPlayers() => RemoveAllPlayers(1);



        private void OnAdvancedCancel_Performed(int playerID, int deviceID)
        {

        }

        #endregion


        #region Private

        private LPlayer _configPlayer;

        private List<LPlayer> _localPlayers = new();
        private List<LocalPlayerData> _savedPlayers = new();

        private List<bool> _playerColorAvailability = new();
        private List<int> _playerColorIndex = new();

        #endregion
    }
}