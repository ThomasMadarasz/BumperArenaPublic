using AI.Data.Runtime;
using AI.Runtime;
using Interfaces.Runtime;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using PlayerController.Runtime;
using System.Collections.Generic;
using UnityEngine;
using Utils.Runtime;

namespace PlayerTaskes.Runtime {

    public class AvoidPlayer : ActionTask {

        #region Exposed

        [BlackboardOnly, RequiredField] public BBParameter<PlayerAIData> m_aiData;
        [BlackboardOnly, RequiredField] public BBParameter<AvoidPlayerData> m_tData;
        [BlackboardOnly, RequiredField] public BBParameter<Transform> m_transform;
        [BlackboardOnly, RequiredField] public BBParameter<PlayerProperties> m_properties;
        [BlackboardOnly, RequiredField] public BBParameter<ITeamable> m_teamable;
        [BlackboardOnly, RequiredField] public BBParameter<bool> m_isHittingRaycasts;
        [BlackboardOnly, RequiredField] public BBParameter<List<RaycastLogic>> m_hittingRaycasts;

        #endregion


        #region NodeCanvasApi

        //Use for initialization. This is called only once in the lifetime of the task.
        //Return null if init was successfull. Return an error string otherwise
        protected override string OnInit()
        {
            return null;
        }

        //This is called once each time the task is enabled.
        //Call EndAction() to mark the action as finished, either in success or failure.
        //EndAction can be called from anywhere.
        protected override void OnExecute() {
            if (!m_isHittingRaycasts.value)
            {
                EndAction(true);
                return;
            }
            SortRaycasts();
            if (_raycasts.Count <= 0)
            {
                EndAction(true);
                return;
            }
            _avoidTimer = new(m_tData.value.m_avoidDuration, SetActionFalse);
            _avoidTimer.Start();
        }

        //Called once per frame while the action is active.
        protected override void OnUpdate() {

            if (_raycasts.Count == 0) return;
            m_properties.value.m_aiInputs = AvoidDirection();
        }

        //Called when the task is disabled.
        protected override void OnStop() {
            
        }

        //Called when the task is paused.
        protected override void OnPause() {
            
        }

        #endregion


        #region Main

        private void SortRaycasts()
        {
            _raycasts.Clear();
            foreach (RaycastLogic ray in m_hittingRaycasts.value)
            {
                if (ray.m_distance <= (m_tData.value.m_maxDistance * Mathf.Cos(ray.m_angle * Mathf.Deg2Rad) + 1) &&
                    m_tData.value.m_playerLayer == (m_tData.value.m_playerLayer | (1 << ray.m_layer)) &&
                    ray.m_hit.collider.GetComponent<ITeamable>().GetTeamID() != m_teamable.value.GetTeamID()) 
                    _raycasts.Add(ray);
            }
        }
        private Vector2 AvoidDirection()
        {
            RaycastLogic rl = null;
            float priority = float.MaxValue;

            foreach (RaycastLogic r in _raycasts)
            {
                float rPriority = r.m_distance / m_aiData.value.m_longestRaycastDistance * m_tData.value.m_distanceFactorOnPriority + r.m_priority / (m_aiData.value.m_raycastNumber / 2 - 1) * m_tData.value.m_priorityFactorOnPriority;
                if (rPriority <= priority)
                {
                    priority = rPriority;
                    rl = r;
                }
            }
            Vector3 direction = m_transform.value.forward;
            if (rl.m_hit.collider == null) return direction;
            if (rl.m_hit.collider.attachedRigidbody == null) return direction;
            float angle = Vector3.SignedAngle(-m_transform.value.forward, rl.m_hit.collider.attachedRigidbody.velocity.normalized, Vector3.up);
            float value = angle - rl.m_angle;
            direction =  value >= 0 && value <= 90f || value < -90f ? m_transform.value.right : -m_transform.value.right;
            return Vector2.Lerp(m_properties.value.m_aiInputs, direction.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);
        }

        private void SetActionFalse()
        {
            EndAction(false);
        }

        #endregion


        #region Private

        private List<RaycastLogic> _raycasts = new List<RaycastLogic>();

        private Timer _avoidTimer;

        #endregion
    }
}