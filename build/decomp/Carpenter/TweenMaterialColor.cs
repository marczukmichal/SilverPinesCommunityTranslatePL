using DG.Tweening;
using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Tween)]
public class TweenMaterialColor : BaseCarpenterTween
{
	public Color m_targetColor;

	public string m_propertyID;

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		if (ownerDefaultTarget != null)
		{
			MeshRenderer component = ownerDefaultTarget.GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.material.DOColor(m_targetColor, m_propertyID, m_duration).SetDelay(m_delay).SetEase(m_curve.curve)
					.SetUpdate(isIndependentUpdate: false)
					.OnComplete(base.OnTweenComplete);
				return;
			}
			Debug.LogError("TweenMaterialColor has no mesh renderer component for " + base.State.Name + " - " + base.Owner.gameObject);
			Finish();
		}
		else
		{
			Debug.LogError("TweenMaterialColor has no gameobject for " + base.State.Name + " - " + base.Owner.gameObject);
			Finish();
		}
	}
}
