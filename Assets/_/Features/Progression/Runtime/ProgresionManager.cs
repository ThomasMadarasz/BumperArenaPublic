using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Progression.Runtime
{
    public class ProgresionManager : MonoBehaviour
    {
        #region Exposed

        public static ProgresionManager s_instance;

        private const string PROGRESSION_FILE_NAME = "Progression.pgs";

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            SetSettingsFilePath();

            if (FileExist()) LoadSettingsFromFile();
            else
            {
                CreateDefaultProgressionFile();
                SaveProgressionFile();
            }
        }

        public void GainCoin(int amount)
        {
            _progressionData.m_currentCoin += amount;
            SaveProgressionFile();
        }

        #endregion


        #region Utils & Tools

        private bool FileExist() => File.Exists(_progressionFilePath);

        private void SetSettingsFilePath() => _progressionFilePath = Path.Combine(Application.persistentDataPath, PROGRESSION_FILE_NAME);

        private void LoadSettingsFromFile()
        {
            try
            {
                string json = File.ReadAllText(_progressionFilePath, Encoding.UTF8);
                _progressionData = JsonUtility.FromJson<ProgressionData>(json);
                Debug.Log("Progression file was successfully loaded");
            }
            catch (Exception)
            {
                Debug.LogError("Error while loading progression file, default file created");
                CreateDefaultProgressionFile();
                return;
            }
        }

        private void SaveProgressionFile()
        {
            string json = JsonUtility.ToJson(_progressionData);

            try
            {
                File.WriteAllText(_progressionFilePath, json, Encoding.UTF8);
                Debug.Log("Progression file was successfully saved");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }
        }

        private void CreateDefaultProgressionFile()
        {
            _progressionData = new()
            {
                m_currentCoin = 0
            };
        }

        #endregion


        #region Private

        private ProgressionData _progressionData;
        private string _progressionFilePath;

        #endregion
    }
}