using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ComputerDesktop : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IMinigameBackInputHandler
{
	[SerializeField]
	private ComputerFileSystem m_fileSystem;

	[SerializeField]
	private RectTransform m_iconsParent;

	[SerializeField]
	private RectTransform m_windowsParent;

	[SerializeField]
	private ComputerFileTypeSettings m_typeSettings;

	[SerializeField]
	private AudioSource m_clickAudio;

	[SerializeField]
	private AudioSource m_loadingAudio;

	[SerializeField]
	private AudioSource m_ambientAudio;

	[SerializeField]
	private AudioSource m_turnOffAudio;

	[SerializeField]
	private int m_processPerTick = 10;

	[SerializeField]
	private float m_tickTime = 0.1f;

	[SerializeField]
	private GameObject m_shutdown;

	[SerializeField]
	private UnityEvent m_onShutdownEvent;

	[SerializeField]
	private GameObject m_loadingBar;

	[SerializeField]
	private Camera m_uiCamera;

	private ComputerIconButton m_selectedIcon;

	private List<ComputerWindow> m_openWindows;

	private int m_processCount;

	private bool m_isShuttingDown;

	public RectTransform WindowsParent => m_windowsParent;

	public float TickTime => m_tickTime;

	public Camera UICamera => m_uiCamera;

	public bool IsBusy => m_processCount > 0;

	public int GetAvailableTicks()
	{
		return Mathf.Max(m_processPerTick / m_processCount, 1);
	}

	public void Select(ComputerIconButton icon)
	{
		if (m_selectedIcon != null)
		{
			m_selectedIcon.SetSelected(selected: false);
		}
		m_selectedIcon = icon;
		if (icon != null)
		{
			icon.SetSelected(selected: true);
		}
		PlayClickAudio();
	}

	private void PlayClickAudio()
	{
		m_clickAudio.Play();
	}

	private void Start()
	{
		m_openWindows = new List<ComputerWindow>();
		PopulateDesktop();
	}

	private void PopulateDesktop()
	{
		foreach (ComputerFolder subFolder in m_fileSystem.RootFolder.SubFolders)
		{
			CreateFolderIcon(subFolder, m_iconsParent);
		}
		foreach (ComputerFile file in m_fileSystem.RootFolder.Files)
		{
			CreateFileIcon(file, m_iconsParent);
		}
	}

	private void CreateFolderIcon(ComputerFolder folder, Transform parent)
	{
		ComputerIconButton computerIconButton = Object.Instantiate(m_typeSettings.FolderIconButton, parent);
		computerIconButton.AttachDesktop(this);
		computerIconButton.AttachFolder(folder);
	}

	private void CreateFileIcon(ComputerFile file, Transform parent)
	{
		ComputerIconButton computerIconButton = Object.Instantiate(m_typeSettings.GetIconButtonForFile(file), parent);
		computerIconButton.AttachDesktop(this);
		computerIconButton.AttachFile(file);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Select(null);
	}

	public void OpenFile(ComputerFile file)
	{
		PlayClickAudio();
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			if (openWindow.File == file)
			{
				FocusWindow(openWindow, open: true);
				return;
			}
		}
		ComputerWindow window = Object.Instantiate(m_typeSettings.GetWindowForFile(file), m_windowsParent);
		StartCoroutine(OpenWindowCoroutine(window, file));
	}

	public void OpenSpecificWindow(ComputerWindow window)
	{
		PlayClickAudio();
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			if (openWindow == window)
			{
				FocusWindow(openWindow, open: true);
				return;
			}
		}
		StartCoroutine(OpenWindowCoroutine(window, null));
	}

	private IEnumerator OpenWindowCoroutine(ComputerWindow window, ComputerFile file)
	{
		AddProcess();
		window.AttachDesktop(this);
		if (file != null)
		{
			window.AttachFile(file);
		}
		window.GetComponent<RectTransform>().anchoredPosition = GetWindowPosition(m_openWindows.Count);
		m_openWindows.Add(window);
		window.gameObject.SetActive(value: false);
		window.transform.SetAsLastSibling();
		int requiredProcessing = 50;
		while (requiredProcessing > 0)
		{
			yield return new WaitForSecondsRealtime(m_tickTime);
			int availableTicks = GetAvailableTicks();
			requiredProcessing -= availableTicks;
		}
		window.gameObject.SetActive(value: true);
		RemoveProcess();
	}

	private Vector2 GetWindowPosition(int index)
	{
		Vector2 zero = Vector2.zero;
		zero.x += 32f * (float)index;
		zero.y -= 32f * (float)index;
		return zero;
	}

	public void OpenFolder(ComputerFolder folder)
	{
		PlayClickAudio();
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			if (openWindow.Folder == folder)
			{
				FocusWindow(openWindow, open: true);
				return;
			}
		}
		StartCoroutine(OpenFolderCoroutine(folder));
	}

	private IEnumerator OpenFolderCoroutine(ComputerFolder folder)
	{
		AddProcess();
		ComputerWindow folderViewerWindow = m_typeSettings.FolderViewerWindow;
		ComputerWindow newWindow = Object.Instantiate(folderViewerWindow, m_windowsParent);
		newWindow.AttachDesktop(this);
		newWindow.AttachFolder(folder);
		newWindow.GetComponent<RectTransform>().anchoredPosition = GetWindowPosition(m_openWindows.Count);
		m_openWindows.Add(newWindow);
		newWindow.gameObject.SetActive(value: false);
		int requiredProcessing = 50 * folder.Files.Count;
		while (requiredProcessing > 0)
		{
			yield return new WaitForSecondsRealtime(m_tickTime);
			int availableTicks = GetAvailableTicks();
			requiredProcessing -= availableTicks;
		}
		newWindow.gameObject.SetActive(value: true);
		RemoveProcess();
	}

	public void FocusWindow(ComputerWindow window, bool open = false)
	{
		window.transform.SetAsLastSibling();
		if (open && window.IsMinimized)
		{
			window.ToggleMinimized();
		}
	}

	public void ToggleMinimized(ComputerWindow window)
	{
		window.ToggleMinimized();
		PlayClickAudio();
	}

	public void CloseWindow(ComputerWindow window)
	{
		if (m_openWindows.Contains(window))
		{
			PlayClickAudio();
			StartCoroutine(CloseWindowCoroutine(window));
		}
	}

	private IEnumerator CloseWindowCoroutine(ComputerWindow window)
	{
		AddProcess();
		m_openWindows.Remove(window);
		int requiredProcessing = 20;
		while (requiredProcessing > 0)
		{
			yield return new WaitForSecondsRealtime(m_tickTime);
			int availableTicks = GetAvailableTicks();
			requiredProcessing -= availableTicks;
		}
		if (window.DestroyOnClose)
		{
			Object.Destroy(window.gameObject);
		}
		else
		{
			window.gameObject.SetActive(value: false);
		}
		RepositionWindows();
		RemoveProcess();
	}

	private void RepositionWindows()
	{
		int num = 0;
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			if (!openWindow.HasMoved)
			{
				openWindow.GetComponent<RectTransform>().anchoredPosition = GetWindowPosition(num++);
			}
		}
	}

	public void AddProcess()
	{
		if (m_processCount == 0)
		{
			m_loadingBar.gameObject.SetActive(value: true);
			m_loadingAudio.DOFade(1f, 0.2f);
		}
		m_processCount++;
	}

	public void RemoveProcess()
	{
		m_processCount--;
		if (m_processCount == 0)
		{
			m_loadingBar.gameObject.SetActive(value: false);
			m_loadingAudio.DOFade(0f, 0.2f);
		}
	}

	public int GetCurrentMemoryUsage()
	{
		int num = 149096;
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			num += openWindow.GetMemoryUsage();
		}
		return num;
	}

	public void Shutdown()
	{
		if (!m_isShuttingDown)
		{
			m_isShuttingDown = true;
			StartCoroutine(ShutdownCoroutine());
		}
	}

	private IEnumerator ShutdownCoroutine()
	{
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			openWindow.gameObject.SetActive(value: false);
		}
		m_loadingAudio.Stop();
		m_ambientAudio.DOFade(0f, 0.1f);
		m_turnOffAudio.Play();
		m_shutdown.gameObject.SetActive(value: true);
		yield return new WaitForSecondsRealtime(1f);
		m_onShutdownEvent.Invoke();
	}

	public bool TryBackInput()
	{
		ComputerWindow computerWindow = null;
		foreach (ComputerWindow openWindow in m_openWindows)
		{
			if (!(computerWindow != null) || computerWindow.transform.GetSiblingIndex() <= openWindow.transform.GetSiblingIndex())
			{
				computerWindow = openWindow;
			}
		}
		if (computerWindow != null)
		{
			CloseWindow(computerWindow);
			return true;
		}
		Shutdown();
		return true;
	}
}
