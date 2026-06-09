using UnityEngine;
using UnityEngine.Events;

public class ArmorPiece : MonoBehaviour, IDamageable
{
	[SerializeField]
	private UnityEvent m_onHit;

	[Tooltip("Set this to pass on status effect impacts on hits")]
	[SerializeField]
	private StatusEffectReceiver m_effectReceiver;

	public ConsumeHitType GetConsumeHitType()
	{
		return ConsumeHitType.NotArmorPiercing;
	}

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (!instance.IsDamagingHit())
		{
			m_onHit.Invoke();
		}
		if (m_effectReceiver != null)
		{
			m_effectReceiver.ApplyDamageInstance(instance);
		}
	}

	public bool ShouldDeflectHit(bool isArmorPiercing)
	{
		return !isArmorPiercing;
	}
}
