namespace Core.UI.Runtime
{
    public class ScoreDataUI
    {
        public int m_playerID;
        public int m_scoreValue;

        public ScoreDataUI()
        { }

        public ScoreDataUI(int playerID, int scoreDalue)
        {
            m_playerID = playerID;
            m_scoreValue = scoreDalue;
        }
    }
}