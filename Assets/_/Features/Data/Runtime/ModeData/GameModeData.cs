using Enum.Runtime;
using UnityEngine.Video;

namespace Data.Runtime
{
    [System.Serializable]
    public class GameModeData
    {
        public int m_logicIndex;
        public string m_name;
        public MapData[] m_maps;
        public TeamModeEnum m_teamMode;
        public string m_howToPlay;
        public VideoClip m_clip;
    }
}