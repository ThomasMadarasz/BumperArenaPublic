using Archi.Runtime;
using System;
using System.IO;
using UnityEngine;
using System.Text;
using UnityEngine.Localization.Settings;
using ScriptableEvent.Runtime;
using System.Reflection;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;
using UnityEngine.Localization;
using UnityEngine.Audio;

namespace Settings.Runtime
{
    public class SettingsManager : CBehaviour
    {
        #region Exposed

        public static SettingsManager s_instance;

        private const string MASTER_VOLUME_PARAM = "MasterVolume";
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SfxVolume";
        private const string SETTINGS_FILE_NAME = "Settings.config";

        public SettingsData m_settingsData
        {
            get { return _settingsData; }
            set { _settingsData = value; }
        }

        [SerializeField] private GameEvent _onSelectedLanguageChanged;
        [SerializeField] private GameEventT _onDeviceControlTypeUIChanged;

        [SerializeField][BoxGroup("Post Process")] private VolumeProfile _volumeProf;

        [SerializeField][BoxGroup("Audio")] private AudioMixerGroup _masterGrp;
        [SerializeField][BoxGroup("Audio")] private AudioMixerGroup _musicGrp;
        [SerializeField][BoxGroup("Audio")] private AudioMixerGroup _sfxGrp;
        [SerializeField][BoxGroup("Audio")] private GameEvent _onStartMainMenuMusic;

        [SerializeField] private int _currentSettingsFileVersion;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Start()
        {
            float mixerValue = Mathf.Pow(10, _settingsData.m_masterVolume / 20);
            mixerValue = Mathf.Log10(mixerValue) * 20;
            _masterGrp.audioMixer.SetFloat(MASTER_VOLUME_PARAM, mixerValue);

            mixerValue = Mathf.Pow(10, _settingsData.m_musicVolume / 20);
            mixerValue = Mathf.Log10(mixerValue) * 20;
            _musicGrp.audioMixer.SetFloat(MUSIC_VOLUME_PARAM, mixerValue);

            mixerValue = Mathf.Pow(10, _settingsData.m_sfxVolume / 20);
            mixerValue = Mathf.Log10(mixerValue) * 20;
            _sfxGrp.audioMixer.SetFloat(SFX_VOLUME_PARAM, mixerValue);

            _onStartMainMenuMusic.Raise();
        }

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance == null) s_instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            SetSettingsFilePath();

            if (File.Exists(_settingsFilePath)) LoadSettingsFromFile();
            else
            {
                CreateDefaultSettingsFile();
                SaveSettings();
            }

