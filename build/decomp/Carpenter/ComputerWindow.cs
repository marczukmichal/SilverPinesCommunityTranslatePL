using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComputerWindow : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI m_windowNameText;

	[SerializeField]
	private GameObject m_contentParent;

	[SerializeField]
	private bool m_destroyOnClose = true;

	[SerializeField]
	private bool m_standaloneWindow;

	private ComputerDesktop m_computerDesktop;

	private ComputerFile m_file;

	private ComputerFolder m_folder;

	private bool m_isMinimized;

	private bool m_hasMoved;

	public bool DestroyOnClose => m_destroyOnClose;

	public ComputerDesktop AttachedDesktop => m_computerDesktop;

	public ComputerFile File => m_file;

	public ComputerFolder Folder => m_folder;

	public bool HasMoved => m_hasMoved;

	public bool IsMinimized => m_isMinimized;

	private void Start()
	{
		if (m_standaloneWindow)
		{
			AttachDesktop(GetComponentInParent<ComputerDesktop>());
		}
	}

	public void AttachDesktop(ComputerDesktop desktop)
	{
		m_computerDesktop = desktop;
	}

	public void AttachFile(ComputerFile file)
	{
		m_file = file;
		m_windowNameText.text = file.FileName;
		IFileViewer[] componentsInChildren = GetComponentsInChildren<IFileViewer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].AttachFile(file);
		}
	}

	public void AttachFolder(ComputerFolder folder)
	{
		m_folder = folder;
		m_windowNameText.text = folder.FolderName;
		IFolderViewer[] componentsInChildren = GetComponentsInChildren<IFolderViewer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].AttachFolder(folder);
		}
	}

	public void MinimizePressed()
	{
		m_computerDesktop.ToggleMinimized(this);
	}

	public void ClosePressed()
	{
		m_computerDesktop.CloseWindow(this);
	}

	public void ToggleMinimized()
	{
		m_isMinimized = !m_isMinimized;
		m_contentParent.gameObject.SetActive(!m_isMinimized);
	}

	public void StartDrag()
	{
		m_hasMoved = true;
	}

	public void EndDrag()
	{
		m_hasMoved = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Focus();
	}

	public void Focus(bool open = false)
	{
		m_computerDesktop.FocusWindow(this, open);
	}

	public int GetMemoryUsage()
	{
		int num = 96;
		IFileViewer[] componentsInChildren = GetComponentsInChildren<IFileViewer>();
		foreach (IFileViewer fileViewer in componentsInChildren)
		{
			num += fileViewer.GetMemoryUsage();
		}
		IFolderViewer[] componentsInChildren2 = GetComponentsInChildren<IFolderViewer>();
		foreach (IFolderViewer folderViewer in componentsInChildren2)
		{
			num += folderViewer.GetMemoryUsage();
		}
		return num;
	}
}
