using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Footstep Effect Area")]
public class FootstepEffectAreaSettings : ScriptableObject
{
	[SerializeField]
	private AudioEvent m_audioEvent;

	public AudioEvent AudioEvent => m_audioEvent;
}
