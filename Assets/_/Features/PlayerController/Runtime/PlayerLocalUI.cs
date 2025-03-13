using Archi.Runtime;
using Feedbacks.Runtime;
using Inputs.Runtime;
using UINavigation.Runtime;
using UnityEngine;

namespace PlayerController.Runtime
{
    public class PlayerLocalUI : CBehaviour
    {
        #region Unity API

        private void Start()
        {
            _p = GetComponent<PlayerProperties>();
            _feedbacks = GetComponent<PlayerFeedbacks>();
            _inputReader = _p.m_inputs;
            Setup();
        }

        private void OnDestroy()
        {
            if (!_p.m_isAnAI)
            {
                UnregisterEvents();
            }
        }

        #endregion


        #region Main

        private void Setup()
        {
            if (!_p.m_isAnAI)
            {
                RegisterEvents();
            }
        }

        #endregion


        #region Utils & Tools

        private void RegisterEvents()
        {
            _inputReader.m_onMenuPerformed += OnMenu_Performed;
            _inputReader.m_onCancelPerformed += OnCancel_Performed;
            _inputReader.m_onNavigatePerformed += OnNavigate_Performed;
            _inputReader.m_onSubmitPerformed += OnSubmit_Performed;
            _inputReader.m_onSubmitStarted += OnSubmit_Started;
            _inputReader.m_onSubmitCanceled += OnSubmit_Canceled;

            _inputReader.m_onWestButtonPerformed += OnWestButton_Performed;
            _inputReader.m_onNorthButtonPerformed += OnNorthButton_Performed;

            _inputReader.m_onRightShoulderPerformed += OnRightShoulder_Performed;
            _inputReader.m_onLeftShoulderPerformed += OnLeftShoulder_Performed;
        }

        private void UnregisterEvents()
        {
            _inputReader.m_onMenuPerformed -= OnMenu_Performed;
            _inputReader.m_onCancelPerformed -= OnCancel_Performed;
            _inputReader.m_onNavigatePerformed -= OnNavigate_Performed;
            _inputReader.m_onSubmitPerformed -= OnSubmit_Performed;
            _inputReader.m_onSubmitStarted -= OnSubmit_Started;
            _inputReader.m_onSubmitCanceled -= OnSubmit_Canceled;

            _inputReader.m_onWestButtonPerformed -= OnWestButton_Performed;
            _inputReader.m_onNorthButtonPerformed -= OnNorthButton_Performed;

            _inputReader.m_onRightShoulderPerformed -= OnRightShoulder_Performed;
            _inputReader.m_onLeftShoulderPerformed -= OnLeftShoulder_Performed;
        }

        #endregion


        #region Events

        private void OnMenu_Performed() => UINavigationManager.s_instance.OnMenu_Performed(_p.i_localPlayerID);
        private void OnCancel_Performed()
        {
            if (UINavigationManager.s_instance.GetActiveMenuCount() > 0)
                _feedbacks.PlayMenuBackSfx();
            UINavigationManager.s_instance.OnCancel_Performed(_p.i_localPlayerID);
        }
        private void OnSubmit_Performed()
        {
            if (UINavigationManager.s_instance.GetActiveMenuCount() > 0)
                _feedbacks.PlayMenuSelectSfx();
            UINavigationManager.s_instance.OnSubmit_Performed(_p.i_localPlayerID);
        }

        private void OnSubmit_Started() => UINavigationManager.s_instance.OnSubmit_Started(_p.i_localPlayerID);
        private void OnSubmit_Canceled() => UINavigationManager.s_instance.OnSubmit_Canceled(_p.i_localPlayerID);
        private void OnNavigate_Performed(Vector2 value)
        {
            if (UINavigationManager.s_instance.GetActiveMenuCount() > 0)
                _feedbacks.PlayMenuSwitchSfx();
            UINavigationManager.s_instance.OnNavigate_Performed(_p.i_localPlayerID, value);
        }

        private void OnNorthButton_Performed() => UINavigationManager.s_instance.OnNorthButton_Performed(_p.i_localPlayerID);
        private void OnWestButton_Performed() => UINavigationManager.s_instance.OnWestButton_Performed(_p.i_localPlayerID);

        private void OnRightShoulder_Performed()
        {
            if (UINavigationManager.s_instance.GetActiveMenuCount() > 0)
                _feedbacks.PlayMenuSwitchSfx();
            UINavigationManager.s_instance.OnRightShoulder_Performed(_p.i_localPlayerID);
        }

        private void OnLeftShoulder_Performed()
        {
            if (UINavigationManager.s_instance.GetActiveMenuCount() > 0)
                _feedbacks.PlayMenuSwitchSfx();
            UINavigationManager.s_instance.OnLeftShoulder_Performed(_p.i_localPlayerID);
        }

        #endregion


        #region Private

        private PlayerProperties _p;
        private InputsReader _inputReader;
        private PlayerFeedbacks _feedbacks;

        #endregion
    }
}