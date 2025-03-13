using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace PlayerController.Data.Runtime
{
    [CreateAssetMenu(fileName = "BoostData", menuName = "Data/Controller/PlayerBoost", order = 2)]
    public class PlayerBoostData : ScriptableObject
    {
        public float m_duration;
        public float m_cooldown;
        public float m_naturalRecoveryCooldownInSeconds;
        public float m_naturalRecoveryInPercent;
        public float m_maxValue;
        public int m_maxBoostsNumber;
        public float m_defaultValue;

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