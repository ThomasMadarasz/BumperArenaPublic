using PlayerController.Data.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Internal.Importer
{
    public class ScriptableImporter : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private ScriptableObject[] _objectToUpdates;

        #endregion


        #region Unity API

        public void ImportFileData()
        {
            if (!CheckPath()) return;
            FileData data = GetDataFromFile();

            FillDictionaryWithScriptableValues();
            UpdateDictionaryWithFileValues(data);
            UpdateScriptableValuesWithDictionary();
        }

        #endregion


        #region Main
        #endregion


        #region Utils & Tools

        private bool CheckPath()
        {
            _directoryPath = Path.Combine(Application.persistentDataPath, IMPORTATION_FOLDER_NAME);
            if (!Directory.Exists(_directoryPath))
            {
                Debug.LogError("Directory not exist");
                return false;
            }

            _filePath = Path.Combine(_directoryPath, IMPORTATION_FILE_NAME);
            if (!File.Exists(_filePath))
            {
                Debug.LogError("File not exist");
                return false;
            }

            return true;
        }

        private FileData GetDataFromFile()
        {
            string json = File.ReadAllText(_filePath, System.Text.Encoding.UTF8);
            object jsonData = JsonUtility.FromJson(json, typeof(FileData));

            return jsonData as FileData;
        }

        private List<ScriptableInfo> GetInfoForScriptable(ScriptableObject scr)
        {
            List<ScriptableInfo> infos = new();

            Type type = scr.GetType();
            FieldInfo[] fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(scr);

                ScriptableInfo info = new()
                { m_fieldName = field.Name, m_fieldValue = value, m_fieldType = field.FieldType.ToString() };

                infos.Add(info);
            }

            return infos;
        }

        private void UpdateDictionaryWithFileValues(FileData data)
        {
            foreach (var sheet in data.m_sheets)
            {
                ScriptableObject key = _dico.Keys.FirstOrDefault(x => x.name == sheet.m_name);
                if (key == null)
                {
                    Debug.LogError($"Key {sheet.m_name} not found!");
                    continue;
                }

                foreach (var row in sheet.m_rows)
                {
                    ScriptableInfo info = _dico[key].FirstOrDefault(x => x.m_fieldName == row.m_propertyName);
                    if (info == null)
                    {
                        Debug.LogError($"Field {row.m_propertyName} not found!");
                        continue;
                    }

                    int index = _dico[key].IndexOf(info);
                    _dico[key][index].m_fieldValue = row.m_propertyValue;
                }

            }
        }

        private void FillDictionaryWithScriptableValues()
        {
            _dico = new();
            foreach (var item in _objectToUpdates)
            {
                List<ScriptableInfo> infos = GetInfoForScriptable(item);
                _dico.Add(item, infos);
            }
        }

        private void UpdateScriptableValuesWithDictionary()
        {
            foreach (var scr in _objectToUpdates)
            {
                FieldInfo[] infos = scr.GetType().GetFields();

                foreach (var info in infos)
                {
                    ScriptableInfo scrInfo = _dico[scr].FirstOrDefault(x => x.m_fieldName == info.Name);

                    if (scrInfo == null)
                    {
                        Debug.LogError($"Field {info.Name} not found in dictionary!");
                        continue;
                    }

                    SerializedObject so = new SerializedObject(scr);
                    SerializedProperty prop = so.FindProperty(info.Name);

                    string strValue = scrInfo.m_fieldValue.ToString();
                    strValue = string.Concat(strValue.Where(c => !Char.IsWhiteSpace(c)));

                    if (info.FieldType == typeof(float))
                    {
                        float result = (float)ParseValue(info, strValue);
                        prop.floatValue = result;
                    }
                    else if (info.FieldType == typeof(int))
                    {
                        int result = (int)ParseValue(info, strValue);
                        prop.intValue = result;
                    }
                    else if (info.FieldType.IsEnum)
                    {
                        int result = (int)ParseValue(info, strValue);
                        prop.enumValueFlag = result;
                    }
                    else if (info.FieldType == typeof(Vector3))
                    {
                        Vector3 result = (Vector3)ParseValue(info, strValue);
                        prop.vector3Value = result;
                    }
                    else if (info.FieldType == typeof(PlayerSpeedData))
                    {
                        string str = strValue.Replace("\n", string.Empty);
                        string[] values = str.Split(';');
                        values = values.Where(x => x.Length > 0).ToArray();

                        PlayerSpeedData psd = new();
                        FieldInfo[] fields = psd.GetType().GetFields();

                        foreach (var item in values)
                        {
                            string[] propertyValue = item.Split('=');

                            string propName = propertyValue[0];
                            string propValue = propertyValue[1];

                            FieldInfo field = fields.FirstOrDefault(x => x.Name == propName);
                            var val = ParseValue(field, propValue);
                            field.SetValue(psd, val);

                        }
                    }
                    so.ApplyModifiedProperties();
                }
            }
        }

        private object ParseValue(FieldInfo info, string str)
        {
            if (info.FieldType == typeof(float))
            {
                float result = 0;
                result = float.Parse(str);
                return result;
            }
            else if (info.FieldType == typeof(int))
            {
                int result = int.Parse(str);

                return result;
            }
            else if (info.FieldType.IsEnum)
            {
                int result = int.Parse(str);

                return result;
            }
            else if (info.FieldType == typeof(Vector3))
            {
                string[] values = str.Split(';');

                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);

                return new Vector3(x, y, z);
            }

            return str;
        }


        #endregion


        #region Debug
        #endregion


        #region Private

        private const string IMPORTATION_FOLDER_NAME = "Import";
        private const string IMPORTATION_FILE_NAME = "Data.json";

        private string _directoryPath;
        private string _filePath;

        private Dictionary<ScriptableObject, List<ScriptableInfo>> _dico;

        #endregion
    }

    [System.Serializable]
    public class ScriptableInfo
    {
        public string m_fieldName;
        public object m_fieldValue;
        public string m_fieldType;
    }
}