using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Internal.Editor
{
    public class BuildTools
    {
        #region Main

        [MenuItem("Internal/Build Dev")]
        private static void BuildDev()
        {
            BuildPlayerOptions buildPlayerOptions = new();

            buildPlayerOptions.scenes = GetBuildScenes();

            string directoryPath = GetBuildDirectoryPath(BuildType.Dev);
            CopySteamAppFile(directoryPath);

            buildPlayerOptions.locationPathName = Path.Combine(directoryPath, $"{Application.productName}.exe");
            buildPlayerOptions.target = BuildTarget.StandaloneWindows;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.CompressWithLz4 | BuildOptions.AllowDebugging | BuildOptions.StrictMode;

            Build(BuildType.Dev, buildPlayerOptions);
        }


        [MenuItem("Internal/Build Release")]
        private static void BuildRelease()
        {
            BuildPlayerOptions buildPlayerOptions = new();

            buildPlayerOptions.scenes = GetBuildScenes();

            string directoryPath = GetBuildDirectoryPath(BuildType.Release);
            CopySteamAppFile(directoryPath);

            buildPlayerOptions.locationPathName = Path.Combine(directoryPath, $"{Application.productName}.exe");
            buildPlayerOptions.target = BuildTarget.StandaloneWindows;
            buildPlayerOptions.options = BuildOptions.DetailedBuildReport | BuildOptions.CompressWithLz4HC | BuildOptions.StrictMode;

            Build(BuildType.Release, buildPlayerOptions);
        }

        [MenuItem(OPEN_FOLDER_BUILD_MENU_NAME)]
        private static void OpenFolderAfterBuild()
        {
            bool value = !Menu.GetChecked(OPEN_FOLDER_BUILD_MENU_NAME);

            Menu.SetChecked(OPEN_FOLDER_BUILD_MENU_NAME, value);
            EditorPrefs.SetBool(OPEN_FOLDER_BUILD_EDITOR, value);
        }

        #endregion


        #region Utils

        private static void CopySteamAppFile(string destination)
        {
            string filePath = Path.Combine(GetOriginDirectory(), "steam_appid.txt");
            string originFilePath = Path.Combine(destination, "steam_appid.txt");

            File.Copy(filePath, originFilePath, true);
        }

        private static void Build(BuildType buildType, BuildPlayerOptions buildPlayerOptions)
        {
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded) LogSuccess(buildType, summary);
            else if (summary.result == BuildResult.Failed) LogFailed(buildType, summary);
        }

        private static void LogSuccess(BuildType buildType, BuildSummary summary)
        {
            Debug.Log($"{buildType} Build succeded : " + GetBuildSize(summary.totalSize) + Environment.NewLine + $"Duration : {summary.totalTime.Minutes}m {summary.totalTime.Seconds}s");
            OpenBuildFolder(buildType);
        }

        private static void LogFailed(BuildType buildType, BuildSummary summary)
        {
            Debug.LogError($"{buildType} Build failed : " + GetBuildSize(summary.totalSize));
        }

        private static void OpenBuildFolder(BuildType buildType)
        {
            if (!EditorPrefs.GetBool(OPEN_FOLDER_BUILD_EDITOR)) return;
            //string path = Path.Combine(GetBuildDirectoryPath(buildType), $"{Application.productName}.exe");
            string path = GetBuildDirectoryPath(buildType);

            System.Diagnostics.ProcessStartInfo startInfo = new()
            {
                Arguments = path,
                FileName = "explorer.exe"
            };

            System.Diagnostics.Process.Start(startInfo);
        }

        private static string GetBuildDirectoryPath(BuildType buildType)
        {
            string path = Path.Combine(GetOriginDirectory(), $"Build_{buildType}");

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }


        private static string GetOriginDirectory() => Path.GetDirectoryName(Application.dataPath);

        private static string[] GetBuildScenes() => EditorBuildSettings.scenes.Select(x => x.path).ToArray();

        private static string GetBuildSize(ulong bytes)
        {
            ulong gb = bytes / 1000000000;
            ulong mb = (bytes / 1000000) % 1000;

            return $"{gb},{mb} Go";
        }

        #endregion


        #region Private

        private const string OPEN_FOLDER_BUILD_MENU_NAME = "Internal/Open Folder after build";
        private const string OPEN_FOLDER_BUILD_EDITOR = "OpenFolder";

        #endregion
    }

    internal enum BuildType
    {
        Unknown = 0,
        Dev = 1,
        Release = 2
    }
}