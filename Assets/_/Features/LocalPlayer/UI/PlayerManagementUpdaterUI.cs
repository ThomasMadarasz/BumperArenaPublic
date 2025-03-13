using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalPlayer.UI.Runtime
{
    public class PlayerManagementUpdaterUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private PlayerManagementUI _playerManagementUI;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            _playerManagementUI.ShowHideInfo();
        }

        #endregion


        #region Private
        #endregion
    }
}