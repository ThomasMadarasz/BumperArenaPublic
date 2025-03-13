using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace AI.Data.Runtime
{
    [CreateAssetMenu(fileName = "BoostToBumpData", menuName = "Data/AI/Taskes/BoostToBumpData", order = 3)]
    public class BoostToBumpData : ScriptableObject
    {
        public float m_maxDistance;
        public LayerMask m_playerLayer;

        private void OnEnable()
        {
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