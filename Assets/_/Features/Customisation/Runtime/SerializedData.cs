namespace Customisation.Runtime
{
    [System.Serializable]
    public class SerializedData
    {
        public SerializedCustomisationData[] m_data;

        public void FillArrayWithDefaultConfig(SerializedCustomisationData defaultConfig)
        {
            m_data = new SerializedCustomisationData[4];

            for (int i = 0; i < 4; i++)
            {
                m_data[i] = (SerializedCustomisationData)defaultConfig.Clone();
            }
        }
    }
}