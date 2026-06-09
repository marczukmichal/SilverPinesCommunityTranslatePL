using UnityEngine;
using VLB;

public class EnvironmentEnemyLightFlicker : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_enemyPresence;

	[SerializeField]
	private float m_enemyPresenceThreshold = 0.4f;

	private EffectFlicker m_flickerEffect;

	private void Start()
	{
		m_flickerEffect = GetComponent<EffectFlicker>();
	}

	public void Update()
	{
		m_flickerEffect.enabled = m_enemyPresence.Value > m_enemyPresenceThreshold;
	}
}
