using Feedbacks.Runtime;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Feedbacks.Editor
{
    [CustomEditor(typeof(PlayerFeedbacks))]
    public class PlayerFeedbacksEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}