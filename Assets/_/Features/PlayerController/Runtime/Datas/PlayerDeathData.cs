using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace PlayerController.Data.Runtime
{
    [CreateAssetMenu(fileName = "DeathData", menuName = "Data/Controller/PlayerDeath", order = 3)]
    public class PlayerDeathData : ScriptableObject
    {
        [Min(0)]
        public float m_invicibilityTimeOnRespawn;

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