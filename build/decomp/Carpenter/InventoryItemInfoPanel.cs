using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class InventoryItemInfoPanel : MonoBehaviour
{
	public enum Mode
	{
		Normal,
		MoveMode,
		Combine
	}

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_itemDescriptionText;

	[SerializeField]
	private TextMeshProUGUI m_itemCountText;

	[SerializeField]
	private float m_animTime = 0.01f;

	[SerializeField]
	private InventoryItemAttachmentListPanel m_attachmentList;

	[SerializeField]
	private Color m_itemHighlightColor = Color.white;

	[Header("Mode: Move")]
	[SerializeField]
	private LocalizedString m_moveItemTitleText;

	[SerializeField]
	private LocalizedString m_moveItemDescriptionText;

	[Header("Mode: Combine")]
	[SerializeField]
	private LocalizedString m_combineItemTitleText;

	[SerializeField]
	private LocalizedString m_combineItemDescriptionText;

	private ItemDefinition m_activeItemDefinition;

	private ItemInstance m_activeItemInstance;

	private Coroutine m_textRevealCoroutine;

	private Mode m_mode;

	private void Start()
	{
		if (m_activeItemInstance == null && m_activeItemDefinition == null)
		{
			SetHidden();
		}
	}

	public void SetShown()
	{
		base.gameObject.SetActive(value: true);
	}

	public void SetHidden()
	{
		base.gameObject.SetActive(value: false);
	}

	private void AnimateTextOut()
	{
		if (m_textRevealCoroutine != null)
		{
			StopCoroutine(m_textRevealCoroutine);
		}
		if (base.gameObject.activeInHierarchy)
		{
			m_textRevealCoroutine = StartCoroutine(DescriptionTextRevealCoroutine());
		}
		else
		{
			m_textRevealCoroutine = null;
		}
	}

	private IEnumerator DescriptionTextRevealCoroutine()
	{
		int charCount = 0;
		float timePassed = 0f;
		while (charCount < m_itemDescriptionText.text.Length)
		{
			timePassed += Time.deltaTime;
			while (timePassed >= m_animTime)
			{
				timePassed -= m_animTime;
				charCount++;
			}
			m_itemDescriptionText.maxVisibleCharacters = charCount;
			yield return new WaitForEndOfFrame();
		}
	}

	public void SetActiveItem(ItemDefinition itemDefinition)
	{
		if (!(m_activeItemDefinition == itemDefinition))
		{
			m_activeItemInstance = null;
			m_activeItemDefinition = itemDefinition;
			UpdateView();
		}
	}

	public void SetActiveItem(ItemInstance itemInstance)
	{
		if (m_activeItemInstance != itemInstance)
		{
			m_activeItemDefinition = null;
			m_activeItemInstance = itemInstance;
			UpdateView();
		}
	}

	public void SetMode(Mode mode)
	{
		if (mode != m_mode)
		{
			m_mode = mode;
			UpdateView();
		}
	}

	private string GetStylizedItemNameString(string itemName)
	{
		return "<uppercase><b><color=#" + ColorUtility.ToHtmlStringRGB(m_itemHighlightColor) + ">" + itemName + "</color></b></uppercase>";
	}

	private void UpdateView()
	{
		if (m_activeItemDefinition != null || m_activeItemInstance != null)
		{
			SetShown();
			ItemInstance itemInstance = null;
			switch (m_mode)
			{
			case Mode.MoveMode:
				m_itemNameText.text = m_moveItemTitleText.GetLocalizedString();
				m_itemDescriptionText.text = m_moveItemDescriptionText.GetLocalizedString((m_activeItemInstance != null) ? GetStylizedItemNameString(m_activeItemInstance.ItemName) : GetStylizedItemNameString(m_activeItemDefinition.ItemName));
				break;
			case Mode.Combine:
				m_itemNameText.text = m_combineItemTitleText.GetLocalizedString();
				m_itemDescriptionText.text = m_combineItemDescriptionText.GetLocalizedString((m_activeItemInstance != null) ? GetStylizedItemNameString(m_activeItemInstance.ItemName) : GetStylizedItemNameString(m_activeItemDefinition.ItemName));
				break;
			case Mode.Normal:
				m_itemNameText.text = ((m_activeItemInstance != null) ? m_activeItemInstance.ItemName : m_activeItemDefinition.ItemName);
				m_itemDescriptionText.text = ((m_activeItemInstance != null) ? m_activeItemInstance.Description : m_activeItemDefinition.Description);
				itemInstance = m_activeItemInstance;
				break;
			}
			if (m_attachmentList != null)
			{
				if (itemInstance == null)
				{
					m_attachmentList.ClearActiveItem();
				}
				else
				{
					m_attachmentList.SetActiveItemInstance(itemInstance);
				}
			}
			AnimateTextOut();
		}
		else
		{
			SetHidden();
		}
	}

	public void SetCountValue(int count)
	{
		if (count <= 1)
		{
			m_itemCountText.text = "";
		}
		else
		{
			m_itemCountText.text = "(" + count + ")";
		}
	}
}
