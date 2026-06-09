using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/File System")]
public class ComputerFileSystem : ScriptableObject
{
	[SerializeField]
	private ComputerRoot m_rootFolder;

	public ComputerRoot RootFolder => m_rootFolder;
}
