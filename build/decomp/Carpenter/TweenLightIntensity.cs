using DG.Tweening;
using HutongGames.PlayMaker;
using UnityEngine;

[ActionCategory(ActionCategory.Tween)]
public class TweenLightIntensity : BaseCarpenterTween
{
	public float m_targetLightIntensity;

	public override void OnEnter()
	{
		base.OnEnter();
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		if (ownerDefaultTarget != null)
		{
			Light component = ownerDefaultTarget.GetComponent<Light>();
			if (component != null)
			{
				component.DOIntensity(m_targetLightIntensity, m_duration).SetDelay(m_delay).SetEase(m_curve.curve)
					.SetUpdate(isIndependentUpdate: false)
					.OnComplete(base.OnTweenComplete);
				return;
			}
			Debug.LogError("TweenLightIntensity has no light component for " + base.State.Name + " - " + base.Owner.gameObject);
			Finish();
		}
		else
		{
			Debug.LogError("TweenLightIntensity has no gameobject for " + base.State.Name + " - " + base.Owner.gameObject);
			Finish();
		}
	}
}
