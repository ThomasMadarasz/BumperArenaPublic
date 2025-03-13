using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace UIFeedbacks.Runtime
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropDownListFeedbacks : SelectedFeedbacks, ICancelHandler
    {
        #region Unity API
        private void Reset()
        {
            base.Reset();
            _dropDown = GetComponent<TMP_Dropdown>();
        }

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            base.Setup();
            if (_dropDown == null) _dropDown = GetComponent<TMP_Dropdown>();
        }

        #endregion


        #region Utils & Tools

        public void OnCancel(BaseEventData eventData)
        {
            base.ResetFeedbacks();
        }

        #endregion


        #region Debug
        #endregion


        #region Private

        private TMP_Dropdown _dropDown;

        #endregion
    }
}