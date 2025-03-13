using Audio.Runtime;
using Customisation.Runtime;
using Data.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using System;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils.Runtime;

namespace LocalPlayer.Runtime
{
    public class LPlayer : SerializedMonoBehaviour
    {
        #region Exposed

        [SerializeField] private IAvancedInputsReader _inputReader;

        [HideInInspector] public int m_localPlayerID = -1;
        [HideInInspector] public bool m_isConfigPlayer;
        [HideInInspector] public bool m_isDefaultPlayer;

        [HideInInspector] public Action<int, int> m_onAdvancedSubmitPerformed;
        [HideInInspector] public Action<int, int> m_onAdvancedCancelPerformed;
        [HideInInspector] public Action m_onPlayerConfigValidated;
        [HideInInspector] public Action<Color> m_onColorChanged;
        [HideInInspector] public Action<int> m_onPlayerIDChanged;

        [HideInInspector] public Action<bool> m_onWorldOrientationChanged;
        [HideInInspector] public Action<bool> m_onRumbleChanged;

        [HideInInspector] public Action<Color, int> m_onColorSelected;
        [HideInInspector] public Action<int> m_onOrientationSelected;
        [HideInInspector] public Action<int> m_onRumbleSelected;

        [HideInInspector] public int m_materialIndex;
        [HideInInspector] public Color m_materialColor;

        [SerializeField] private GameEvent _onSkipStartup;
        [SerializeField] private GameObject _graphics;
        [SerializeField] private GameObject _UI;

        [SerializeField][BoxGroup("SFX")] SFXData _backSfxData;
        [SerializeField][BoxGroup("SFX")] SFXData _lockSfxData;
        [SerializeField][BoxGroup("SFX")] SFXData _switchSfxData;
        [SerializeField][BoxGroup("SFX")] SFXData _selectSfxData;

        private bool _useWorldOrientation = true;
        private bool _useRumble = true;

        #endregion


        #region Unity API

        private void Awake()
        {
            _graphics.SetActive(false);
            _UI.SetActive(false);
        }

        private void Start()
        {
            _customisationPreview = GetComponent<CustomisationPreview>();
            _customisationPreview.ChangeMaterial(m_materialIndex);
        }

        private void OnEnable()
        {
            _inputReader.m_onMenuPerformed += OnMenu_Performed;
            _inputReader.m_onCancelPerformed += OnCancel_Performed;
            _inputReader.m_onNavigatePerformed += OnNavigate_Performed;
            _inputReader.m_onSubmitPerformed += OnSubmit_Performed;
            _inputReader.m_onSubmitStarted += OnSubmit_Started;
            _inputReader.m_onSubmitCanceled += OnSubmit_Canceled;

            _inputReader.m_onWestButtonPerformed += OnWestButton_Performed;
            _inputReader.m_onNorthButtonPerformed += OnNorthButton_Performed;

            _inputReader.m_onRightShoulderPerformed += OnRightShoulder_Performed;
            _inputReader.m_onLeftShoulderPerformed += OnLeftShoulder_Performed;

            _inputReader.m_onRightTriggerPerformed += OnRightTrigger_Performed;
            _inputReader.m_onLeftTriggerPerformed += OnLeftTrigger_Performed;

            _inputReader.m_onAdvancedSubmitPerformed += OnAdvancedSubmit_Performed;
            _inputReader.m_onAdvancedCancelPerformed += OnAdvancedCancel_Performed;

            InputSystem.onDeviceChange += OnDeviceChanged;
        }

        private void OnDisable()
        {
            _inputReader.m_onMenuPerformed -= OnMenu_Performed;
            _inputReader.m_onCancelPerformed -= OnCancel_Performed;
            _inputReader.m_onNavigatePerformed -= OnNavigate_Performed;
            _inputReader.m_onSubmitPerformed -= OnSubmit_Performed;
            _inputReader.m_onSubmitStarted -= OnSubmit_Started;
            _inputReader.m_onSubmitCanceled -= OnSubmit_Canceled;

            _inputReader.m_onWestButtonPerformed -= OnWestButton_Performed;
            _inputReader.m_onNorthButtonPerformed -= OnNorthButton_Performed;

            _inputReader.m_onRightShoulderPerformed -= OnRightShoulder_Performed;
            _inputReader.m_onLeftShoulderPerformed -= OnLeftShoulder_Performed;

            _inputReader.m_onRightTriggerPerformed -= OnRightTrigger_Performed;
            _inputReader.m_onLeftTriggerPerformed -= OnLeftTrigger_Performed;

            _inputReader.m_onAdvancedSubmitPerformed -= OnAdvancedSubmit_Performed;
            _inputReader.m_onAdvancedCancelPerformed -= OnAdvancedCancel_Performed;

            InputSystem.onDeviceChange -= OnDeviceChanged;
        }

        #endregion


        #region Main

        private void OnDeviceChanged(InputDevice device, InputDeviceChange e)
        {
            if (m_isConfigPlayer || m_isDefaultPlayer)
            {
                if (e == InputDeviceChange.Added)
                {
                    _inputReader.AddNewDevice(device.deviceId);
                }
            }
        }

        public void SetConfig(LocalPlayerData data)
        {
            _useRumble = data.m_useRumble;
            _useWorldOrientation = data.m_isWorldOrientation;
        }

        public void ChangeToNextColor()
        {
            int materialIndex = LocalPlayerManager.s_instance.ChangeToNextColorFrom(m_localPlayerID, m_materialIndex);
            SetMaterialID(materialIndex);
        }

