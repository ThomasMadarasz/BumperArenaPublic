using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.Runtime
{
    public class InformationModal : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Button _btn;
        [SerializeField] private TextMeshProUGUI _txt;

        #endregion


        #region Main

        public void Open(string message)
        {
            _txt.text = message;
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _txt.text = string.Empty;
        }

        #endregion
    }
}