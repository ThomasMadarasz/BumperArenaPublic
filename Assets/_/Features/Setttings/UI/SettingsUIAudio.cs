using Settings.Runtime;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UIFeedbacks.Runtime;

namespace Settings.UI
{
    public class SettingsUIAudio : MenuBase
    {
        #region Exposed

        private const string MASTER_VOLUME_PARAM = "MasterVolume";
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SfxVolume";

        [SerializeField] private AudioMixerGroup _masterMixer;
        [SerializeField] private AudioMixerGroup _musicMixer;
        [SerializeField] private AudioMixerGroup _sfxMixer;

        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [SerializeField] private SettingsUIManager _manager;

        [SerializeField] private GameObject _firstSelectedGO;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshUI();

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstSelectedGO);
        }

        #endregion


        #region Main

        private void RefreshUI()
        {
            if (_masterMixer.audioMixer.GetFloat(MASTER_VOLUME_PARAM, out float masterVolume))
                _masterVolumeSlider.SetValueWithoutNotify(Mathf.Pow(10, SettingsManager.s_instance.m_settingsData.m_masterVolume / 20));

            if (_musicMixer.audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out float musicVolume))
                _musicVolumeSlider.SetValueWithoutNotify(Mathf.Pow(10, SettingsManager.s_instance.m_settingsData.m_musicVolume / 20));

            if (_sfxMixer.audioMixer.GetFloat(SFX_VOLUME_PARAM, out float sfxVolume))
                _sfxVolumeSlider.SetValueWithoutNotify(Mathf.Pow(10, SettingsManager.s_instance.m_settingsData.m_sfxVolume / 20));

            _masterVolumeSlider.GetComponent<SelectedFeedbacks>().PlayFeedbacks();
            _musicVolumeSlider.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
            _sfxVolumeSlider.GetComponent<SelectedFeedbacks>().PublicResetFeedbacks();
        }

        private void SaveVolumes()
        {
            SettingsManager.s_instance.SaveSettings();
        }

        public void BackToSelectionMenu()
        {
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion


        #region Event

        public void OnMasterVolumeValueChanged(float value)
        {
            float logarithmicValue = Mathf.Log10(value) * 20;
            _masterMixer.audioMixer.SetFloat(MASTER_VOLUME_PARAM, logarithmicValue);
            SettingsManager.s_instance.m_settingsData.m_masterVolume = logarithmicValue;
            SaveVolumes();
        }

        public void OnMusicVolumeValueChanged(float value)
        {
            float logarithmicValue = Mathf.Log10(value) * 20;
            _musicMixer.audioMixer.SetFloat(MUSIC_VOLUME_PARAM, logarithmicValue);
            SettingsManager.s_instance.m_settingsData.m_musicVolume = logarithmicValue;
            SaveVolumes();
        }

        public void OnSfxVolumeValueChanged(float value)
        {
            float logarithmicValue = Mathf.Log10(value) * 20;
            Debug.Log(logarithmicValue);
            _sfxMixer.audioMixer.SetFloat(SFX_VOLUME_PARAM, logarithmicValue);
            SettingsManager.s_instance.m_settingsData.m_sfxVolume = logarithmicValue;
            SaveVolumes();
        }

        #endregion


        #region UI Navigation

        public override void OnCancel_Performed(int playerId)
        {
            _manager.BackToSelectionMenu(gameObject);
        }

        #endregion

        private float _minVolume = 0.0001f;
    }
}