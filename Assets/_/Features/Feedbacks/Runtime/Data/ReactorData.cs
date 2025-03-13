using UnityEngine;

namespace Feedbacks.Runtime
{
    [CreateAssetMenu(fileName = "NewReactorData", menuName = "Data/Feedback/ReactorData")]
    public class ReactorData : ScriptableObject
    {
        public float m_lifeTime;
        public float m_startSpeed;
        public Vector2 m_startSize;
        public AnimationCurve m_sizeOverLife;
    }
}