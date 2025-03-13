namespace Settings.Runtime
{
    [System.Serializable]
    public class SettingsData
    {
        #region General

        public int m_fileVersion;

        #endregion


        #region Audio

        public float m_masterVolume;
        public float m_musicVolume;
        public float m_sfxVolume;

        #endregion


        #region Language

        public string m_languageKey;

        #endregion


        #region Graphics

        public int m_width;
        public int m_height;
        public int m_refreshRate;
        public int m_targetFrameRate;
        public int m_screenMode;
        public bool m_screenshake;
        public float m_postExposure;
        public int m_contrast;
        public int m_saturation;

        #endregion


        #region UI

        public int m_controlDeviceTypeUI;

        #endregion


        #region Controls

        public int m_controlType;
        public bool m_rumble;

        #endregion
    }
}