using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Utils.UI;
using UnityEngine.UI;
using Settings.Runtime;
using Sirenix.OdinInspector;
using UINavigation.Runtime;
using System;
using Utils.Translation;
using ScriptableEvent.Runtime;
using UnityEngine.EventSystems;
using UIFeedbacks.Runtime;
using System.Collections;
using SceneManager.runtime;

namespace Settings.UI
{
    public class SettingsUIGraphics : MenuBase
    {
        #region Exposed

        [BoxGroup("Components")][SerializeField] private TMP_Dropdown _resolutionDropDown;
        [BoxGroup("Components")][SerializeField] private TMP_Dropdown _frameRateDropDown;
        [BoxGroup("Components")][SerializeField] private TMP_Dropdown _screenModeDropDown;
        [BoxGroup("Components")][SerializeField] private TMP_Dropdown _deviceControlTypeUI;
        [BoxGroup("Components")][SerializeField] private Slider _saturationSlider;
        [BoxGroup("Components")][SerializeField] private Slider _contrastSlider;
        [BoxGroup("Components")][SerializeField] private Slider _postExposureSlider;

        [SerializeField] private ModalWindow _modalPrefab;
        [SerializeField] private GameObject[] _objToDisableWhenModalWindow;

        [SerializeField] private FrameRateData[] _framerateData;

        [BoxGroup("Screenshake")][SerializeField] private Toggle _screenshakeOnToggle;
        [BoxGroup("Screenshake")][SerializeField] private Toggle _screenshakeOffToggle;

        [BoxGroup("Screenshake")][SerializeField] private ToggleGroup _screenshakeToggleGroup;

        [SerializeField] private SettingsUIManager _manager;
        [SerializeField] private GameEvent _onSelectedLanguageChanged;
        [SerializeField] private GameObject _firstSelectedGO;

        [BoxGroup("Post Process")][SerializeField] private float _minPostExposure;
        [BoxGroup("Post Process")][SerializeField] private float _maxPostExposure;
        [BoxGroup("Post Process")][SerializeField] private int _minSaturation;
        [BoxGroup("Post Process")][SerializeField] private int _maxSaturation;
        [BoxGroup("Post Process")][SerializeField] private int _minContrast;
        [BoxGroup("Post Process")][SerializeField] private int _maxContrast;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        protected override void OnEnable()
        {
            EventSystem.current.sendNavigationEvents = false;

            base.OnEnable();

            _resolutionDropDown.interactable = !SceneLoader.s_instance.PauseMenuIsLoaded();

            RefreshUI();

            StartCoroutine(nameof(OnEnableRoutine));
        }

        #endregion


        #region Main

        private IEnumerator OnEnableRoutine()
        {
            EventSystem.current.SetSelectedGameObject(null);

            float elapsedTime = 0;

            while (elapsedTime < 0.1f)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            EventSystem.current.sendNavigationEvents = true;
            elapsedTime = 0;
            while (elapsedTime < 0.01f)
            {
                elapsedTime += Time.unscaledDeltaTime;
            }

            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);
        }

        private void Setup()
        {
            _saturationSlider.minValue = _minSaturation;
            _saturationSlider.maxValue = _maxSaturation;

            _contrastSlider.minValue = _minContrast;
            _contrastSlider.maxValue = _maxContrast;

            _postExposureSlider.minValue = _minPostExposure;
            _postExposureSlider.maxValue = _maxPostExposure;

            _onSelectedLanguageChanged?.RegisterListener(UpdateTranslations);

            _resolutionDropDown.ClearOptions();
            _frameRateDropDown.ClearOptions();
            _screenModeDropDown.ClearOptions();
            _deviceControlTypeUI.ClearOptions();

            FillResolutions();
            FillFrameRates();
            FillScreenMode();
            FillDeviceControlTypeUI();

            UpdateTranslations();

            //_frameRateDropDown.AddOptions(_availableFrameRateText);
            _resolutionDropDown.AddOptions(_availableResolutionsText);
            //_screenModeDropDown.AddOptions(_availableScreenModeText);
            //_deviceControlTypeUI.AddOptions(_availableDeviceControlTypeUIText);

            _currentResolution = _availableResolutions.First();
        }

