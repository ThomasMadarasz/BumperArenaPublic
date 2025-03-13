using System;

namespace Customisation.Runtime
{
    [System.Serializable]
    public class SerializedCustomisationData : ICloneable
    {
        public int m_carIndex;
        public int m_characterIndex;
        public int m_animationIndex;
        public int m_materialIndex;

        public object Clone()
        {
            SerializedCustomisationData clone = new SerializedCustomisationData()
            {
                m_carIndex = this.m_carIndex,
                m_characterIndex = this.m_characterIndex,
                m_animationIndex = this.m_animationIndex,
                m_materialIndex = this.m_materialIndex
            };

            return clone;
        }
    }
}