using AI.Data.Runtime;
using AI.Runtime;
using Interfaces.Runtime;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerTaskes.Runtime {

	public class BoostForSpeed : ActionTask 
	{
		#region Exposed

		[BlackboardOnly, RequiredField] public BBParameter<BoostForSpeedData> m_tData;
		[BlackboardOnly, RequiredField] public BBParameter<bool> m_isHittingRaycasts;
		[BlackboardOnly, RequiredField] public BBParameter<List<RaycastLogic>> m_hittingRaycasts;
		[BlackboardOnly, RequiredField] public BBParameter<bool> m_isBoostAvailable;
		[BlackboardOnly, RequiredField] public BBParameter<IBoostable> m_boostable;


		[BlackboardOnly] public BBParameter<Transform> m_transform;

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
		protected override void OnExecute()
		{
			EndAction(true);
			if (!m_isBoostAvailable.value || m_currentTarget == null) return; 

			float angle = Vector3.Angle((m_currentTarget.value.position - m_transform.value.position).normalized, m_transform.value.forward);

			if(angle > 10f) return;

			if (!m_isHittingRaycasts.value)
            {
				Boost();
				return;
            }
            else
            {
				SortRaycasts();
				if(_raycasts.Count == 0)
                {
					Boost();
					return;
                }
            }
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
					m_tData.value.m_obstaclesLayer == (m_tData.value.m_obstaclesLayer | (1 << ray.m_layer)) &&
					(ray.m_distance <= m_tData.value.m_maxDistance))
					_raycasts.Add(ray);
			}
		}

		private void Boost() => m_boostable.value.TryToBoost();


		#endregion


		#region Private

		private List<RaycastLogic> _raycasts = new List<RaycastLogic>();

		#endregion
	}
}