        private void UpdateTranslations()
        {
            _deviceControlTypeUI.ClearOptions();
            _screenModeDropDown.ClearOptions();
            _frameRateDropDown.ClearOptions();

            _availableDeviceControlTypeUIText.Clear();
            _availableDeviceControlTypeUIText.Add(TranslationManager.Translate("Keyboard"));
            _availableDeviceControlTypeUIText.Add(TranslationManager.Translate("Playstation"));
            _availableDeviceControlTypeUIText.Add(TranslationManager.Translate("Xbox"));

            _availableScreenModeText.Clear();
            _availableScreenModeText.Add(TranslationManager.Translate("Windowed"));
            _availableScreenModeText.Add(TranslationManager.Translate("Fullscreen"));
            _availableScreenModeText.Add(TranslationManager.Translate("Borderless windowed"));

            _availableFrameRateText.Clear();
            foreach (var data in _framerateData)
            {
                if (data.m_frameRateValue == -1) _availableFrameRateText.Add(TranslationManager.Translate(data.m_displayText));
                else _availableFrameRateText.Add(data.m_displayText);
            }

            _frameRateDropDown.AddOptions(_availableFrameRateText);
            _deviceControlTypeUI.AddOptions(_availableDeviceControlTypeUIText);
            _screenModeDropDown.AddOptions(_availableScreenModeText);
        }

        private void FillResolutions()
        {
            foreach (var resolution in Screen.resolutions) _availableResolutions.Add(resolution);
            _availableResolutions.Reverse();
            foreach (var resolution in _availableResolutions) _availableResolutionsText.Add($"{resolution.width} x {resolution.height} - {resolution.refreshRate} Hz");
        }

        private void FillDeviceControlTypeUI()
        {
            _availableDeviceControlTypeUI.Add((int)UIControlDeviceType.Keyboard);
            _availableDeviceControlTypeUI.Add((int)UIControlDeviceType.Playstation);
            _availableDeviceControlTypeUI.Add((int)UIControlDeviceType.Xbox);
        }

        private void FillScreenMode()
        {
            _availableScreenMode.Add(FullScreenMode.Windowed);
            _availableScreenMode.Add(FullScreenMode.FullScreenWindow);
            _availableScreenMode.Add(FullScreenMode.ExclusiveFullScreen);
        }

        private void FillFrameRates()
        {
            foreach (var data in _framerateData)
            {
                _availableFrameRate.Add(data.m_frameRateValue);
            }
        }

        private void RefreshUI()
        {
            SettingsData settings = SettingsManager.s_instance.m_settingsData;

            RefreshResolution(settings);
            RefreshFrameRate(settings);
            RefreshScreenMode(settings);
            RefreshScreenShake(settings);
            RefreshDeviceControlTypeUI(settings);
            RefreshContrast(settings);
            RefreshPostExposure(settings);
            RefreshSaturation(settings);

            _screenShakeValue = settings.m_screenshake;
        }

        private void ApplyResolution()
        {
            Debug.Log("Save new resolution");
            _currentResolution = _resolutionToApply;
            SettingsManager.s_instance.SaveCurrentResolution(_currentResolution);
            foreach (GameObject obj in _objToDisableWhenModalWindow) obj.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_resolutionDropDown.gameObject);

