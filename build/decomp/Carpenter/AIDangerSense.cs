using UnityEngine;

public class AIDangerSense : MonoBehaviour, IAISense, IDamageable
{
	[SerializeField]
	private float m_timeout = 5f;

	private Detectable m_mostRecentDamageSourceDetectable;

	private float m_timer;

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (!(instance.DamageSource == null))
		{
			Detectable component = instance.DamageSource.GetComponent<Detectable>();
			if ((object)component != null)
			{
				m_mostRecentDamageSourceDetectable = component;
				m_timer = m_timeout;
			}
		}
	}

	private void Update()
	{
		if (m_timer > 0f)
		{
			m_timer -= Time.deltaTime;
			if (m_timer < 0f)
			{
				m_mostRecentDamageSourceDetectable = null;
			}
		}
	}

	public void PopulateTargets(AISenses.DetectableResults detectableResults, bool onlyDangerSense)
	{
		if (m_mostRecentDamageSourceDetectable != null)
		{
			detectableResults.AddDetectable(m_mostRecentDamageSourceDetectable, 10f);
		}
	}
}
