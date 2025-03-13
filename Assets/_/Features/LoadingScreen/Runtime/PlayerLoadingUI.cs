using TMPro;
using UnityEngine;

namespace LoadingScreen.Runtime
{
    public class PlayerLoadingUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _playerNameTxt;
        [SerializeField] private TextMeshProUGUI _playerIndexTxt;

        #endregion


        #region Main

        public void UpdatePlayerName(string playerName) => _playerNameTxt.text = playerName;
        public void UpdatePlayerIndex(int playerIndex) => _playerIndexTxt.text = $"P {playerIndex + 1}";

        #endregion
    }
}