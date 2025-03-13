using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "UISkinData", menuName = "Data/UISkinData")]
    public class UISkinData : ScriptableObject
    {
        [BoxGroup("Selected")] public ColorBlock m_selectedColorBlock;
        [BoxGroup("Default")] public ColorBlock m_normalColorBlock;
    }
}