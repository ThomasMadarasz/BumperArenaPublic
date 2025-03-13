using UnityEngine;

namespace GameModes.Runtime
{

    [CreateAssetMenu(fileName = "PuckData", menuName = "Data/GameModes/AirHockey/Puck", order = 2)]
    public class PuckData : ScriptableObject
    {
        public float m_mass = 0.1f;
        [Range(0f,1f)]
        public float m_dynamicFriction = 0.1f;
        [Range(0f, 1f)]
        public float m_staticFriction = 0f;
        [Range(0f, 1f)]
        public float m_bounciness = 0.5f;
        public PhysicMaterialCombine m_frictionCombine = PhysicMaterialCombine.Minimum;
        public PhysicMaterialCombine m_bounceCombine = PhysicMaterialCombine.Maximum;
    }
}