        public void SetMaterialID(int materialID)
        {
            m_materialIndex = materialID;
            Material mat = LocalPlayerManager.s_instance.GetMaterialFromIndex(materialID);
            m_materialColor = mat.color;

            _customisationPreview?.ChangeMaterial(materialID);
            m_onColorChanged?.Invoke(m_materialColor);
        }

        #endregion


        #region Utils & Tools

        public bool UseRmble() => _useRumble;

        public bool UseWorldOrientation() => _useWorldOrientation;

        public void ForceRumbleValue(bool value)
        {
            _useRumble = value;
            m_onRumbleChanged?.Invoke(value);
        }
        public void ForceOrientationValue(bool value)
        {
            _useWorldOrientation = value;
            m_onWorldOrientationChanged?.Invoke(value);
        }

        public IAvancedInputsReader GetInputReader() => _inputReader;

        public void ValidatePlayerConfig()
        {
            m_onPlayerIDChanged?.Invoke(m_localPlayerID);
            _onConfigScreen = false;
            m_onPlayerConfigValidated?.Invoke();
            _graphics.SetActive(true);
            _UI.SetActive(true);
        }

        private void PlaySfx(SFXData data)
        {
            AudioManager.s_instance.PlaySfx(data._sfx.GetRandom(), true);
        }

        #endregion


        #region Events

        private void OnMenu_Performed() => UINavigationManager.s_instance.OnMenu_Performed(m_localPlayerID);

        private void OnCancel_Performed()
        {
            PlaySfx(_backSfxData);
            UINavigationManager.s_instance.OnCancel_Performed(m_localPlayerID);
        }
        private void OnSubmit_Performed()
        {
            if (m_isDefaultPlayer) _onSkipStartup.Raise();
            if (m_isConfigPlayer) return;

            PlaySfx(_selectSfxData);

            if (_onConfigScreen)
            {
                switch (_configScreenPosition)
                {
                    case 0:
                        //Color

                        ChangeToNextColor();
                        Debug.Log("New color : " + m_materialColor);
                        break;

                    case 1:
                        //Orientation

                        _useWorldOrientation = !_useWorldOrientation;
                        Debug.Log("Orientation world : " + _useWorldOrientation);
                        m_onWorldOrientationChanged?.Invoke(_useWorldOrientation);
                        break;

                    case 2:
                        //Rumble

                        _useRumble = !_useRumble;
                        Debug.Log("Use rumble : " + _useRumble);
                        m_onRumbleChanged?.Invoke(_useRumble);

                        break;

                }
            }

            UINavigationManager.s_instance.OnSubmit_Performed(m_localPlayerID);
        }

        private void OnSubmit_Started() => UINavigationManager.s_instance.OnSubmit_Started(m_localPlayerID);

        private void OnSubmit_Canceled() => UINavigationManager.s_instance.OnSubmit_Canceled(m_localPlayerID);

        private void OnNavigate_Performed(Vector2 value)
        {
            if (m_isConfigPlayer) return;
            if (_onConfigScreen)
            {
                int previousSelection = _configScreenPosition;
                if (value.y > 0.1f)
                {
                    _configScreenPosition--;
                }
                else if (value.y < -0.1f)
                {
                    _configScreenPosition++;
                }

                if (_configScreenPosition != previousSelection)
                {

                    if (_configScreenPosition < 0) _configScreenPosition = 2;
                    else if (_configScreenPosition > 2) _configScreenPosition = 0;

                    switch (_configScreenPosition)
                    {
                        case 0:
                            //Color

                            m_onColorSelected?.Invoke(m_materialColor, previousSelection);
                            break;

                        case 1:
                            //Orientation

                            m_onOrientationSelected?.Invoke(previousSelection);
                            break;

                        case 2:
                            //Rumble

                            m_onRumbleSelected?.Invoke(previousSelection);
                            break;

                    }
                }
            }

            PlaySfx(_switchSfxData);
            UINavigationManager.s_instance.OnNavigate_Performed(m_localPlayerID, value);
        }

        private void OnNorthButton_Performed() => UINavigationManager.s_instance.OnNorthButton_Performed(m_localPlayerID);
        private void OnWestButton_Performed() => UINavigationManager.s_instance.OnWestButton_Performed(m_localPlayerID);

        private void OnRightShoulder_Performed()
        {
            PlaySfx(_switchSfxData);
            UINavigationManager.s_instance.OnRightShoulder_Performed(m_localPlayerID);
        }

        private void OnLeftShoulder_Performed()
        {
            PlaySfx(_switchSfxData);
            UINavigationManager.s_instance.OnLeftShoulder_Performed(m_localPlayerID);
        }

        private void OnRightTrigger_Performed(float value) => UINavigationManager.s_instance.OnRightTrigger_Performed(m_localPlayerID, value);
        private void OnLeftTrigger_Performed(float value) => UINavigationManager.s_instance.OnLeftTrigger_Performed(m_localPlayerID, value);

        private void OnAdvancedSubmit_Performed(int deviceID)
        {
            if (!m_isConfigPlayer) return;
            m_onAdvancedSubmitPerformed?.Invoke(m_localPlayerID, deviceID);
        }

        private void OnAdvancedCancel_Performed(int deviceID)
        {
            if (!m_isConfigPlayer) return;
            m_onAdvancedCancelPerformed?.Invoke(m_localPlayerID, deviceID);
        }

        #endregion


        #region Private

        private bool _onConfigScreen = true;

        private int _configScreenPosition;

        private CustomisationPreview _customisationPreview;

        #endregion
    }
}