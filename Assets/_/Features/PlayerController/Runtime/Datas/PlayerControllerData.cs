using Sirenix.OdinInspector;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace PlayerController.Data.Runtime
{
    [CreateAssetMenu(fileName = "ControllerData", menuName = "Data/Controller/PlayerController", order = 0)]
    public class PlayerControllerData : ScriptableObject
    {
        [BoxGroup("No Boost Controller")]
        public PlayerSpeedData m_speedData;

        [BoxGroup("Boost Controller")]
        public PlayerSpeedData m_boostSpeedData;

        [BoxGroup("Other values")]
        public Vector3 m_ForceApplicationPoint;
        [BoxGroup("Other values")]
        public float m_unlockedDuration = 0.5f;
        [BoxGroup("Other values")]
        public float m_torqueDeceleration;

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

    [System.Serializable]
    public class PlayerSpeedData
    {
        public ForceMode m_forceMode;
        public float m_maxSpeed;
        public float m_minSpeed;
        public float m_maxSpeedWithExternalForce;
        public float m_acceleration;
        public float m_handling;
        public float m_maxTurnAngle;
    }
}