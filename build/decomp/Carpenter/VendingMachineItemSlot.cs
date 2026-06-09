using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class VendingMachineItemSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IItemPickup
{
	[SerializeField]
	private TextMeshProUGUI m_costTextLabel;

	[SerializeField]
	private TextMeshProUGUI m_idTextLabel;

	[SerializeField]
	private string m_id;

	private VendingMachineMinigameData.VendingMachineItemDefinition m_vendingItemDefintion;

	[SerializeField]
	private Color m_normalColor;

	[SerializeField]
	private Color m_backgroundItem1Color;

	[SerializeField]
	private Color m_backgroundItem2Color;

	[SerializeField]
	private RectTransform m_frontPosition;

	[SerializeField]
	private RectTransform m_position1Position;

	[SerializeField]
	private RectTransform m_position2Position;

	[SerializeField]
	private Color m_highlightColor;

	[SerializeField]
	private float m_spoolPushDuration = 1f;

	[SerializeField]
	private float m_purchaseFallSpeed = 1000f;

	[SerializeField]
	private float m_purchaseFallRotationRate = 100f;

	[SerializeField]
	private float m_fallDistance = 1024f;

	[SerializeField]
	private GameObject m_highlight;

	[SerializeField]
	private Image m_spool;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_startSpoolingAudio;

	[SerializeField]
	private AudioEvent m_itemLandAudio;

	[Header("Product Image")]
	[SerializeField]
	private Image[] m_images;

	[SerializeField]
	private float m_itemSizeScalar = 1f;

	private float m_baseSize;

	private int m_instancesRemaining;

	private int m_calculatedCost;

	public UnityAction<VendingMachineItemSlot, bool> OnHovered;

	private float m_baseScaleSize;

	public string ID => m_id;

	public VendingMachineMinigameData.VendingMachineItemDefinition VendingItemDefinition => m_vendingItemDefintion;

	private Image FrontImage
	{
		get
		{
			Image[] images = m_images;
			foreach (Image image in images)
			{
				if (image.isActiveAndEnabled)
				{
					return image;
				}
			}
			return null;
		}
	}

	public bool SoldOut => m_instancesRemaining == 0;

	public int Cost => m_calculatedCost;

	public ItemDefinition ItemDefinition
	{
		get
		{
			if (m_vendingItemDefintion != null)
			{
				return m_vendingItemDefintion.ItemDefinition;
			}
			return null;
		}
	}

	public int ItemAmount
	{
		get
		{
			return m_vendingItemDefintion.ItemDefinition.ShopStackSize;
		}
		set
		{
		}
	}

	public int AmountInItem => 0;

	public LootType LootType => LootType.ItemDefinition;

	public bool SuppressNotification => false;

	public bool HasItem()
	{
		if (m_vendingItemDefintion != null)
		{
			return !SoldOut;
		}
		return false;
	}

	public void Start()
	{
		m_baseSize = m_images[0].rectTransform.rect.width;
		m_idTextLabel.text = m_id;
		m_costTextLabel.gameObject.SetActive(value: false);
		m_images[0].color = m_normalColor;
		m_images[1].color = m_backgroundItem1Color;
		m_images[2].color = m_backgroundItem2Color;
		Image[] images = m_images;
		for (int i = 0; i < images.Length; i++)
		{
			images[i].gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		Image[] images = m_images;
		foreach (Image obj in images)
		{
			DOTween.Kill(obj.GetComponent<RectTransform>());
			DOTween.Kill(obj);
		}
		DOTween.Kill(m_spool);
	}

	public void Configure(VendingMachineMinigameData.VendingMachineItemDefinition vendingItemDefinition, int instanceCount)
	{
		ItemDefinition itemDefinition = vendingItemDefinition.ItemDefinition;
		m_calculatedCost = Mathf.RoundToInt(itemDefinition.BaseShopPrice * (float)vendingItemDefinition.ItemDefinition.ShopStackSize);
		m_costTextLabel.gameObject.SetActive(value: true);
		if (vendingItemDefinition.m_instanceCount > 1)
		{
			m_instancesRemaining = vendingItemDefinition.m_instanceCount;
		}
		else
		{
			m_instancesRemaining = 1;
		}
		float num = 1f;
		if (itemDefinition.InventorySprite != null && itemDefinition.InventorySprite.rect.width > itemDefinition.InventorySprite.rect.height)
		{
			num = itemDefinition.InventorySprite.rect.width / itemDefinition.InventorySprite.rect.height;
		}
		m_baseScaleSize = itemDefinition.VendingMachineSizeScalar * m_itemSizeScalar * num;
		int num2 = 0;
		Image[] images = m_images;
		foreach (Image obj in images)
		{
			obj.gameObject.SetActive(num2 < instanceCount);
			obj.sprite = itemDefinition.InventorySprite;
			obj.transform.localScale = Vector3.one * m_baseScaleSize;
			num2++;
		}
		m_costTextLabel.text = "$" + m_calculatedCost;
		m_vendingItemDefintion = vendingItemDefinition;
	}

	public void SetHighlighted(bool highlight)
	{
		FrontImage.color = (highlight ? m_highlightColor : m_normalColor);
		m_highlight.gameObject.SetActive(highlight);
	}

	public void OnPurchase()
	{
		m_instancesRemaining--;
		StartCoroutine(PurchaseCoroutine());
	}

	private IEnumerator PurchaseCoroutine()
	{
		if (m_startSpoolingAudio != null)
		{
			m_startSpoolingAudio.Play2D();
		}
		base.transform.SetAsLastSibling();
		FrontImage.color = m_normalColor;
		m_spool.DOFillAmount(0f, m_spoolPushDuration);
		int num = 0;
		Image[] images = m_images;
		foreach (Image image in images)
		{
			if (image.gameObject.activeSelf && !(image == FrontImage))
			{
				Color endValue = m_normalColor;
				Vector2 anchoredPosition = m_frontPosition.anchoredPosition;
				switch (num)
				{
				case 1:
					endValue = m_backgroundItem1Color;
					anchoredPosition = m_position1Position.anchoredPosition;
					break;
				case 2:
					endValue = m_backgroundItem2Color;
					anchoredPosition = m_position2Position.anchoredPosition;
					break;
				}
				num++;
				image.rectTransform.DOAnchorPos(anchoredPosition, m_spoolPushDuration);
				image.DOColor(endValue, m_spoolPushDuration * 0.5f);
			}
		}
		FrontImage.DOColor(m_highlightColor, m_spoolPushDuration * 0.5f);
		yield return FrontImage.transform.DOScale(1.1f * m_baseScaleSize, m_spoolPushDuration * 0.5f).WaitForCompletion();
		RectTransform component = FrontImage.GetComponent<RectTransform>();
		Vector2 anchoredPosition2 = component.anchoredPosition;
		anchoredPosition2.y -= m_fallDistance;
		Vector3 endValue2 = new Vector3(0f, 0f, Random.Range(-45f, 45f));
		component.DOLocalRotate(endValue2, m_purchaseFallRotationRate).SetSpeedBased(isSpeedBased: true);
		Color black = Color.black;
		FrontImage.DOColor(black, 2f);
		yield return component.DOAnchorPos(anchoredPosition2, m_purchaseFallSpeed).SetEase(Ease.InSine).SetSpeedBased(isSpeedBased: true)
			.WaitForCompletion();
		if (m_itemLandAudio != null)
		{
			m_itemLandAudio.Play2D();
		}
		FrontImage.gameObject.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnHovered?.Invoke(this, arg1: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnHovered?.Invoke(this, arg1: false);
	}
}
