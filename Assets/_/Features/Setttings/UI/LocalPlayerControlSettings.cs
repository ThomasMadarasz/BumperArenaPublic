using MoreMountains.Feedbacks;
using Settings.Runtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Translation;

namespace Settings.UI
{
    public class LocalPlayerControlSettings : MonoBehaviour
    {
        #region Exposed

        [BoxGroup("Controls")][SerializeField] private Toggle _orientationWorldToggle;
        [BoxGroup("Controls")][SerializeField] private Toggle _playerToggle;

        [BoxGroup("Controls")][SerializeField] private ToggleGroup _controlToggleGroup;
        [BoxGroup("Controls")][SerializeField] private MMF_Player _onOrientationSelectedFeedback;
        [BoxGroup("Controls")][SerializeField] private MMF_Player[] _onOrientationToggleFeedback;

        [BoxGroup("Rumble")][SerializeField] private Toggle _onRumbleToggle;
        [BoxGroup("Rumble")][SerializeField] private Toggle _offToggle;

        [BoxGroup("Rumble")][SerializeField] private ToggleGroup _rumbleToggleGroup;
        [BoxGroup("Rumble")][SerializeField] private MMF_Player _onRumbleSelectedFeedback;
        [BoxGroup("Rumble")][SerializeField] private MMF_Player[] _onRumbleToggleFeedback;

        [SerializeField] private TextMeshProUGUI _txt;

        [SerializeField] private Color _defaultEnabledColor;

        #endregion


        #region Utils & Tools

        public void SetPlayerText(int id)
        {
           string playerNumberTxt = TranslationManager.Translate("PlayerNumberKey");
            _txt.text = $"{playerNumberTxt}{id}";
        }

        public void SetPlayerColor(Color color)
        {
            _txt.color = color;
            _playerColor = color;
            SwitchColorsInFeedbacks();
        }

        public void RefreshRumble(bool useRumble)
        {
            _rumbleToggleGroup.SetAllTogglesOff(false);

            if (useRumble) _onRumbleToggle.SetIsOnWithoutNotify(true);
            else _offToggle.SetIsOnWithoutNotify(true);
        }

        public void RefreshControls(bool useWorldOrientation)
        {
            _controlToggleGroup.SetAllTogglesOff(false);

            if (useWorldOrientation) _orientationWorldToggle.SetIsOnWithoutNotify(true);
            else _playerToggle.SetIsOnWithoutNotify(true);

            PlayFeedbacks(0);
        }

        public void PlayerValidate()
        {
            if (_currentIndex == 0)
            {
                bool isOn = _orientationWorldToggle.isOn;
                _orientationWorldToggle.isOn = !_orientationWorldToggle.isOn;
                _playerToggle.isOn = isOn;
            }
            else
            {
                bool isOn = _onRumbleToggle.isOn;
                _onRumbleToggle.isOn = !_onRumbleToggle.isOn;
                _offToggle.isOn = isOn;
            }
        }

        public void PlayerNavigateUp()
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = 1;
            PlayFeedbacks(_currentIndex);

        }

        public void PlayerNavigateDown()
        {
            // 1 orientation
            // 2 rumble
            _currentIndex++;
            if (_currentIndex > 1) _currentIndex = 0;
            PlayFeedbacks(_currentIndex);
        }

        private void PlayFeedbacks(int index)
        {
            if(index == 0)
            {
                foreach (MMF_Player player in _onOrientationToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = true;
                }
                MMF_Player feedbackOrientation = _orientationWorldToggle.isOn ? _onOrientationToggleFeedback[0] : _onOrientationToggleFeedback[1];
                feedbackOrientation.Direction = MMFeedbacks.Directions.TopToBottom;
                feedbackOrientation.PlayFeedbacks();
                _onOrientationSelectedFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
                _onOrientationSelectedFeedback.PlayFeedbacks();

                foreach (MMF_Player player in _onRumbleToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = false;
                }
                MMF_Player feedbackRumble = _onRumbleToggle.isOn ? _onRumbleToggleFeedback[0] : _onRumbleToggleFeedback[1];
                feedbackRumble.Direction = MMFeedbacks.Directions.BottomToTop;
                feedbackRumble.PlayFeedbacks();
                _onRumbleSelectedFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
                _onRumbleSelectedFeedback.PlayFeedbacks();
            }
            else if(index == 1)
            {
                foreach (MMF_Player player in _onRumbleToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = true;
                }
                MMF_Player feedbackRumble = _onRumbleToggle.isOn ? _onRumbleToggleFeedback[0] : _onRumbleToggleFeedback[1];
                feedbackRumble.Direction = MMFeedbacks.Directions.TopToBottom;
                feedbackRumble.PlayFeedbacks();
                _onRumbleSelectedFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
                _onRumbleSelectedFeedback.PlayFeedbacks();

                foreach (MMF_Player player in _onOrientationToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = false;
                }
                MMF_Player feedbackOrientation = _orientationWorldToggle.isOn ? _onOrientationToggleFeedback[0] : _onOrientationToggleFeedback[1];
                feedbackOrientation.Direction = MMFeedbacks.Directions.BottomToTop;
                feedbackOrientation.PlayFeedbacks();
                _onOrientationSelectedFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
                _onOrientationSelectedFeedback.PlayFeedbacks();
            }

        }

        public bool GetRumbleValue() => _onRumbleToggle.isOn;
        public bool GetOrientationIsWorldValue() => _orientationWorldToggle.isOn;


        private void SwitchColorsInFeedbacks()
        {
            foreach (var item in _onOrientationToggleFeedback)
            {
                Gradient gradientOrientation = item.GetFeedbackOfType<MMF_Image>().ColorOverTime;
                GradientColorKey[] keysOrientation = { gradientOrientation.colorKeys[0], gradientOrientation.colorKeys[1] };
                keysOrientation[0].color = _defaultEnabledColor;
                keysOrientation[1].color = _playerColor;
                gradientOrientation.SetKeys(keysOrientation, gradientOrientation.alphaKeys);
            }

            foreach (var item in _onRumbleToggleFeedback)
            {
                Gradient gradientRumble = item.GetFeedbackOfType<MMF_Image>().ColorOverTime;
                GradientColorKey[] keysRumble = { gradientRumble.colorKeys[0], gradientRumble.colorKeys[1] };
                keysRumble[0].color = _defaultEnabledColor;
                keysRumble[1].color = _playerColor;
                gradientRumble.SetKeys(keysRumble, gradientRumble.alphaKeys);
            }

        }

        #endregion


        #region Events

        public void OnRumbleOnValueChanged(bool value)
        {
            if (!value) return;
            SettingsManager.s_instance.ChangeRumble(true);
        }

        public void OnRumbleOffValueChanged(bool value)
        {
            if (!value) return;
            SettingsManager.s_instance.ChangeRumble(false);
        }

        public void OnControlWorldOnValueChanged(bool value)
        {
            if (!value) return;
            SettingsManager.s_instance.ChangeControls(ControlType.World);
        }

        public void OnControlPlayerOnValueChanged(bool value)
        {
            if (!value) return;
            SettingsManager.s_instance.ChangeControls(ControlType.Player);
        }

        #endregion


        #region Private

        private int _currentIndex;
        private Color _playerColor;

        #endregion
    }
}