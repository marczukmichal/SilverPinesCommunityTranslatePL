using UnityEngine;

public class CharacterDamageRumble : MonoBehaviour, IDamageable
{
	[SerializeField]
	private ControllerRumbleSettings m_hitRumble;

	public void ApplyDamageInstance(DamageInstance instance)
	{
		if (instance.IsDamagingHit())
		{
			GlobalReferences.Instance.EventChannels.Generic.ControllerRumble.Raise(new ControllerRumbleEventData
			{
				m_controllerRumbleSettings = m_hitRumble,
				m_position = instance.Position
			});
		}
	}
}
