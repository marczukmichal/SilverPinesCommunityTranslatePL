using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Lore/Lore Item")]
public class LoreEntry : AddressableScriptableObject<LoreEntry>
{
	[Tooltip("Tooltip description that appears on the conspiracy board")]
	[SerializeField]
	private LocalizedString m_titleStringReference;

	[SerializeField]
	private LorePage[] m_pages;

	[Tooltip("Image that appears on the conspiracy board")]
	[SerializeField]
	private Sprite m_leadImage;

	[SerializeField]
	private float m_imageScale = 1f;

	[Tooltip("If set then the player can access this lore item in their notes menu")]
	[SerializeField]
	private bool m_saveToLoreInventory = true;

	[Header("Category")]
	[SerializeField]
	private LevelRegionSettings m_associatedRegion;

	[Header("Associated Lore")]
	[SerializeField]
	private LoreEntry[] m_connectedLore;

	[SerializeField]
	private LoreAudioLogSettings m_audioLog;

	[Header("Random Code")]
	[SerializeField]
	private DeterministicCodeGenerator.Settings m_randomCodeSettings;

	public string Title
	{
		get
		{
			if (!m_titleStringReference.IsEmpty)
			{
				return m_titleStringReference.GetLocalizedString();
			}
			return "";
		}
	}

	public int PageCount
	{
		get
		{
			if (m_pages == null)
			{
				return 0;
			}
			return m_pages.Length;
		}
	}

	public Sprite LeadImage
	{
		get
		{
			if (m_leadImage != null)
			{
				return m_leadImage;
			}
			LorePage[] pages = m_pages;
			foreach (LorePage lorePage in pages)
			{
				if (lorePage.Image != null)
				{
					return lorePage.Image;
				}
			}
			return null;
		}
	}

	public float ImageScale => m_imageScale;

	public bool SaveToLoreInventory => m_saveToLoreInventory;

	public LevelRegionSettings AssociatedRegion => m_associatedRegion;

	public LoreEntry[] ConnectedLore => m_connectedLore;

	public LoreAudioLogSettings AudioLog => m_audioLog;

	public bool IsAudioLog
	{
		get
		{
			if (m_audioLog != null)
			{
				return !m_audioLog.FMODEvent.IsNull;
			}
			return false;
		}
	}

	public DeterministicCodeGenerator.Settings RandomCodeSettings => m_randomCodeSettings;

	public LorePage GetPage(int index)
	{
		return m_pages[index];
	}
}
