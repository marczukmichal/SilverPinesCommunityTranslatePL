using UnityEngine;

[CreateAssetMenu(fileName = "PlayAudioEventEffect", menuName = "Items/Effect/Play Audio")]
public class PlayAudioEventEffect : ItemEffect
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	[SerializeField]
	private PlayAudioEventGameEventChannel m_playAudioEventGameEventChannel;

	public override void ApplyEffect(GameObject character, Inventory inventory)
	{
		Vector3 position = Vector3.zero;
		if (character != null)
		{
			position = character.transform.position;
		}
		m_playAudioEventGameEventChannel.Raise(new PlayAudioEventData(m_audioEvent, position, useOcclusion: false));
	}
}
