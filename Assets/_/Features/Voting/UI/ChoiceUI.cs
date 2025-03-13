using UnityEngine;
using UnityEngine.UI;

namespace Voting.UI.Runtime
{
    public class ChoiceUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Sprite _defaultImage;
        [SerializeField] private Sprite _votedImage;
        [SerializeField] private Image[] _playersPos;

        #endregion


        #region Main

        public void Select(int playerPos, Color playerColor)
        {
            _playersPos[playerPos].color = playerColor;

            _voteCount++;
        }

        public void UnSelect(int playerPos, Color defaultColor)
        {
            _playersPos[playerPos].color = defaultColor;

            _voteCount--;
        }

        public void Lock(int playerPos, Color playerColor) => _playersPos[playerPos].sprite = _votedImage;

        public void Unlock(int playerPos, Color playerColor) => _playersPos[playerPos].sprite = _defaultImage;


        public void ResetVotes(Color defaultColor)
        {
            foreach (var img in _playersPos)
            {
                img.color = defaultColor;
                img.sprite = _defaultImage;
            }

            _voteCount = 0;
        }

        public void SetPlayerCount(int count)
        {
            for (int i = 0; i < _playersPos.Length; i++)
            {
                if (i + 1 > count) { _playersPos[i].gameObject.SetActive(false); }
            }
        }

        public int GetVoteCount() => _voteCount;

        #endregion


        #region Private

        private int _voteCount;

        #endregion

    }
}