using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Internal.Editor
{
    public class CustomMenu
    {
        [MenuItem("Internal/Check Bakery Directory")]
        private static void CheckBakeryDirectory()
        {
            bool directoryExist = CheckPresenceOfBakeryDirectory();
            Debug.Log(GetBakeryDirectoreyPath());
            if (directoryExist)
            {
                Debug.Log("Bakery Directory is present!");
                return;
            }

            Debug.Log("Copy Bakery Directory...");

            DownloadBakeryDirectory();
            AssetDatabase.Refresh();

            Debug.Log("Bakery Directory copy is finished");
        }

        private static string GetBakeryDirectoreyPath()
        {
            string path = Path.Combine(Application.dataPath, "Plugins", "Bakery");

            path = path.Replace("\\", "/");
            return path;
        }

        private static bool CheckPresenceOfBakeryDirectory()
        {
            return Directory.Exists(GetBakeryDirectoreyPath());
        }

        private static void DownloadBakeryDirectory()
        {
            string originPath = @"Z:\Bakery";
            if (!Directory.Exists(originPath))
            {
                Debug.LogError($"Path for bakery not exist!{Environment.NewLine}{originPath}");
                return;
            }

            string destinationPath = GetBakeryDirectoreyPath();
            CopyDirectory(originPath, destinationPath, true);
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            if (!Directory.Exists(destinationDir)) Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                targetFilePath = targetFilePath.Replace("\\", "/");
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}