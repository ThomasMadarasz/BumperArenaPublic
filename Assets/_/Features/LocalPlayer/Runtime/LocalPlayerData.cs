namespace LocalPlayer.Runtime
{
    public class LocalPlayerData
    {
        public int m_localPlayerID;
        public int m_deviceID;
        public bool m_isDefaultPlayer;

        public bool m_useRumble;
        public bool m_isWorldOrientation;

        public LocalPlayerData(int playerID, int deviceID, bool isDefaultPlayer, bool useRumble, bool isWorldOrientation)
        {
            m_localPlayerID = playerID;
            m_deviceID = deviceID;
            m_isDefaultPlayer = isDefaultPlayer;
            m_useRumble = useRumble;
            m_isWorldOrientation = isWorldOrientation;
        }
    }
}