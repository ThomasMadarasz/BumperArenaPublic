using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace PlayerController.Data.Runtime
{
    [CreateAssetMenu(fileName = "CollisionsData", menuName = "Data/Controller/PlayerCollisions", order = 1)]
    public class PlayerCollisionsData : ScriptableObject
    {
        public ForceMode m_forceMode;
        public float m_power;
        public float m_powerWithEnvironment;
        public float m_bounceFactor;
        public float m_maxPower;
        [Range(0, 1)]
        public float m_dominantBumpFactor;
        public float m_durationFactor;
        public float m_torqueFactor;

        private void OnEnable()
        {
            if (!Debug.isDebugBuild) return;

#if !UNITY_EDITOR
            if (!ScriptableImporter.CheckPath()) return;
            Debug.Log($"Import Values for {this.name}");
            var dict = ScriptableImporter.FillDictionaryWithScriptableValues(this);
            var data = ScriptableImporter.GetDataFromFile();
            ScriptableImporter.UpdateDictionaryWithFileValues(data, dict);

            FieldInfo[] fields = this.GetType().GetFields();

            foreach (var item in dict[this])
            {
                FieldInfo field = fields.FirstOrDefault(x => x.Name == item.m_fieldName);
                if (field == null)
                {
                    Debug.LogError($"Field {item.m_fieldName} not found");
                    continue;
                }

                var value = ScriptableImporter.ParseValue(field, item.m_fieldValue.ToString());
                field.SetValue(this, value);
            }

#endif
        }
    }
}