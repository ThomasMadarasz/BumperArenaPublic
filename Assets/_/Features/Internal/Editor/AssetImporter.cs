using UnityEditor;
using UnityEngine;

namespace Internal.Editor
{
    public class AssetImporter : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject g)
        {
            if (ObjectAlreadyExist(g.name)) return;

            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.materialLocation = ModelImporterMaterialLocation.External;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        }

        private Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            string[] guids = AssetDatabase.FindAssets($"{material.name} t:Material", new[] { "Assets/_/Content/Art/Materials" });

            if (guids.Length > 0)
            {
                // Return existing Material
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Material>(path);
            }
            else
            {
                //Create new Material
                material.name = $"{material.name}";
                string path = $"Assets/_/Content/Art/Materials/{material.name}.mat";

                path = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CreateAsset(material, path);

                Debug.Log($"Material '{material.name}' created!");

                return material;
            }
        }

        private bool ObjectAlreadyExist(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:Object", new[] { "Assets/_/Content/Art/Meshes" });

            return guids.Length == 0;
        }

    }
}