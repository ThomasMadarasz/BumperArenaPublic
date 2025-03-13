using NodeCanvas.Framework;
using ParadoxNotion.Design;
using PlayerController.Runtime;
using System.Collections.Generic;
using Traps.Runtime;
using UnityEngine;
using Utils.Runtime;
using AI.Data.Runtime;

namespace PlayerTaskes.Runtime {

	public class SearchForBoost : ActionTask 
	{
		#region Exposed

		[BlackboardOnly, RequiredField] public BBParameter<PlayerAIData> m_aiData;
		[BlackboardOnly, RequiredField] public BBParameter<SearchForBoostData> m_tData;
		[BlackboardOnly, RequiredField] public BBParameter<Transform> m_transform;
		[BlackboardOnly, RequiredField] public BBParameter<PlayerProperties> m_properties;
		[BlackboardOnly, RequiredField] public BBParameter<List<BoostPack>> m_boostPackes;
		[BlackboardOnly, RequiredField] public BBParameter<bool> m_isBoostAvailable;
		[BlackboardOnly, RequiredField] public BBParameter<float> m_targetMaximumAngle;
		[BlackboardOnly, RequiredField] public BBParameter<float> m_targetMinimumDistance;
		public BBParameter<bool> m_isOutOfBoostNotRequired;

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
            if (m_isBoostAvailable.value && !m_isOutOfBoostNotRequired.value)
			{
				EndAction(true);
				return;
			}
			_boostPack = SelectBoostPack();
			if(_boostPack != null)
            {
				m_properties.value.m_aiInputs = TargetDirection(_boostPack.transform);
				EndAction(false);
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

		private BoostPack SelectBoostPack()
        {
			BoostPack sbp = null;
			float sDistance = float.MaxValue;
			foreach(BoostPack bp in m_boostPackes.value)
            {
				float distance = Vector3.Distance(bp.transform.position, m_transform.value.position);
				if(sbp == null ||
					bp.GetRemainingTime() < 0.2f && distance < sDistance ||
					bp.GetRemainingTime() < m_tData.value.m_distanceOverRemainingTimeFactor * distance && distance < sDistance)
                {
					sbp = bp;
					sDistance = distance;
                }
            }
			return sbp;
        }


		private Vector2 TargetDirection(Transform target) 
		{
			Vector3 targetDirection = target.position - m_transform.value.position;
			if (targetDirection.magnitude < m_targetMinimumDistance.value && Vector3.Angle(m_transform.value.forward, targetDirection) > m_targetMaximumAngle.value)
            {
				return Vector2.Lerp(m_properties.value.m_aiInputs, m_transform.value.forward.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);
			}
			return Vector2.Lerp(m_properties.value.m_aiInputs, (target.position - m_transform.value.position).normalized.ToVector2(), m_aiData.value.m_directionInputsStep * Time.deltaTime);
		}

		#endregion


		#region Private

		private BoostPack _boostPack;

        #endregion
    }
}