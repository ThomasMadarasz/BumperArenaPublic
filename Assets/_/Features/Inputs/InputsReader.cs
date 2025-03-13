using Interfaces.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.InputSystem.InputAction;

namespace Inputs.Runtime
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputsReader : MonoBehaviour, IAvancedInputsReader
    {
        #region Exposed

        public event Action m_onBoostPerformed;

        public event Action m_onMenuPerformed;

        public event Action<Vector2> m_onNavigatePerformed;
        public event Action m_onSubmitPerformed;
        public event Action m_onSubmitStarted;
        public event Action m_onSubmitCanceled;
        public event Action m_onCancelPerformed;

        public event Action<int> m_onAdvancedSubmitPerformed;
        public event Action<int> m_onAdvancedCancelPerformed;

        public event Action m_onNorthButtonPerformed;
        public event Action m_onWestButtonPerformed;

        public event Action m_onRightShoulderPerformed;
        public event Action m_onLeftShoulderPerformed;

        public event Action<float> m_onRightTriggerPerformed;
        public event Action<float> m_onLeftTriggerPerformed;

        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private float _timeBetweenNavigateEvent;

        #endregion


        #region Unity API

        private void OnEnable()
        {
            RegisterEvent();
        }

        private void Update()
        {
            if (!_canSendNavigationInputEvent)
            {
                _remainingTime -= Time.unscaledDeltaTime;
                if (_remainingTime <= 0) _canSendNavigationInputEvent = true;
                return;
            }

            if (_playerInput.currentActionMap.name == "UI")
            {
                float rightTriggerValue = _playerInput.actions["RightTrigger"].ReadValue<float>();
                if (rightTriggerValue > 0.1f)
                {
                    m_onRightTriggerPerformed?.Invoke(rightTriggerValue);
                }

                float leftTriggerValue = _playerInput.actions["LeftTrigger"].ReadValue<float>();
                if (leftTriggerValue > 0.1f)
                {
                    m_onLeftTriggerPerformed?.Invoke(leftTriggerValue);
                }
            }
        }

        #endregion


        #region Main

        public Vector2 GetMoveDirection() => _playerInput.actions["Move"].ReadValue<Vector2>();
        public Vector2 GetClientMoveDirection() => _clientInputDirection;

        public void SetClientMoveDirection(Vector2 value) => _clientInputDirection = value;

        public void EnableActionMap(string mapName)
        {
            _playerInput.actions["Menu"].performed -= OnMenu_Performed;

            if (!_playerInput.actions.FindActionMap(mapName).enabled) _playerInput.actions.FindActionMap(mapName).Enable();
            if (_playerInput.currentActionMap != _playerInput.actions.FindActionMap(mapName)) _playerInput.SwitchCurrentActionMap(mapName);

            _playerInput.actions["Menu"].performed += OnMenu_Performed;

            ReadOnlyArray<InputDevice>? newDevices = new(_currentDevices.ToArray());
            _playerInput.currentActionMap.devices = newDevices;
        }

        public void DisableActionMap(string mapName)
        {
            if (_playerInput.actions.FindActionMap(mapName).enabled) _playerInput.actions.FindActionMap(mapName).Disable();
        }

        public void ActivateInput()
        {
            _playerInput.enabled = true;
            _playerInput.ActivateInput();
        }

        public void DeactivateInput()
        {
            UnregisterEvent();
            _playerInput.DeactivateInput();
        }

        public void UnpairDevicesAndRemoveUser()
        {
            _playerInput.user.UnpairDevicesAndRemoveUser();
        }

        #endregion


        #region Utils & Tools

        public bool IsCurrentActionMapIsUI() => _playerInput.currentActionMap.name == "UI";

        private InputDevice GetDeviceWithID(int id)
        {
            return InputSystem.GetDeviceById(id);
        }

        public void SetMainDevice(int device)
        {
            _mainDevice = GetDeviceWithID(device);
        }

        public int GetMainDeviceID() => _mainDevice.deviceId;

        public void SetDevicesInCurrentActionMap(IEnumerable<int> devices)
        {
            List<InputDevice> deviceList = new();
            _currentDevices.Clear();

            foreach (var id in devices)
            {
                deviceList.Add(GetDeviceWithID(id));
            }

            foreach (var device in deviceList)
            {
                InputUser.PerformPairingWithDevice(device, _playerInput.user);
            }

            _currentDevices.AddRange(deviceList);

            ReadOnlyArray<InputDevice>? newDevices = new(deviceList.ToArray());
            _playerInput.currentActionMap.devices = newDevices;
        }

        public void SetAvailableDevice(IEnumerable<int> devices)
        {
            _availableDevice = new();

            foreach (var id in devices)
            {
                _availableDevice.Add(GetDeviceWithID(id));
            }
        }

        public IEnumerable<int> GetAvailableDevices()
        {
            return _availableDevice.Select(x => x.deviceId);
        }

        public List<int> GetAllDevices()
        {
            return InputSystem.devices.Select(x => x.deviceId).ToList();
        }

        public bool SubmitButtonPressed()
        {
            return _playerInput.actions["Boost"].ReadValue<float>() > .1f;
        }

        public List<InputDevice> GetCurrentDevices() => _currentDevices;

        #endregion


        #region Events

        private void RegisterEvent()
        {
            _playerInput.actions["Boost"].performed += OnBoost_Performed;
            _playerInput.actions["Menu"].performed += OnMenu_Performed;

            _playerInput.actions["Navigate"].performed += OnNavigate_Performed;
            _playerInput.actions["Submit"].performed += OnSubmit_Performed;
            _playerInput.actions["Submit"].started += OnSubmit_Started;
            _playerInput.actions["Submit"].canceled += OnSubmit_Canceled;
            _playerInput.actions["Cancel"].performed += OnCancel_Performed;

            _playerInput.actions["NorthButton"].performed += OnNorthButton_Performed;
            _playerInput.actions["WestButton"].performed += OnWestButton_Performed;

            _playerInput.actions["RightShoulder"].performed += OnRightShoulder_Performed;
            _playerInput.actions["LeftShoulder"].performed += OnLeftShoulder_Performed;

            _playerInput.actions["Submit"].performed += OnAdvancedSubmit_Performed;
            _playerInput.actions["Cancel"].performed += OnAdvancedCancel_Performed;

            _playerInput.onDeviceLost += OnDeviceLost;
            _playerInput.onDeviceRegained += OnDeviceRegained;
        }

        private void UnregisterEvent()
        {
            _playerInput.actions["Boost"].performed -= OnBoost_Performed;
            _playerInput.actions["Menu"].performed -= OnMenu_Performed;

            _playerInput.actions["Navigate"].performed -= OnNavigate_Performed;
            _playerInput.actions["Submit"].performed -= OnSubmit_Performed;
            _playerInput.actions["Submit"].started -= OnSubmit_Started;
            _playerInput.actions["Submit"].canceled -= OnSubmit_Canceled;
            _playerInput.actions["Cancel"].performed -= OnCancel_Performed;

            _playerInput.actions["NorthButton"].performed -= OnNorthButton_Performed;
            _playerInput.actions["WestButton"].performed -= OnWestButton_Performed;

            _playerInput.actions["RightShoulder"].performed -= OnRightShoulder_Performed;
            _playerInput.actions["LeftShoulder"].performed -= OnLeftShoulder_Performed;

            _playerInput.actions["Submit"].performed -= OnAdvancedSubmit_Performed;
            _playerInput.actions["Cancel"].performed -= OnAdvancedCancel_Performed;

            _playerInput.onDeviceLost -= OnDeviceLost;
            _playerInput.onDeviceRegained -= OnDeviceRegained;
        }

        private void OnDeviceLost(PlayerInput input)
        { }

        private void OnDeviceRegained(PlayerInput input)
        {
            if (_playerInput.playerIndex != input.playerIndex) return;

            ReadOnlyArray<InputDevice>? newDevices = new(_currentDevices.ToArray());
            _playerInput.currentActionMap.devices = newDevices;
        }

        private void OnBoost_Performed(CallbackContext context) => m_onBoostPerformed?.Invoke();

        private void OnMenu_Performed(CallbackContext context) => m_onMenuPerformed?.Invoke();

        private void OnNavigate_Performed(CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();

            if (!_canSendNavigationInputEvent) return;
            if (value == Vector2.zero) return;
            _remainingTime = _timeBetweenNavigateEvent;
            _canSendNavigationInputEvent = false;

            m_onNavigatePerformed?.Invoke(value);
        }
        private void OnSubmit_Performed(CallbackContext context) => m_onSubmitPerformed?.Invoke();

        private void OnSubmit_Started(CallbackContext context) => m_onSubmitStarted?.Invoke();

        private void OnSubmit_Canceled(CallbackContext context) => m_onSubmitCanceled?.Invoke();

        private void OnCancel_Performed(CallbackContext context) => m_onCancelPerformed?.Invoke();

        private void OnAdvancedSubmit_Performed(CallbackContext context)
        {
            m_onAdvancedSubmitPerformed?.Invoke(context.control.device.deviceId);
        }
        private void OnAdvancedCancel_Performed(CallbackContext context)
        {
            m_onAdvancedCancelPerformed?.Invoke(context.control.device.deviceId);
        }

        private void OnNorthButton_Performed(CallbackContext context) => m_onNorthButtonPerformed?.Invoke();
        private void OnWestButton_Performed(CallbackContext context) => m_onWestButtonPerformed?.Invoke();

        private void OnRightShoulder_Performed(CallbackContext context) => m_onRightShoulderPerformed?.Invoke();
        private void OnLeftShoulder_Performed(CallbackContext context) => m_onLeftShoulderPerformed?.Invoke();

        public void AddNewDevice(int deviceID)
        {
            InputDevice device = GetDeviceWithID(deviceID);

            _availableDevice.Add(device);
            _currentDevices.Add(device);

            ReadOnlyArray<InputDevice>? newDevices = new(_currentDevices.ToArray());
            _playerInput.currentActionMap.devices = newDevices;
        }

        #endregion


        #region Private

        private List<InputDevice> _availableDevice;
        private List<InputDevice> _currentDevices = new();
        private InputDevice _mainDevice;

        private Vector2 _clientInputDirection;

        private bool _canSendNavigationInputEvent = true;

        private float _remainingTime;

        #endregion
    }
}