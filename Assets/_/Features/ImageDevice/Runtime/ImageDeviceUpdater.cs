using Data.Runtime;
using ScriptableEvent.Runtime;
using Settings.Runtime;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ImageDevice.Runtime
{
    public class ImageDeviceUpdater : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Image _img;
        [SerializeField] GameEventT _onControlDeviceTypeUIChanged;
        [SerializeField] private ControlTypeUIData _data;
        [SerializeField] private ControlTypeEnum _currentType;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onControlDeviceTypeUIChanged.UnregisterListener(UpdateCurrentImage);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onControlDeviceTypeUIChanged.RegisterListener(UpdateCurrentImage);

            UpdateCurrentImage(SettingsManager.s_instance.GetCurrentDeviceTypeUI());
            UpdateCurrentImage(SettingsManager.s_instance.GetCurrentDeviceTypeUI());
            UpdateCurrentImage(SettingsManager.s_instance.GetCurrentDeviceTypeUI());
        }

        private void UpdateCurrentImage(object obj)
        {
            ControlDeviceProperties prop = _data._deviceProperties.FirstOrDefault(x => x._type == _currentType);
            if(prop == null)
            {
                Debug.LogError($"No image found for this type : {_currentType}");
                return;
            }

            int value = (int)obj;
            UIControlDeviceType newType = (UIControlDeviceType)value;

            switch (newType)
            {
                case UIControlDeviceType.Keyboard:
                    _img.sprite = prop._keyboardImage;
                    break;

                case UIControlDeviceType.Playstation:
                    _img.sprite = prop._playstationImage;
                    break;

                case UIControlDeviceType.Xbox:
                    _img.sprite = prop._xboxImage;
                    break;
            }

        }

        #endregion
    }
}