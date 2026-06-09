using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Camera)]
public class TriggerCameraShake : FsmStateAction
{
	[RequiredField]
	[HutongGames.PlayMaker.Tooltip("The Game Object to position.")]
	public FsmOwnerDefault m_gameObject;

	public CameraShakeSettings m_settings;

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		CameraShakeEventData cameraShakeEventData = new CameraShakeEventData();
		cameraShakeEventData.m_cameraShakeSettings = m_settings;
		cameraShakeEventData.m_position = ownerDefaultTarget.transform.position;
		cameraShakeEventData.m_direction = Random.insideUnitCircle.normalized;
		GlobalReferences.Instance.EventChannels.Generic.CameraShake.Raise(cameraShakeEventData);
	}
}
