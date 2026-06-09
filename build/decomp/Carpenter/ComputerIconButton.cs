using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerIconButton : MonoBehaviour
{
	[SerializeField]
	private ComputerDesktop m_computerDesktop;

	[SerializeField]
	private ComputerWindow m_customWindow;

	[SerializeField]
	private float m_doubleClickTime = 0.2f;

	[SerializeField]
	private Color m_selectedColor = Color.white;

	[SerializeField]
	private Color m_deselectedColor = Color.gray;

	private Image m_icon;

	private TextMeshProUGUI m_text;

	private Button m_button;

	private bool m_selected;

	private float m_clickTimer;

	private ComputerFile m_associatedFile;

	private ComputerFolder m_associatedFolder;

	private ComputerWindow m_parentWindow;

	public ComputerFile AssociatedFile => m_associatedFile;

	public ComputerFolder AssociatedFolder => m_associatedFolder;

	public ComputerWindow ParentWindow => m_parentWindow;

	private void Awake()
	{
		m_icon = GetComponent<Image>();
		m_text = GetComponentInChildren<TextMeshProUGUI>();
		m_button = GetComponent<Button>();
		m_button.onClick.AddListener(OnClicked);
		SetSelected(selected: false);
		if (m_customWindow != null)
		{
			m_customWindow.gameObject.SetActive(value: false);
		}
	}

	public void AttachDesktop(ComputerDesktop computerDesktop)
	{
		m_computerDesktop = computerDesktop;
	}

	public void AttachFile(ComputerFile computerFile)
	{
		m_associatedFile = computerFile;
		m_text.text = computerFile.FileName;
	}

	public void AttachFolder(ComputerFolder computerFolder)
	{
		m_associatedFolder = computerFolder;
		m_text.text = computerFolder.FolderName;
	}

	public void AttachWindow(ComputerWindow window)
	{
		m_parentWindow = window;
	}

	private void OnClicked()
	{
		if (m_parentWindow != null)
		{
			m_parentWindow.Focus();
		}
		m_clickTimer = Time.time;
		if (!m_selected)
		{
			m_computerDesktop.Select(this);
		}
		else if (Time.time - m_clickTimer <= m_doubleClickTime)
		{
			if (m_customWindow != null)
			{
				m_computerDesktop.OpenSpecificWindow(m_customWindow);
			}
			else if (m_associatedFolder != null)
			{
				m_computerDesktop.OpenFolder(m_associatedFolder);
			}
			else if (m_associatedFile != null)
			{
				m_computerDesktop.OpenFile(AssociatedFile);
			}
		}
	}

	public void SetSelected(bool selected)
	{
		m_selected = selected;
		m_icon.color = (selected ? m_selectedColor : m_deselectedColor);
		m_text.color = (selected ? m_selectedColor : m_deselectedColor);
	}
}
