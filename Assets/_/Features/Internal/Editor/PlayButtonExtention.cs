using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace Internal.Editor
{
    [InitializeOnLoad]
    public class PlayButtonExtention
    {
        static PlayButtonExtention()
        {
            EditorApplication.playModeStateChanged += OnPlayModstateChanged;
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Launch Game", "Start Scene Manager"), ToolbarStyles.commandButtonStyle))
            {
                if (EditorApplication.isPlaying) return;
                SessionState.SetBool("customLaunch", true);

                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                SessionState.SetString("startingScenePath", EditorSceneManager.GetActiveScene().path);
                EditorSceneManager.OpenScene("Assets/_/Database/Scenes/Manager.unity");
                EditorApplication.isPlaying = true;
            }
        }

        private static void OnPlayModstateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (!SessionState.GetBool("customLaunch", false)) return;
            SessionState.SetBool("customLaunch", false);
            EditorSceneManager.OpenScene(SessionState.GetString("startingScenePath", string.Empty));
        }

    }


    static class ToolbarStyles
    {
        public static readonly GUIStyle commandButtonStyle;

        static ToolbarStyles()
        {
            commandButtonStyle = new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Bold,
                fixedWidth = 100
            };
        }
    }
}