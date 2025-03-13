using Archi.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Customisation.Runtime.UI
{
    [RequireComponent(typeof(CustomisationSelection))]
    public class SelectionUI : CBehaviour//, IPointerEnterHandler
    {
        #region Exposed

        [SerializeField] private CustomisationSelection _selection;

        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private Image[] _localPlayerImages;

        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Sprite _lockSprite;

        #endregion


        #region Unity API

        private void Start() => UpdateUI();

        #endregion


        #region Utils & Tools

        private void UpdateUI()
        {
            if (_image != null) _image.sprite = _selection.LoadSprite();
            if (_text != null) _text.text = _selection.LoadText();
        }

        //public void OnPointerEnter(PointerEventData eventData)
        //{
        //    _selection.Select();
        //}

        public void Select(int playerIndex, Color playerColor)
        {
            _localPlayerImages[playerIndex].sprite = _defaultSprite;
            _localPlayerImages[playerIndex].color = playerColor;
            _localPlayerImages[playerIndex].gameObject.SetActive(true);
            _selection.Select(playerIndex + 1);
        }

        public void UnSelect(int playerIndex)
        {
            _localPlayerImages[playerIndex].gameObject.SetActive(false);
        }

        public void Lock(int playerIndex)
        {
            _localPlayerImages[playerIndex].sprite = _lockSprite;
        }
        public void Unlock(int playerIndex)
        {
            _localPlayerImages[playerIndex].sprite = _defaultSprite;
        }

        #endregion
    }
}