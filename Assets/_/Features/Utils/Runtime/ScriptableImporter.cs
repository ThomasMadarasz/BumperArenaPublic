using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utils.Runtime
{
    public static class ScriptableImporter
    {
        public static bool CheckPath()
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, IMPORTATION_FOLDER_NAME);
            if (!Directory.Exists(directoryPath)) return false;

            string filePath = Path.Combine(directoryPath, IMPORTATION_FILE_NAME);
            if (!File.Exists(filePath)) return false;

            return true;
        }

        public static void UpdateDictionaryWithFileValues(FileData data, Dictionary<ScriptableObject, List<ScriptableInfo>> dico)
        {
            foreach (var sheet in data.m_sheets)
            {
                ScriptableObject key = dico.Keys.FirstOrDefault(x => x.name == sheet.m_name);
                if (key == null)
                {
                    continue;
                }

                foreach (var row in sheet.m_rows)
                {
                    ScriptableInfo info = dico[key].FirstOrDefault(x => x.m_fieldName == row.m_propertyName);
                    if (info == null)
                    {
                        continue;
                    }

                    int index = dico[key].IndexOf(info);
                    dico[key][index].m_fieldValue = row.m_propertyValue;
                }

            }
        }

        public static Dictionary<ScriptableObject, List<ScriptableInfo>> FillDictionaryWithScriptableValues(ScriptableObject scr)
        {
            Dictionary<ScriptableObject, List<ScriptableInfo>> dico = new();

            List<ScriptableInfo> infos = GetInfoForScriptable(scr);
            dico.Add(scr, infos);

            return dico;
        }

        private static List<ScriptableInfo> GetInfoForScriptable(ScriptableObject scr)
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

        public static FileData GetDataFromFile()
        {
            string path = Path.Combine(Application.persistentDataPath, IMPORTATION_FOLDER_NAME, IMPORTATION_FILE_NAME);

            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            object jsonData = JsonUtility.FromJson(json, typeof(FileData));

            return jsonData as FileData;
        }

        public static object ParseValue(FieldInfo info, string str)
        {
            string strValue = str;
            strValue = string.Concat(strValue.Where(c => !Char.IsWhiteSpace(c)));

            if (info.FieldType == typeof(float))
            {
                float result = 0;
                result = float.Parse(strValue);
                return result;
            }
            else if (info.FieldType == typeof(int))
            {
                int result = int.Parse(strValue);

                return result;
            }
            else if (info.FieldType.IsEnum)
            {
                int result = int.Parse(strValue);

                return result;
            }
            else if (info.FieldType == typeof(Vector3))
            {
                string[] values = strValue.Split(';');

                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);

                return new Vector3(x, y, z);
            }
            else if (info.FieldType.Name == "PlayerSpeedData")
            {
                strValue = strValue.Replace("\n", string.Empty);
                string[] values = strValue.Split(';');
                values = values.Where(x => x.Length > 0).ToArray();

                var instance = Activator.CreateInstance(info.FieldType);
                FieldInfo[] fields = instance.GetType().GetFields();

                foreach (var item in values)
                {
                    string[] propertyValue = item.Split('=');

                    string propName = propertyValue[0];
                    string propValue = propertyValue[1];

                    FieldInfo field = fields.FirstOrDefault(x => x.Name == propName);
                    var val = ParseValue(field, propValue);
                    field.SetValue(instance, val);
                }
                return instance;
            }

            return strValue;
        }

        private static string IMPORTATION_FOLDER_NAME = "Import";
        private static string IMPORTATION_FILE_NAME = "Data.json";
    }

    [System.Serializable]
    public class ScriptableInfo
    {
        public string m_fieldName;
        public object m_fieldValue;
        public string m_fieldType;
    }
}