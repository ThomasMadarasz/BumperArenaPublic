using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName ="NewSFXData",menuName ="Audio/SFXData")]
    public class SFXData : ScriptableObject
    {
        public SFXProperties[] _sfx;
    }
}