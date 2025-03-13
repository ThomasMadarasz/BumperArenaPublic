using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using AI.Runtime;
using Interfaces.Runtime;
using AI.Data.Runtime;

namespace PlayerTaskes.Runtime {

    public class BoostToBump : ActionTask 
    {

        #region Exposed


        [BlackboardOnly, RequiredField] public BBParameter<BoostToBumpData> m_tData;
        [BlackboardOnly, RequiredField] public BBParameter<bool> m_isHittingRaycasts;
        [BlackboardOnly, RequiredField] public BBParameter<List<RaycastLogic>> m_hittingRaycasts;
        [BlackboardOnly, RequiredField] public BBParameter<bool> m_isBoostAvailable;
        [BlackboardOnly, RequiredField] public BBParameter<IBoostable> m_boostable;
        [BlackboardOnly, RequiredField] public BBParameter<ITeamable> m_teamable;

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
        protected override void OnExecute() 
        {
            if (!m_isBoostAvailable.value || !m_isHittingRaycasts.value)
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
            Boost();
            EndAction(false);
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

        private void SortRaycasts()
        {
            _raycasts.Clear();
            foreach (RaycastLogic ray in m_hittingRaycasts.value)
            {
                if (ray.m_priority <= 2 &&
                    m_tData.value.m_playerLayer == (m_tData.value.m_playerLayer | (1 << ray.m_layer)) && 
                    (ray.m_distance <= m_tData.value.m_maxDistance) &&
                    ray.m_hit.collider.GetComponent<ITeamable>().GetTeamID() != m_teamable.value.GetTeamID()) 
                        _raycasts.Add(ray);
            }
        }

        private void Boost() => m_boostable.value.TryToBoost();

        #endregion

        [GetFromAgent] private IBoostable _b;
        [GetFromAgent] private ITeamable _t;

        #region Private

        private List<RaycastLogic> _raycasts = new List<RaycastLogic>();

        #endregion
    }
}