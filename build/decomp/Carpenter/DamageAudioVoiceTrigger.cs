using UnityEngine;

public class DamageAudioVoiceTrigger : MonoBehaviour, IDamageable
{
	public enum TriggerType
	{
		NoDamageMelee
	}

	[SerializeField]
	private TriggerType m_triggerType;

	[SerializeField]
	private AudioVoicedEvent m_voicedEvent;

	private bool m_triggered;

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (!m_triggered && GameUtils.IsPlayer(instance.DamageSource))
		{
			instance.DamageSource.GetComponent<CharacterIdentifier>();
			if (m_triggerType == TriggerType.NoDamageMelee && instance.HealthDamageAmount == 0 && instance.DamageCategory == DamageCategory.DamageCollider)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_voicedEvent, instance.DamageSource.transform, isPlayer: true));
				m_triggered = true;
			}
		}
	}
}
