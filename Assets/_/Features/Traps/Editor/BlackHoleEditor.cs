using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Traps.Runtime;

namespace Traps.Editor
{
    [CustomEditor(typeof(BlackHole))]
    public class BlackHoleEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}