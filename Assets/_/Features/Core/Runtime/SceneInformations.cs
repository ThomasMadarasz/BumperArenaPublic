using Archi.Runtime;
using UnityEngine;

namespace Core.Runtime
{
    public class SceneInformations : CBehaviour
    {
        #region Exposed

        public static SceneInformations s_instance;

        [SerializeField] private SpawnPoint[] _spawnPoints;

        public SpawnPoint[] m_spawnPoints { get { return _spawnPoints; } }

        #endregion


        #region Unity API

        private void Awake()
        {
            if (s_instance != null) Destroy(s_instance);
            s_instance = this;
        }

        #endregion
    }
}