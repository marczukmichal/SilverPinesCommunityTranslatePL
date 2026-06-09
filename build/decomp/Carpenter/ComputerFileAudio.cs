using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/File (Audio)")]
public class ComputerFileAudio : ComputerFile
{
	[SerializeField]
	private AudioClip m_audioClip;

	protected override string Extension => ".snd";

	protected override int MemoryUse => m_audioClip.samples * 8;

	public AudioClip Audio => m_audioClip;
}
