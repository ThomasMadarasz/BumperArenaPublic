using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "CustomisationData", menuName = "Data/Customisation")]
    public class CustomisationData : ScriptableObject
    {
        public CustomisationInfo[] m_cars;
        public GameObject[] m_carsReactors;
        public CustomisationInfo[] m_characters;
        public CustomisationInfo[] m_animations;
        public Material[] m_materials;
        public Material[] m_emissiveMaterials;
        public Material[] m_scoreboardMaterials;
        public Material[] m_teamModeDuoMaterials;
        public Material[] m_teamModeDuoEmissiveMaterials;
        public Material m_AIMaterial;
    }

    [System.Serializable]
    public class CustomisationInfo
    {
        public CustomisationType m_type;

        [HideIf(nameof(m_type), CustomisationType.Animation)] public GameObject m_lowPoly;
        [HideIf(nameof(m_type), CustomisationType.Animation)] public GameObject m_highPoly;
        [HideIf(nameof(m_type), CustomisationType.Animation)] public Sprite m_sprite;

        [ShowIf(nameof(m_type), CustomisationType.Animation)] public string m_displayName;
        [ShowIf(nameof(m_type), CustomisationType.Animation)] public AnimationClip m_nameMenu;
        [ShowIf(nameof(m_type), CustomisationType.Animation)] public AnimationClip m_nameInGame;
        
    }

    [System.Serializable]
    public enum CustomisationType
    {
        Unknown = 0,
        Car = 1,
        Character = 2,
        Animation = 3
    }
}