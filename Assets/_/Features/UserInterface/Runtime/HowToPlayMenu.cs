using Sirenix.OdinInspector;
using UINavigation.Runtime;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UserInterface.Runtime
{
    public class HowToPlayMenu : MenuBase
    {
        #region Exposed

        [SerializeField][BoxGroup("General")] private GameObject _nextPageBtn;
        [SerializeField][BoxGroup("General")] private GameObject _previousPageBtn;
        [SerializeField][BoxGroup("General")] private Button _cancelBtn;

        [SerializeField][BoxGroup("Explanation")] private HowToPlayData[] _data;
        [SerializeField][BoxGroup("Explanation")] private LocalizeStringEvent _explanationText;
        [SerializeField][BoxGroup("Explanation")] private VideoPlayer _videoPlayer;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentPageIndex = 0;
            UpdateUI();
        }

        private void Update() => UpdateAutomaticTextChanger();

        #endregion


        #region Utils & Tools

        private void UpdateAutomaticTextChanger()
        {
            if (!_useChangeText) return;

            if (_textHasBenChanged)
            {
                if (_videoPlayer.time < _targetedTime)
                {
                    _textHasBenChanged = false;
                    _explanationText.StringReference = _data[_currentPageIndex].m_explanation[0];
                }
            }
            else
            {
                if (_videoPlayer.time >= _targetedTime)
                {
                    _textHasBenChanged = true;
                    _explanationText.StringReference = _data[_currentPageIndex].m_explanation[1];
                }
            }
        }

        private void UpdateUI()
        {
            _textHasBenChanged = false;
            _targetedTime = _data[_currentPageIndex].m_timeBeforeChangeText;
            _useChangeText = _data[_currentPageIndex].m_useTimerForChangeText;

            _explanationText.StringReference = _data[_currentPageIndex].m_explanation[0];
            _videoPlayer.clip = _data[_currentPageIndex].m_clip;

            _previousPageBtn.SetActive(_currentPageIndex > 0);
            _nextPageBtn.SetActive(_currentPageIndex < _data.Length - 1);
        }

        public void GoToNextPage()
        {
            if (_currentPageIndex == _data.Length - 1) return;
            _currentPageIndex++;
            UpdateUI();
        }

        public void GoToPreviousPage()
        {
            if (_currentPageIndex == 0) return;
            _currentPageIndex--;
            UpdateUI();
        }

        public override void OnCancel_Performed(int playerId)
        {
            _cancelBtn.onClick.Invoke();
        }

        public override void OnRightShoulder_Performed(int playerdId)
        {
            GoToNextPage();
        }

        public override void OnLeftShoulder_Performed(int playerdId)
        {
            GoToPreviousPage();
        }

        #endregion


        #region Private

        private int _currentPageIndex;
        private float _targetedTime;
        private bool _useChangeText;
        private bool _textHasBenChanged;

        #endregion
    }
}