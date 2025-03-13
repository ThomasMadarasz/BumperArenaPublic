using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.Runtime;

namespace AI.Data.Runtime
{
    [CreateAssetMenu(fileName = "AIData", menuName = "Data/AI", order = 1)]
    public class PlayerAIData : ScriptableObject
    {
        [Tooltip("Modifie la vitesse � laquelle l'input souhait� par l'IA va �tre atteint. Plus ce sera haut, plus l'IA aura des r�flexes hors normes. Plus ce sera bas, plus l'IA aura deux de tension. DEFAULT VALUE = 5")]
        public float m_directionInputsStep = 5;
        [Tooltip("Facteur qui permet au bot d'anticiper la trajectoire des joueurs en projetant leur position un peu plus vers l'avant. Augmenter projette la position plus loin. Attention cela n'anticipe pas la vraie trajectoire juste la position devant. DEFAULT VALUE = 1.5")]
        public float m_anticipationOffset = 1.5f;
        [Tooltip("L'angle maximal qui permet � l'IA de consid�rer le joueur comme une cible potentiel. Augmenter de trop peut affaiblir la pr�cision des boosts. DEFAULT VALUE = 90")]
        public float m_maxAngleToConsiderAPlayerAsAPotentialTarget = 90;

        public int m_raycastNumber;
        public float m_raycastMaxAngle;
        public float m_raycastBaseAngleOffset;
        public float m_longestRaycastDistance;
        public float m_shortestRaycastDistance;
        public LayerMask m_layerMask;

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