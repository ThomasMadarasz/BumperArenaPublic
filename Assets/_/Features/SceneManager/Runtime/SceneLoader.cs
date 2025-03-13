using Archi.Runtime;
using ScriptableEvent.Runtime;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using sc = UnityEngine.SceneManagement;

namespace SceneManager.runtime
{
    public class SceneLoader : CBehaviour
    {
        #region Exposed

        public static SceneLoader s_instance;

        [SerializeField] private string _mainMenuSceneName;
        [SerializeField] private string _startupSceneName;
        [SerializeField] private string _pauseMenuSceneName;
        [SerializeField] private string _settingsMenuSceneName;

#if UNITY_EDITOR
        [SerializeField] private GameEvent _onSteamDataLoaded;
#endif

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance == null) s_instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            sc.SceneManager.sceneLoaded += OnSceneLoaded;
            sc.SceneManager.sceneUnloaded += OnSceneUnloaded;

#if UNITY_EDITOR
            _onSteamDataLoaded.RegisterListener(LoadMainMenuDebug);
#endif
        }

        public bool PauseMenuIsLoaded()
        {
            bool sceneIsLoaded = false;

            for (int i = 0; i < sc.SceneManager.sceneCount; i++)
            {
                if (sc.SceneManager.GetSceneAt(i).name == _pauseMenuSceneName)
                {
                    sceneIsLoaded = true;
                    break;
                }
            }

            return sceneIsLoaded;
        }

        public bool SettingsMenuIsLoaded()
        {
            bool sceneIsLoaded = false;

            for (int i = 0; i < sc.SceneManager.sceneCount; i++)
            {
                if (sc.SceneManager.GetSceneAt(i).name == _settingsMenuSceneName)
                {
                    sceneIsLoaded = true;
                    break;
                }
            }

            return sceneIsLoaded;
        }

        public void LoadSettingsMenu() => StartCoroutine(LoadAsync(_settingsMenuSceneName, null));
        public void UnloadSettingsMenu() => StartCoroutine(UnloadAsync(_settingsMenuSceneName, null));

        public void LoadPauseMenu() => StartCoroutine(LoadAsync(_pauseMenuSceneName, null));
        public void UnloadPauseMenu() => StartCoroutine(UnloadAsync(_pauseMenuSceneName, null));

        public void LoadMainMenu(Action callback) => StartCoroutine(LoadAsync(_mainMenuSceneName, callback));
        private void LoadMainMenuDebug() => StartCoroutine(LoadAsync(_mainMenuSceneName, null));

        public void UnloadStartupScene()
        {
            sc.SceneManager.UnloadSceneAsync(_startupSceneName);
        }

        public void LoadSceneAsync(string sceneName, Action callback) => StartCoroutine(LoadAsync(sceneName, callback));

        public AsyncOperation LaunchLoadSceneAsync(string sceneName)
        {
            _lastLoadedScene = sceneName;
            return sc.SceneManager.LoadSceneAsync(sceneName, sc.LoadSceneMode.Additive);
        }

        private IEnumerator LoadAsync(string sceneToLoad, Action callback)
        {
            if (SceneIsAlreadyLoaded(sceneToLoad)) yield break;

            _lastLoadedScene = sceneToLoad;

            AsyncOperation async = sc.SceneManager.LoadSceneAsync(sceneToLoad, sc.LoadSceneMode.Additive);

            while (!async.isDone)
            {
                yield return null;
            }

            sc.SceneManager.SetActiveScene(sc.SceneManager.GetSceneByName(sceneToLoad));
            callback?.Invoke();
        }

        private IEnumerator UnloadAsync(string sceneToUnload, Action callback)
        {
            if (!string.IsNullOrWhiteSpace(sceneToUnload))
            {
                bool sceneIsLoaded = false;

                for (int i = 0; i < sc.SceneManager.sceneCount; i++)
                {
                    if (sc.SceneManager.GetSceneAt(i).name == sceneToUnload)
                    {
                        sceneIsLoaded = true;
                        break;
                    }
                }

                if (!sceneIsLoaded) Debug.LogWarning("The scene to be unloaded is not loaded");
                else
                {
                    AsyncOperation async = sc.SceneManager.UnloadSceneAsync(sceneToUnload);

                    while (!async.isDone)
                    {
                        yield return null;
                    }
                }
            }

            callback?.Invoke();
        }

        public void SetActiveScene()
        {
            string sceneName = GetSceneNameWithPath(_lastLoadedScene);
            sc.SceneManager.SetActiveScene(sc.SceneManager.GetSceneByName(sceneName));
        }

        public void UnloadSceneAsync(string sceneName, Action callback) => StartCoroutine(UnloadAsync(sceneName, callback));

        public string GetCurrentActiveScene() => sc.SceneManager.GetActiveScene().name;

        public string GetLastLoadedScene() => GetSceneNameWithPath(_lastLoadedScene);

        private void ReloadLightProbes()
        {
            LightProbes.Tetrahedralize();
        }

        private void OnSceneLoaded(sc.Scene scene, sc.LoadSceneMode mode)
        {
            ReloadLightProbes();
        }

        private void OnSceneUnloaded(sc.Scene scene)
        {
            if (scene.name == _settingsMenuSceneName)
            {
                for (int i = 0; i < sc.SceneManager.sceneCount; i++)
                {
                    sc.Scene loadedScene = sc.SceneManager.GetSceneAt(i);
                    if (loadedScene.name.Contains("MainMenu_"))
                    {
                        sc.SceneManager.SetActiveScene(loadedScene);
                        break;
                    }
                }
            }
        }

        private bool SceneIsAlreadyLoaded(string sceneName)
        {
            for (int i = 0; i < sc.SceneManager.sceneCount; i++)
            {
                sc.Scene loadedScene = sc.SceneManager.GetSceneAt(i);
                if (loadedScene.name == sceneName)
                {
                    Debug.LogError($"Scene {sceneName} already loaded, operation abandoned");
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Utils & Tools

        private string GetSceneNameWithPath(string path)
        {
            string sceneName = path.Replace(".unity", string.Empty);
            return sceneName.Split('/').Last();
        }

        #endregion


        #region Private

        private string _lastLoadedScene;

        #endregion
    }
}