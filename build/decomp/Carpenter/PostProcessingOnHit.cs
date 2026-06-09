using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingOnHit : MonoBehaviour
{
	[SerializeField]
	private FloatVariable m_playerHealthPercentage;

	[SerializeField]
	private AnimationCurve m_healthChangeCurve;

	private float m_currentHealth;

	private Volume m_volume;

	private Sequence m_activeSequence;

	private void Awake()
	{
		m_volume = GetComponent<Volume>();
		m_volume.enabled = false;
	}

	private void OnDisable()
	{
		if (m_activeSequence != null)
		{
			DOTween.Kill(m_activeSequence);
		}
	}

	public void Update()
	{
		if (m_playerHealthPercentage.Value < m_currentHealth)
		{
			float time = m_currentHealth - m_playerHealthPercentage.Value;
			float num = m_healthChangeCurve.Evaluate(time);
			if (m_activeSequence != null)
			{
				m_activeSequence.Kill();
				m_activeSequence = null;
			}
			m_activeSequence = DOTween.Sequence();
			m_activeSequence.AppendCallback(delegate
			{
				m_volume.enabled = true;
			});
			m_activeSequence.Append(DOTween.To(() => m_volume.weight, delegate(float x)
			{
				m_volume.weight = x;
			}, 1f * num, 0.125f));
			m_activeSequence.AppendInterval(0.5f * num);
			m_activeSequence.Append(DOTween.To(() => m_volume.weight, delegate(float x)
			{
				m_volume.weight = x;
			}, 0f, 0.25f));
			m_activeSequence.AppendCallback(delegate
			{
				m_volume.enabled = false;
			});
		}
		m_currentHealth = m_playerHealthPercentage.Value;
	}
}
