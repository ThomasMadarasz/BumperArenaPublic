using Customisation.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.UI;

namespace UserInterface.Runtime
{
    public class GarageMenuUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameObject _selectionUI;

        [SerializeField] private GameObject _carUI;
        [SerializeField] private GameObject _characterUI;
        [SerializeField] private GameObject _animationUI;

        [SerializeField] private GameObject _firstSelectedGo;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnEnable() => DisplaySelectionMenu();

        #endregion


        #region Main

        private void Setup()
        {
            _charactersFeedbacks = _characterUI.GetComponent<MMF_Player>();
            _carsFeedbacks = _carUI.GetComponent<MMF_Player>();
            _animsFeedbacks = _animationUI.GetComponent<MMF_Player>();
        }

        public void DisplaySelectionMenu()
        {
            CustomisationManager.s_instance.ResetPlayerPreviews();

            ShowSelectionUI();

            EventSystem.current.sendNavigationEvents = false;

            EventSystem.current.SetSelectedGameObject(null);

            Invoke(nameof(EnableNavigationEvent), 0.1f);
            Invoke(nameof(SetSelectedUI), 0.11f);
        }

        private void SetSelectedUI()
        {
            EventSystem.current.SetSelectedGameObject(_firstSelectedGo);
        }

        private void EnableNavigationEvent()
        {
            EventSystem.current.sendNavigationEvents = true;
        }

        public void ToggleCarUI()
        {
            HideSelectionUI();
            _carUI.SetActive(!_carUI.activeInHierarchy);
        }

        public void ToggleCharacterUI()
        {
            HideSelectionUI();
            _characterUI.SetActive(!_characterUI.activeInHierarchy);
        }

        public void ToggleAnimationUI()
        {
            HideSelectionUI();
            _animationUI.SetActive(!_animationUI.activeInHierarchy);
        }

        private void ShowSelectionUI()
        {
            _carUI.SetActive(false);
            _characterUI.SetActive(false);
            _animationUI.SetActive(false);

            _selectionUI.SetActive(true);
        }

        #endregion


        #region Utils & Tools

        private void HideSelectionUI() => _selectionUI.SetActive(false);

        private MMF_Player _charactersFeedbacks;
        private MMF_Player _carsFeedbacks;
        private MMF_Player _animsFeedbacks;

        #endregion
    }
}