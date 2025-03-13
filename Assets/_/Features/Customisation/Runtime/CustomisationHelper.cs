using Data.Runtime;
using System.IO;
using System.Text;
using UnityEngine;

namespace Customisation.Runtime
{
    public static class CustomisationHelper
    {
        public static void UpdateMaterial(Renderer renderer, CustomisationType type, Material material,Material emissiveMat)
        {
            if (type != CustomisationType.Character && type != CustomisationType.Car) return;

            int matIndex = -1;
            int matEmissiveIndex = -1;

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (renderer.materials[i].name.Contains("PlayerColor"))
                {
                    matIndex = i;
                    break;
                }
            }

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                string matName = renderer.materials[i].name;

                if (matName.Contains("Player") && matName.Contains("_E"))
                {
                    matEmissiveIndex = i;
                    break;
                }
            }

            if (matIndex == -1)
            {
                Debug.LogError("Material not found!");
                return;
            }
            else
            {
                MaterialPropertyBlock block = new();

                renderer.GetPropertyBlock(block, matIndex);
                block.SetColor("_BaseColor", material.color);
                renderer.SetPropertyBlock(block, matIndex);
            }

            if (matEmissiveIndex != -1)
            {
                MaterialPropertyBlock block = new();

                renderer.GetPropertyBlock(block, matEmissiveIndex);

                block.SetColor("_BaseColor", emissiveMat.color);
                block.SetColor("_EmissionColor", emissiveMat.GetColor("_EmissionColor"));

                renderer.SetPropertyBlock(block, matEmissiveIndex);
            }
        }

        public static SerializedCustomisationData GetCustomisation(int index)
        {
            string path = GetFilePath();
            if (!File.Exists(path)) return null;
            else
            {
                SerializedData data = GetSavedData();
                if (data == null) return null;
                return data.m_data[index];
            }
        }

        public static void SaveAllCustomisationData(SerializedData data)
        {
            string path = GetFilePath();
            string json = JsonUtility.ToJson(data);

            WriteFile(path, json);
        }

        private static string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, FILE_NAME);
        }

        private static void WriteFile(string path, string json)
        {
            try
            {
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error while writing customisation file : {ex}");
                return;
            }

            Debug.Log("Customisation file was successfully updated");
        }

        public static SerializedData GetSavedData()
        {
            if (m_data != null) return m_data;

            string path = GetFilePath();
            string json = File.ReadAllText(path, Encoding.UTF8);

            try
            {
                m_data = JsonUtility.FromJson<SerializedData>(json);
            }
            catch (System.Exception)
            {
                Debug.LogError("Customisation file not correct, file will be recreated with default configuration");
                File.Delete(path);
                return null;
            }

            return m_data;
        }

        public static void UpdateLayerFor(GameObject go, string layerName)
        {
            go.layer = LayerMask.NameToLayer(layerName);

            foreach (var item in go.GetComponentsInChildren<Transform>())
            {
                item.gameObject.layer = LayerMask.NameToLayer(layerName);
            }
        }

        private const string FILE_NAME = "Customisation.Json";
        private static SerializedData m_data;
    }
}