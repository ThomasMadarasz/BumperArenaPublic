using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UIFeedbacks.Runtime
{
    [RequireComponent(typeof(MMF_Player), typeof(Selectable))]
    public class SelectedFeedbacks : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        #region Unity API

        private void Awake() => Setup();

        protected void Reset()
        {
            _player = GetComponent<MMF_Player>();
            _ui = GetComponent<Selectable>();
        }

        #endregion


        #region Main

        protected void Setup()
        {
            if(_player == null) _player = GetComponent<MMF_Player>();
            if(_ui == null) _ui = GetComponent<Selectable>();
        }

        #endregion


        #region Utils & Tools

        public void OnSelect(BaseEventData eventData)
        {
            _player.Direction = MMFeedbacks.Directions.TopToBottom;
            _player.PlayFeedbacks();
            Debug.Log(gameObject.name);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _player.Direction = MMFeedbacks.Directions.BottomToTop;
            _player.PlayFeedbacks();
        }


        protected void ResetFeedbacks()
        {
            _player.Direction = MMFeedbacks.Directions.TopToBottom;
            _player.StopFeedbacks();
            _player.ResetFeedbacks();
            _player.RestoreInitialValues();
        }

        public void PlayFeedbacks()
        {
            if (_player == null) return;
            _player.Direction = MMFeedbacks.Directions.TopToBottom;
            _player.PlayFeedbacks();
        }

        public void PublicResetFeedbacks()
        {
            if (_player == null) return;
            _player.Direction = MMFeedbacks.Directions.BottomToTop;
            _player.PlayFeedbacks();
        }

        #endregion


        #region Private

        private MMF_Player _player;
        private Selectable _ui;

        #endregion
    }
}