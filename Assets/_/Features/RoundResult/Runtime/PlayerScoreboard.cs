using System;
using TMPro;
using UnityEngine;

namespace RoundResult.Runtime
{
    [Serializable]
    public class PlayerScoreboard
    {
        public Animator[] m_animators;
        public TextMeshProUGUI m_playerNumber;
    }
}