            SetSettingsOnStartup();
        }

        private void SetSettingsOnStartup()
        {
            LocalizationSettings.InitializeSynchronously = true;
            LocalizationSettings.InitializationOperation.WaitForCompletion();

            var localEN = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));
            var localFR = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("fr"));

            if (_settingsData.m_languageKey == localEN.Identifier.Code) LocalizationSettings.SelectedLocale = localEN;
            else LocalizationSettings.SelectedLocale = localFR;
            _onSelectedLanguageChanged?.Raise();

            _onDeviceControlTypeUIChanged.Raise((int)_settingsData.m_controlDeviceTypeUI);

            Application.targetFrameRate = _settingsData.m_targetFrameRate;

            if (_volumeProf.TryGet<ColorAdjustments>(out ColorAdjustments ca))
            {
                _colarAdj = ca;
                _colarAdj.postExposure.Override(m_settingsData.m_postExposure);
                _colarAdj.saturation.Override(m_settingsData.m_saturation);
                _colarAdj.contrast.Override(m_settingsData.m_contrast);
            }
        }

        public void SaveSettings()
        {
            string json = GetJsonFromObject();

            try
            {
                File.WriteAllText(_settingsFilePath, json, Encoding.UTF8);
                Debug.Log("Settings file was successfully saved");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }
        }

        public void ChangeLanguage(string key)
        {
            int index = -1;

            foreach (var item in LocalizationSettings.AvailableLocales.Locales)
            {
                string languageCode = item.name.Split('(')[1];
                languageCode = languageCode.Replace(")", string.Empty);

                if (languageCode != key) continue;
                index = LocalizationSettings.AvailableLocales.Locales.IndexOf(item);
            }

            if (index == -1)
            {
                Debug.LogError("Language Key not found");
                return;
            }

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            _onSelectedLanguageChanged?.Raise();

            m_settingsData.m_languageKey = key;
            SaveSettings();
        }

        public void ChangeResolution(Resolution res)
        {
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        }

        public void ChangeFullScreenMode(FullScreenMode mode)
        {
            Screen.fullScreenMode = mode;

            m_settingsData.m_screenMode = (int)mode;
            SaveSettings();
        }

        public void ChangeControlDeviceTypeUI(UIControlDeviceType value)
        {
            m_settingsData.m_controlDeviceTypeUI = (int)value;
            SaveSettings();

            _onDeviceControlTypeUIChanged?.Raise((int)value);
        }

        public void ChangeScreenShake(bool value)
        {
            m_settingsData.m_screenshake = value;
            SaveSettings();
        }

        public void ChangeRumble(bool value)
        {
            m_settingsData.m_rumble = value;
            SaveSettings();
        }

        public void ChangeControls(ControlType value)
        {
            m_settingsData.m_controlType = (int)value;
            SaveSettings();
        }

        public void ChangeTargetFrameRate(int value)
        {
            Application.targetFrameRate = value;

            m_settingsData.m_targetFrameRate = value;
            SaveSettings();
        }

        public void SaveCurrentResolution(Resolution res)
        {
            m_settingsData.m_height = res.height;
            m_settingsData.m_width = res.width;
            m_settingsData.m_refreshRate = res.refreshRate;

            SaveSettings();
        }

        public void ChangeSaturation(int value)
        {
            m_settingsData.m_saturation = value;
            SaveSettings();
            if (_colarAdj != null) _colarAdj.saturation.Override(value);
        }

        public void ChangePostExposure(float value)
        {
            m_settingsData.m_postExposure = value;
            SaveSettings();
            if (_colarAdj != null) _colarAdj.postExposure.Override(value);
        }

        public void ChangeContrast(int value)
        {
            m_settingsData.m_contrast = value;
            SaveSettings();
            if (_colarAdj != null) _colarAdj.contrast.Override(value);
        }

        #endregion


        #region Utils & Tools

        public int GetCurrentDeviceTypeUI() => m_settingsData.m_controlDeviceTypeUI;

        private string GetJsonFromObject() => JsonUtility.ToJson(_settingsData);

        private void SetSettingsFilePath() => _settingsFilePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);

        private void LoadSettingsFromFile()
        {
            try
            {
                string json = File.ReadAllText(_settingsFilePath, Encoding.UTF8);

                SettingsData savedSettings = JsonUtility.FromJson<SettingsData>(json);
                if (savedSettings.m_fileVersion < _currentSettingsFileVersion) CopyValueFromOldFile(savedSettings);
                else _settingsData = savedSettings;

                Debug.Log("Settings file was successfully loaded");
            }
            catch (Exception)
            {
                Debug.LogError("Error while loading settings file, default config loaded");
                CreateDefaultSettingsFile();
                return;
            }
        }

        private void CopyValueFromOldFile(SettingsData savedSettings)
        {
            try
            {
                CreateDefaultSettingsFile();

                foreach (PropertyInfo prop in _settingsData.GetType().GetProperties())
                {
                    PropertyInfo property = savedSettings.GetType().GetProperty(prop.Name, prop.PropertyType);
                    if (property == null) continue;

                    prop.SetValue(_settingsData, property.GetValue(savedSettings));
                }

                SaveSettings();
                Debug.Log("Old value from settings file was correctly copied");
            }
            catch (Exception)
            {
                Debug.LogError("Error while copying old values in new file");
                CreateDefaultSettingsFile();
                return;
            }
        }

        private void CreateDefaultSettingsFile()
        {
            _settingsData = new()
            {
                m_height = Screen.currentResolution.height,
                m_width = Screen.currentResolution.width,
                m_refreshRate = Screen.currentResolution.refreshRate,
                m_targetFrameRate = 60,
                m_screenMode = (int)FullScreenMode.FullScreenWindow,
                m_controlDeviceTypeUI = (int)UIControlDeviceType.Keyboard,
                m_screenshake = true,
                m_masterVolume = 0f,
                m_musicVolume = 0f,
                m_sfxVolume = 0f,
                m_languageKey = "en",
                m_rumble = true,
                m_controlType = (int)ControlType.World,
                m_fileVersion = _currentSettingsFileVersion,
                m_contrast = 0,
                m_postExposure = 0,
                m_saturation = 0
            };
        }

        #endregion


        #region Private

        private SettingsData _settingsData;
        private string _settingsFilePath;
        private ColorAdjustments _colarAdj;

        #endregion
    }
}