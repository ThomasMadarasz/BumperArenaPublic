using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using PlayerController.Runtime;
using Utils.Runtime;
using Interfaces.Runtime;
using AI.Data.Runtime;

namespace PlayerTaskes.Runtime {

    public class SetDirectionToATarget : ActionTask
    {
        #region Exposed

        [BlackboardOnly, RequiredField] public BBParameter<PlayerAIData> m_aiData;
        [BlackboardOnly, RequiredField] public BBParameter<SetDirectionToATargetData> m_tData;
        [BlackboardOnly, RequiredField] public BBParameter<Transform> m_targetTransform;
        [BlackboardOnly, RequiredField] public BBParameter<Transform> m_transform;
        [BlackboardOnly, RequiredField] public BBParameter<PlayerProperties> m_properties;
        [BlackboardOnly, RequiredField] public BBParameter<IBoostable> m_boostable;
        [BlackboardOnly] public BBParameter<float> m_targetMaximumAngle;
        [BlackboardOnly] public BBParameter<float> m_targetMinimumDistance;

        public BBParameter<bool> m_isATargetToBump;
        public BBParameter<bool> m_isAMovingTarget = true;

        [BlackboardOnly] public BBParameter<Transform> m_currentTarget;

        #endregion


        #region NodeCanvasApi

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit() {
            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute() {
            Transform target = m_targetTransform.value;
            m_currentTarget.value = target;
            if (m_targetTransform.value == null) m_properties.value.m_aiInputs = ForwardDirection();
            else
            {
                if (m_isATargetToBump.value && IsBumpable()) Boost();
                m_properties.value.m_aiInputs = TargetDirection(target);
            }
            EndAction(true);
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate() {
            
        }

        //Called when the task is disabled.
        protected override void OnStop() {
            
        }

        //Called when the task is paused.
        protected override void OnPause() {
            
        }

        #endregion


        #region Main

        private void Boost() => m_boostable.value.TryToBoost();

        private bool IsBumpable()
        {
            Transform target = m_targetTransform.value;
            Vector3 anticipatedTargetPos = target.position + (target.forward * m_tData.value.m_forwardAnticipation);
            float distance = Vector3.Distance(m_transform.value.position, anticipatedTargetPos);
            float angleWithOffset = Vector3.Angle(m_transform.value.forward, (anticipatedTargetPos - m_transform.value.position).normalized);
            float angle = Vector3.Angle(m_transform.value.forward, (target.position - m_transform.value.position).normalized);
            return distance <= m_tData.value.m_closeRangeToBoost && angle <= 90 || angleWithOffset <= m_tData.value.m_angleToBoost && distance <= m_tData.value.m_rangeToBoost;
        }

        private Vector2 ForwardDirection() => Vector2.Lerp(m_properties.value.m_aiInputs, m_transform.value.forward.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);

        private Vector2 TargetDirection(Transform target)
        {
            Vector3 targetDirection = target.position - m_transform.value.position;
            if (!m_isAMovingTarget.value && targetDirection.magnitude < m_targetMinimumDistance.value && Vector3.Angle(m_transform.value.forward, targetDirection) > m_targetMaximumAngle.value)
            {
                return Vector2.Lerp(m_properties.value.m_aiInputs, m_transform.value.forward.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);
            }
            return Vector2.Lerp(m_properties.value.m_aiInputs, (target.position - m_transform.value.position).normalized.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);
        }

        #endregion

    }
}