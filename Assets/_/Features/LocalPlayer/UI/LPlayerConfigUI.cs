using LocalPlayer.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Translation;
using MoreMountains.Feedbacks;

namespace LocalPlayer.UI.Runtime
{
    public class LPlayerConfigUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _playerIndexTxt;
        [SerializeField] private Image _deviceImg;
        [SerializeField] private Image _playerColorImg;
        [SerializeField] private Image _playerChangeColorImg;
        [SerializeField] private MMF_Player _onColorSelectedFeedback;

        [SerializeField] private Toggle _orientationWorldToggle;
        [SerializeField] private Toggle _orientationPlayerdToggle;
        [SerializeField] private MMF_Player _onOrientationSelectedFeedback;
        [SerializeField] private MMF_Player[] _onOrientationToggleFeedback;

        [SerializeField] private Toggle _onRumbleToggle;
        [SerializeField] private Toggle _offRumbleToggle;
        [SerializeField] private MMF_Player _onRumbleSelectedFeedback;
        [SerializeField] private MMF_Player[] _onRumbleToggleFeedback;

        [SerializeField] private Sprite _playstationDeviceSprite;
        [SerializeField] private Sprite _xboxDeviceSprite;
        [SerializeField] private Sprite _desktopDeviceSprite;

        [SerializeField] private Color _defaultEnabledColor;
        [SerializeField] private Color _defaultColor;

        #endregion


        #region Main

        public void Setup(LPlayer player, Enum.Runtime.DeviceType type)
        {
            _player = player;
            string pNumber = TranslationManager.Translate("PlayerNumberKey");
            _playerIndexTxt.text = $"{pNumber} {_player.m_localPlayerID}";

            _player.m_onWorldOrientationChanged += UpdateOrientationToggle;
            _player.m_onRumbleChanged += UpdateUseRumble;
            _player.m_onPlayerConfigValidated += OnPlayerConfigValidated;
            _player.m_onColorSelected += SelectColor;
            _player.m_onOrientationSelected += SelectOrientation;
            _player.m_onRumbleSelected += SelectRumble;

            switch (type)
            {
                case Enum.Runtime.DeviceType.Playstation:
                    _deviceImg.sprite = _playstationDeviceSprite;
                    break;

                case Enum.Runtime.DeviceType.Xbox:
                    _deviceImg.sprite = _xboxDeviceSprite;
                    break;

                case Enum.Runtime.DeviceType.Desktop:
                    _deviceImg.sprite = _desktopDeviceSprite;
                    break;
            }
        }

        public void UpdateText()
        {
            string pNumber = TranslationManager.Translate("PlayerNumberKey");
            _playerIndexTxt.text = $"{pNumber} {_player.m_localPlayerID}";
        }

        #endregion


        #region Utils & Tools

        private void UpdateOrientationToggle(bool isWorld)
        {
            _orientationWorldToggle.isOn = isWorld;
            _orientationPlayerdToggle.isOn = !isWorld;
        }

        private void UpdateUseRumble(bool enable)
        {
            _onRumbleToggle.isOn = enable;
            _offRumbleToggle.isOn = !enable;
        }

        private void OnPlayerConfigValidated()
        {
            _player.m_onWorldOrientationChanged -= UpdateOrientationToggle;
            _player.m_onRumbleChanged -= UpdateUseRumble;
            _player.m_onPlayerConfigValidated -= OnPlayerConfigValidated; 
            _player.m_onColorSelected -= SelectColor;
            _player.m_onOrientationSelected -= SelectOrientation;
            _player.m_onRumbleSelected -= SelectRumble;

            this.enabled = false;
        }

        public void UpdatePlayerColor(Color color)
        {
            _playerColorImg.color = color;
            _playerChangeColorImg.color = color;
            _playerColor = color;
            SwitchColorsInFeedbacks();
        }

        public void ChangeColor()
        {
            _player.ChangeToNextColor();
        }

        private void SelectColor(Color color, int index)
        {
            PlayFeedbacks(0, index);
        }

        private void SelectOrientation(int index) => PlayFeedbacks(1, index);

        private void SelectRumble(int index) => PlayFeedbacks(2, index);

        private void PlayFeedbacks(int currentIndex, int previousIndex)
        {
            if (_isFirstInitialization)
            {
                _isFirstInitialization = false;
                SwitchColorsInFeedbacks();
            }

            if (currentIndex == 0)
            {
                _onColorSelectedFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
                _onColorSelectedFeedback.PlayFeedbacks();
            }
            else if (currentIndex == 1)
            {
                foreach (MMF_Player player in _onOrientationToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = true;
                }
                MMF_Player feedback = _orientationWorldToggle.isOn ? _onOrientationToggleFeedback[0] : _onOrientationToggleFeedback[1];
                feedback.Direction = MMFeedbacks.Directions.TopToBottom;
                feedback.PlayFeedbacks();
                _onOrientationSelectedFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
                _onOrientationSelectedFeedback.PlayFeedbacks();
            }
            else if (currentIndex == 2)
            {
                foreach (MMF_Player player in _onRumbleToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = true;
                }
                MMF_Player feedback = _onRumbleToggle.isOn ? _onRumbleToggleFeedback[0] : _onRumbleToggleFeedback[1];
                feedback.Direction = MMFeedbacks.Directions.TopToBottom;
                feedback.PlayFeedbacks();
                _onRumbleSelectedFeedback.Direction = MMFeedbacks.Directions.TopToBottom;
                _onRumbleSelectedFeedback.PlayFeedbacks();
            }

            if (previousIndex == 0)
            {
                _onColorSelectedFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
                _onColorSelectedFeedback.PlayFeedbacks();
            }
            else if (previousIndex == 1)
            {
                foreach (MMF_Player player in _onOrientationToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = false;
                }
                MMF_Player feedback = _orientationWorldToggle.isOn ? _onOrientationToggleFeedback[0] : _onOrientationToggleFeedback[1];
                feedback.Direction = MMFeedbacks.Directions.BottomToTop;
                feedback.PlayFeedbacks();
                _onOrientationSelectedFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
                _onOrientationSelectedFeedback.PlayFeedbacks();
            }
            else if (previousIndex == 2)
            {
                foreach (MMF_Player player in _onRumbleToggleFeedback)
                {
                    foreach (MMF_TMPColor textColor in player.GetFeedbacksOfType<MMF_TMPColor>()) textColor.Active = false;
                }
                MMF_Player feedback = _onRumbleToggle.isOn ? _onRumbleToggleFeedback[0] : _onRumbleToggleFeedback[1];
                feedback.Direction = MMFeedbacks.Directions.BottomToTop;
                feedback.PlayFeedbacks();
                _onRumbleSelectedFeedback.Direction = MMFeedbacks.Directions.BottomToTop;
                _onRumbleSelectedFeedback.PlayFeedbacks();
            }
        }

        private void SwitchColorsInFeedbacks()
        {
            Gradient gradient = _onColorSelectedFeedback.GetFeedbackOfType<MMF_Image>().ColorOverTime;
            GradientColorKey[] keys = { gradient.colorKeys[0], gradient.colorKeys[1] };
            keys[0].color = _defaultEnabledColor;
            keys[1].color = _playerColor;
            gradient.SetKeys(keys, gradient.alphaKeys);

            if (_isFirstInitialization) return;

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


        #region Private

        private LPlayer _player;

        private Color _playerColor;

        private bool _isFirstInitialization = true;

        #endregion
    }
}