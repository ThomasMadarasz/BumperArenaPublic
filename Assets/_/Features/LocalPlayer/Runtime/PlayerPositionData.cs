using UnityEngine;

namespace LocalPlayer.Runtime
{
    [System.Serializable]
    public class PlayerPositionData
    {
        public int m_playerCount;
        public Transform[] m_positions;
        public GameObject m_go;
    }
}