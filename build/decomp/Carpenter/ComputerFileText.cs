using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/File (Text)")]
public class ComputerFileText : ComputerFile
{
	[Multiline(10)]
	[SerializeField]
	private string m_text;

	protected override string Extension => ".txt";

	protected override int MemoryUse => m_text.Length;

	public string Text => m_text;
}
