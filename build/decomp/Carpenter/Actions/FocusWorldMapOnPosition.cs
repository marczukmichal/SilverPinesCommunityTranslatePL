using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("World Map")]
public class FocusWorldMapOnPosition : FsmStateAction
{
	public FsmGameObject m_targetObjectPos;

	public FsmVector3 m_targetPosition;

	public float m_speed = 250f;

	protected Map3D m_map3d;

	private Vector3 FocusPosition()
	{
		if (m_targetObjectPos.Value != null)
		{
			return m_targetObjectPos.Value.transform.position;
		}
		return m_targetPosition.Value;
	}

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_map3d = base.Owner.GetComponentInParent<Map3D>();
		}
	}

	public override void OnEnter()
	{
		StartCoroutine(FocusCoroutine());
	}

	private IEnumerator FocusCoroutine()
	{
		yield return m_map3d.FocusCameraOnMapPositionAnimated(FocusPosition(), m_speed);
		Finish();
	}
}