            _onConfirmationScreenDisplayed = false;
        }

        private void ReturnToLastResolution()
        {
            Debug.Log("Back to last resolution");
            SettingsManager.s_instance.ChangeResolution(_currentResolution);

            _resolutionDropDown.SetValueWithoutNotify(_availableResolutions.IndexOf(_currentResolution));
            foreach (GameObject obj in _objToDisableWhenModalWindow) obj.SetActive(true);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_resolutionDropDown.gameObject);

            _onConfirmationScreenDisplayed = false;
        }

        private void SaveScreenshake(bool value) => SettingsManager.s_instance.ChangeScreenShake(value);

        #endregion


        #region Event

        public void OnResolutionValueChanged(int value)
        {
            _onConfirmationScreenDisplayed = true;

            _resolutionToApply = _availableResolutions[value];
            SettingsManager.s_instance.ChangeResolution(_resolutionToApply);

            EventSystem.current.SetSelectedGameObject(null);

            ModalWindow modal = Instantiate(_modalPrefab);
            modal.ExternalSetup("Save this resolution?", 15, ApplyResolution, ReturnToLastResolution);
            foreach (GameObject obj in _objToDisableWhenModalWindow) obj.SetActive(false);
        }

        public void OnTargetFrameRateChanged(int value)
        {
            SettingsManager.s_instance.ChangeTargetFrameRate(_availableFrameRate[value]);
        }

        public void OnScreenModeValueChanged(int value)
        {
            FullScreenMode mode = _availableScreenMode[value];
            SettingsManager.s_instance.ChangeFullScreenMode(mode);
        }

        public void OnDeviceControlTypeUIChanged(int value)
        {
            UIControlDeviceType enumValue = (UIControlDeviceType)value;
            SettingsManager.s_instance.ChangeControlDeviceTypeUI(enumValue);
        }

        public void OnScreenShakeValueChanged(bool value)
        {
            if (value == _screenShakeValue)
            {
                _screenshakeOnToggle.SetIsOnWithoutNotify(false);
                _screenshakeOnToggle.GetComponent<CustomToggle>().PlayFeedbackValue(false);
                _screenshakeOffToggle.SetIsOnWithoutNotify(true);
                _screenshakeOffToggle.GetComponent<CustomToggle>().PlayFeedbackValue(true);
            }
            else
            {
                _screenshakeOffToggle.SetIsOnWithoutNotify(false);
                _screenshakeOffToggle.GetComponent<CustomToggle>().PlayFeedbackValue(false);
                _screenshakeOnToggle.SetIsOnWithoutNotify(true);
                _screenshakeOnToggle.GetComponent<CustomToggle>().PlayFeedbackValue(true);
            }

            SaveScreenshake(value);
            _screenShakeValue = value;
        }

        public void OnSaturationValueChanged(float value)
        {
            SettingsManager.s_instance.ChangeSaturation((int)value);
        }

        public void OnContrastValueChanged(float value)
        {
            SettingsManager.s_instance.ChangeContrast((int)value);
        }

        public void OnPostExposureValueChanged(float value)
        {
            SettingsManager.s_instance.ChangePostExposure(value);
        }

        #endregion


        #region Utils & Tools

        private void RefreshResolution(SettingsData settings)
        {
            Resolution currentRes = _availableResolutions.FirstOrDefault(x => x.height == settings.m_height && x.width == settings.m_width && x.refreshRate == settings.m_refreshRate);
            _resolutionDropDown.SetValueWithoutNotify(_availableResolutions.IndexOf(currentRes));
            _resolutionDropDown.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshFrameRate(SettingsData settings)
        {
            int frameRate = _availableFrameRate.FirstOrDefault(x => x == settings.m_targetFrameRate);
            _frameRateDropDown.SetValueWithoutNotify(_availableFrameRate.IndexOf(frameRate));
            _frameRateDropDown.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshScreenMode(SettingsData settings)
        {
            FullScreenMode mode = (FullScreenMode)settings.m_screenMode;

            int index = _availableScreenMode.IndexOf(mode);
            _screenModeDropDown.SetValueWithoutNotify(index);
            _screenModeDropDown.GetComponent<SelectedFeedbacks>().PlayFeedbacks();
        }

        private void RefreshDeviceControlTypeUI(SettingsData settings)
        {
            int index = _availableDeviceControlTypeUI.IndexOf(settings.m_controlDeviceTypeUI);
            _deviceControlTypeUI.SetValueWithoutNotify(index);
            _deviceControlTypeUI.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshScreenShake(SettingsData settings)
        {
            _screenshakeToggleGroup.SetAllTogglesOff(false);

            if (settings.m_screenshake) _screenshakeOnToggle.SetIsOnWithoutNotify(true);
            else _screenshakeOffToggle.SetIsOnWithoutNotify(true);
            _screenshakeOnToggle.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshContrast(SettingsData settings)
        {
            _contrastSlider.SetValueWithoutNotify(settings.m_contrast);
            _contrastSlider.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshPostExposure(SettingsData settings)
        {
            _postExposureSlider.SetValueWithoutNotify(settings.m_postExposure);
            _postExposureSlider.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void RefreshSaturation(SettingsData settings)
        {
            _saturationSlider.SetValueWithoutNotify(settings.m_saturation);
            _saturationSlider.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        public void BackToSelectionMenu()
        {
            if (_onConfirmationScreenDisplayed) return;
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion


        #region UI Navigation

        public override void OnCancel_Performed(int playerId)
        {
            if (_onConfirmationScreenDisplayed) return;
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion


        #region Private

        private Resolution _currentResolution;
        private Resolution _resolutionToApply;

        private List<Resolution> _availableResolutions = new();
        private List<string> _availableResolutionsText = new();

        private List<int> _availableFrameRate = new();
        private List<string> _availableFrameRateText = new();

        private List<int> _availableDeviceControlTypeUI = new();
        private List<string> _availableDeviceControlTypeUIText = new();

        private List<FullScreenMode> _availableScreenMode = new();
        private List<string> _availableScreenModeText = new();

        private bool _screenShakeValue;
        private bool _onConfirmationScreenDisplayed;

        #endregion
    }

    [Serializable]
    public class FrameRateData
    {
        public string m_displayText;
        public int m_frameRateValue;
    }
}