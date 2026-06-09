using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoreReader : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_textFadeCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private TextMeshProUGUI m_pageInfoText;

	[SerializeField]
	private TextMeshProUGUI m_infoText;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private LoreEntry m_activeLoreItem;

	[Header("Page Switch")]
	[SerializeField]
	private Image m_nextPageArrow;

	[SerializeField]
	private Image m_previousPageArrow;

	[SerializeField]
	private Color m_arrowNormalColor;

	[SerializeField]
	private Color m_arrowDisabledColor;

	[Header("Layout")]
	[SerializeField]
	private RectTransform m_textTransform;

	[SerializeField]
	private Transform m_textLayoutParent;

	[SerializeField]
	private Transform m_imageLayoutParent;

	[SerializeField]
	private Color m_imageDarkenTint;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_audioPageChangeEvent;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_pageCountStringReference;

	[FormerlySerializedAs("m_availableInputs")]
	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputsWithPages;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputsNoPages;

	[SerializeField]
	private bool m_isGameLoreReader;

	private int m_activePageIndex;

	private float m_openTime;

	public UnityAction<bool> m_onLoreReaderActive;

	public MenuInputPrompts.AvaialbleInput[] AvailableInputs
	{
		get
		{
			if (m_activeLoreItem == null || m_activeLoreItem.PageCount <= 1)
			{
				return m_availableInputsNoPages;
			}
			return m_availableInputsWithPages;
		}
	}

	private void Start()
	{
		if (!m_isGameLoreReader)
		{
			if ((bool)m_activeLoreItem)
			{
				ShowLore(m_activeLoreItem);
			}
			else
			{
				ShowEmpty();
			}
		}
	}

	private void OnEnable()
	{
		m_openTime = Time.unscaledTime;
		GameInputManager.GameInputActions.UI.NextPage.performed += UINextPage;
		GameInputManager.GameInputActions.UI.PreviousPage.performed += UIPreviousPage;
		m_onLoreReaderActive?.Invoke(arg0: true);
	}

	private void OnDisable()
	{
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.NextPage.performed -= UINextPage;
			GameInputManager.GameInputActions.UI.PreviousPage.performed -= UIPreviousPage;
		}
		m_onLoreReaderActive?.Invoke(arg0: false);
	}

	private void ShowEmpty()
	{
		m_titleText.text = "";
		m_pageInfoText.text = "";
		m_infoText.text = "";
		m_image.gameObject.SetActive(value: false);
		m_textFadeCanvasGroup.alpha = 0f;
		SetupLayout(LorePage.Layout.Text);
		UpdateButtonVisibility();
	}

	private void SetTitleText(string newTitle, int pageNumber, int totalPageCount)
	{
		if (totalPageCount > 1)
		{
			m_pageInfoText.gameObject.SetActive(value: true);
			m_pageInfoText.text = m_pageCountStringReference.GetLocalizedString(pageNumber + 1, totalPageCount);
		}
		else
		{
			m_pageInfoText.gameObject.SetActive(value: false);
		}
		m_titleText.text = newTitle;
	}

	public void ShowLore(LoreEntry item)
	{
		m_activeLoreItem = item;
		m_activePageIndex = -1;
		m_textFadeCanvasGroup.alpha = 1f;
		ShowPage(0);
	}

	public void ShowPage(int page)
	{
		if (m_activePageIndex != page)
		{
			DOTween.Kill(m_image);
			m_activePageIndex = page;
			UpdateButtonVisibility();
			LorePage page2 = m_activeLoreItem.GetPage(page);
			PopulateFromPage(page2);
			SetTitleText(m_activeLoreItem.Title, m_activePageIndex, m_activeLoreItem.PageCount);
			SetupLayout(page2.PageLayout);
			AudioEvent.Play2D(m_audioPageChangeEvent);
		}
	}

	private void SetupLayout(LorePage.Layout layout)
	{
		switch (layout)
		{
		case LorePage.Layout.Text:
			m_image.color = m_imageDarkenTint;
			m_textTransform.SetParent(m_textLayoutParent);
			break;
		case LorePage.Layout.Image:
			m_image.color = Color.white;
			m_textTransform.SetParent(m_imageLayoutParent);
			break;
		}
		m_textTransform.anchoredPosition = Vector2.zero;
		m_textTransform.sizeDelta = Vector2.zero;
	}

	private void PopulateFromPage(LorePage page)
	{
		string text = "";
		if (page.TextStringReference != null && !page.TextStringReference.IsEmpty)
		{
			if (m_activeLoreItem.RandomCodeSettings.m_id > 0 && m_activeLoreItem.RandomCodeSettings.m_length > 0)
			{
				string numbersCode = DeterministicCodeGenerator.GetNumbersCode(m_activeLoreItem.RandomCodeSettings);
				text = page.TextStringReference.GetLocalizedString(numbersCode).Replace("\r", "");
			}
			else
			{
				text = page.TextStringReference.GetLocalizedString().Replace("\r", "");
			}
		}
		m_infoText.text = text;
		Sprite sprite = m_activeLoreItem.LeadImage;
		if (page.Image != null)
		{
			sprite = page.Image;
		}
		m_image.gameObject.SetActive(sprite);
		if ((bool)sprite)
		{
			m_image.sprite = sprite;
			m_image.transform.localScale = Vector3.one * m_activeLoreItem.ImageScale;
		}
	}

	public void NextPage()
	{
		if (HasNextPage())
		{
			ShowPage(m_activePageIndex + 1);
		}
	}

	public void PreviousPage()
	{
		if (HasPreviousPage())
		{
			ShowPage(m_activePageIndex - 1);
		}
	}

	private bool HasNextPage()
	{
		if (m_activeLoreItem == null)
		{
			return false;
		}
		return m_activeLoreItem.PageCount > m_activePageIndex + 1;
	}

	private bool HasPreviousPage()
	{
		if (m_activeLoreItem == null)
		{
			return false;
		}
		return m_activePageIndex > 0;
	}

	private void UpdateButtonVisibility()
	{
		m_nextPageArrow.color = (HasNextPage() ? m_arrowNormalColor : m_arrowDisabledColor);
		m_previousPageArrow.color = (HasPreviousPage() ? m_arrowNormalColor : m_arrowDisabledColor);
	}

	public void TryClose()
	{
		if (Time.unscaledTime - m_openTime > 0.25f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void UINextPage(InputAction.CallbackContext input)
	{
		if (input.performed)
		{
			NextPage();
		}
	}

	private void UIPreviousPage(InputAction.CallbackContext input)
	{
		if (input.performed)
		{
			PreviousPage();
		}
	}

	public void DoReadFromGameAnimationFlow()
	{
		m_textFadeCanvasGroup.alpha = 0f;
		m_textFadeCanvasGroup.DOFade(1f, 1f).SetDelay(0f).SetUpdate(isIndependentUpdate: true);
		Color color = m_image.color;
		m_image.color = Color.white;
		m_image.DOColor(color, 1f).SetDelay(0f);
	}
}
