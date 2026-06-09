using UnityEngine;
using UnityEngine.Events;

public class PursuerSpawnTrigger : MonoBehaviour
{
	[SerializeField]
	private float m_triggerRadius = 1f;

	[SerializeField]
	private float m_chanceToTrigger = 0.1f;

	[SerializeField]
	private bool m_ignoreTimeBetweenPursuits;

	[SerializeField]
	private PursuerSpawnPosition m_forceSpawnPosition;

	[SerializeField]
	private UnityEvent m_onSpawnEvent;

	public bool IgnoreTimeBetweenPursuits => m_ignoreTimeBetweenPursuits;

	public PursuerSpawnPosition ForceSpawnPosition => m_forceSpawnPosition;

	public float ChanceToTrigger => m_chanceToTrigger;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, m_triggerRadius);
		Gizmos.color = Color.white;
	}

	private void Update()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			Vector2 a = item.transform.position;
			Vector2 b = base.transform.position;
			if (Vector2.Distance(a, b) < m_triggerRadius)
			{
				Trigger();
			}
		}
	}

	private void Trigger()
	{
		GlobalReferences.Instance.EventChannels.Gameplay.PursuerSpawnTrigger.Raise(this);
		base.enabled = false;
	}

	public void ForceTrigger()
	{
		float chanceToTrigger = m_chanceToTrigger;
		m_chanceToTrigger = 1f;
		Trigger();
		m_chanceToTrigger = chanceToTrigger;
	}

	public void OnTriggered()
	{
		m_onSpawnEvent.Invoke();
	}
}
