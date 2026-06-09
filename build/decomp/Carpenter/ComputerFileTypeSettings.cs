using UnityEngine;

[CreateAssetMenu(menuName = "Minigames/Computer Desktop/File Type Settings")]
public class ComputerFileTypeSettings : ScriptableObject
{
	[SerializeField]
	private ComputerWindow m_textViewerWindow;

	[SerializeField]
	private ComputerIconButton m_textIconButton;

	[SerializeField]
	private ComputerWindow m_imageViewerWindow;

	[SerializeField]
	private ComputerIconButton m_imageIconButton;

	[SerializeField]
	private ComputerWindow m_audioPlayerWindow;

	[SerializeField]
	private ComputerIconButton m_audioIconButton;

	[SerializeField]
	private ComputerWindow m_folderViewerWindow;

	[SerializeField]
	private ComputerIconButton m_folderIconButton;

	public ComputerWindow TextViewerWindow => m_textViewerWindow;

	public ComputerIconButton TextIconButton => m_textIconButton;

	public ComputerWindow ImageViewerWindow => m_imageViewerWindow;

	public ComputerIconButton ImageIconButton => m_imageIconButton;

	public ComputerWindow AudioPlayerWindow => m_audioPlayerWindow;

	public ComputerIconButton AudioIconButton => m_audioIconButton;

	public ComputerWindow FolderViewerWindow => m_folderViewerWindow;

	public ComputerIconButton FolderIconButton => m_folderIconButton;

	public ComputerIconButton GetIconButtonForFile(ComputerFile file)
	{
		if (file is ComputerFileText)
		{
			return TextIconButton;
		}
		if (file is ComputerFileImage)
		{
			return ImageIconButton;
		}
		if (file is ComputerFileAudio)
		{
			return AudioIconButton;
		}
		return null;
	}

	public ComputerWindow GetWindowForFile(ComputerFile file)
	{
		if (file is ComputerFileText)
		{
			return TextViewerWindow;
		}
		if (file is ComputerFileImage)
		{
			return ImageViewerWindow;
		}
		if (file is ComputerFileAudio)
		{
			return AudioPlayerWindow;
		}
		return null;
	}
}
