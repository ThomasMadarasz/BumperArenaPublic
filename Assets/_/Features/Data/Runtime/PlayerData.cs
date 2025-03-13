using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Runtime
{
    public class PlayerData : MonoBehaviour
    {
        #region Exposed

        public static PlayerData s_instance;

        public PlayerSteamData m_playerSteamData;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance == null) s_instance = this;
            else Destroy(gameObject);
        }

        #endregion
    }
}