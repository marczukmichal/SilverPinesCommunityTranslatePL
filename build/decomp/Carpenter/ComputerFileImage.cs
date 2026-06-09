using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/File (Image)")]
public class ComputerFileImage : ComputerFile
{
	[SerializeField]
	private Sprite m_image;

	protected override string Extension => ".img";

	protected override int MemoryUse => m_image.texture.width * m_image.texture.height * 4;

	public Sprite Image => m_image;
}
