using UnityEngine;

public class ComputerFolderViewer : MonoBehaviour, IFolderViewer
{
	[SerializeField]
	private Transform m_iconsParent;

	[SerializeField]
	private ComputerFileTypeSettings m_typeSettings;

	private ComputerFolder m_folder;

	public void AttachFolder(ComputerFolder folder)
	{
		m_folder = folder;
		foreach (ComputerFile file in folder.Files)
		{
			CreateFileIcon(file, m_iconsParent);
		}
	}

	public int GetMemoryUsage()
	{
		return m_folder.Files.Count * 64 + m_folder.FolderName.Length * 4;
	}

	private void CreateFileIcon(ComputerFile file, Transform parent)
	{
		ComputerIconButton iconButtonForFile = m_typeSettings.GetIconButtonForFile(file);
		ComputerWindow component = GetComponent<ComputerWindow>();
		ComputerIconButton computerIconButton = Object.Instantiate(iconButtonForFile, parent);
		computerIconButton.AttachDesktop(component.AttachedDesktop);
		computerIconButton.AttachWindow(component);
		computerIconButton.AttachFile(file);
	}
}
