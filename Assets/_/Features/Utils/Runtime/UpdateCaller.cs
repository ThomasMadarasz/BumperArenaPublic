using System;
using UnityEngine;

namespace Utils.Runtime
{
    public class UpdateCaller : MonoBehaviour
    {
        #region Exposed

        public static UpdateCaller s_instance;
        public static Action<float> OnUpdate;
        public static Action<float> OnUnscaledUpdate;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update()
        {
            OnUpdate?.Invoke(Time.deltaTime);
            OnUnscaledUpdate?.Invoke(Time.unscaledDeltaTime);
        }

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance == null)
            {
                s_instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}