using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using Enum.Runtime;
using ScriptableEvent.Runtime;

namespace ScreenShakes.Runtime
{
    public class ScreenShakeManager : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private MMF_Player _wiggleNoiseShake;
        [SerializeField] private MMF_Player _impulseShake;
        [SerializeField] private MMF_Player _zoomShake;

        [SerializeField] private GameEventT _onScreenShake;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onScreenShake.UnregisterListener(PlayScreenShake);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onScreenShake.RegisterListener(PlayScreenShake);

            _wiggleNoiseSettings = _wiggleNoiseShake.GetFeedbackOfType<MMF_CameraShake>();
            _impulseSettings = _impulseShake.GetFeedbackOfType<MMF_CinemachineImpulse>();
            _zoomSettings = _zoomShake.GetFeedbackOfType<MMF_CameraZoom>();
        }

        private void PlayScreenShake(object obj)
        {
            ScreenShakeParameters parameters = (ScreenShakeParameters)obj;

            switch (parameters.m_type)
            {
                case ScreenShakeType.None:
                    break;
                case ScreenShakeType.WiggleNoise:
                    _wiggleNoiseSettings.FeedbackDuration = parameters.m_duration;
                    _wiggleNoiseSettings.CameraShakeProperties.Amplitude = parameters.m_amplitude;
                    _wiggleNoiseSettings.CameraShakeProperties.Frequency = parameters.m_frequency;
                    _wiggleNoiseShake.PlayFeedbacks();
                    break;
                case ScreenShakeType.Impulse:
                    _impulseSettings.FeedbackDuration = parameters.m_duration;
                    _impulseSettings.m_ImpulseDefinition.m_AmplitudeGain = parameters.m_amplitude;
                    _impulseSettings.m_ImpulseDefinition.m_FrequencyGain = parameters.m_frequency;
                    _impulseShake.PlayFeedbacks();
                    break;
                case ScreenShakeType.Zoom:
                    _zoomSettings.FeedbackDuration = parameters.m_duration;
                    _zoomSettings.ZoomFieldOfView = parameters.m_duration;
                    _zoomShake.PlayFeedbacks();
                    break;
                default:
                    break;
            }
        }

        #endregion


        #region Private

        private MMF_CameraShake _wiggleNoiseSettings;
        private MMF_CinemachineImpulse _impulseSettings;
        private MMF_CameraZoom _zoomSettings;

        #endregion
    }
}