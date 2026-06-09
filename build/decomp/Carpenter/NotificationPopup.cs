using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class NotificationPopup : MonoBehaviour
{
	public enum NotificationType
	{
		None,
		LoreEntry,
		PhoneNumber,
		Key,
		MapUpdated,
		ArtifactAdded,
		CollectableAdded,
		NewArtifactSlot,
		GameSaved,
		PhotoTaken,
		ItemPickedUp,
		ItemPurchased
	}

	private class NotificationQueueEntry
	{
		public NotificationType m_notificationType;

		public LoreEntry m_loreEntry;

		public PhoneNumber m_phoneNumber;

		public ItemInstance m_key;

		public AppearsOnMap m_appearsOnMap;

		public ItemDefinition m_artifact;

		public CollectableDefinition m_collectable;

		public PhotoData m_photo;

		public AddItemEventData m_addItemEventData;
	}

	[Header("UI")]
	[SerializeField]
	private TextMeshProUGUI m_titleText;

	[SerializeField]
	private TextMeshProUGUI m_bodyText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private GameObject m_photoContainer;

	[SerializeField]
	private RawImage m_photoImage;

	[Header("Images")]
	[SerializeField]
	private Sprite m_phoneImage;

	[SerializeField]
	private Sprite m_artifactSlotImage;

	[SerializeField]
	private Sprite m_mapImage;

	[Header("Prompt Settings")]
	[SerializeField]
	private GameObject m_promptContainer;

	[SerializeField]
	private InputPrompt m_buttonPrompt;

	[SerializeField]
	private InputActionReference m_actionReferenceLoreGamepad;

	[SerializeField]
	private InputActionReference m_actionReferenceLoreMouseKeyboard;

	[Header("Animation")]
	[SerializeField]
	private float m_delayTime;

	[SerializeField]
	private float m_holdTime;

	[SerializeField]
	private float m_animInTime;

	[SerializeField]
	private float m_animOutTime;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_loreAddedString;

	[SerializeField]
	private LocalizedString m_phoneNumberAddedString;

	[SerializeField]
	private LocalizedString m_keyAddedString;

	[SerializeField]
	private LocalizedString m_artifactAddedString;

	[SerializeField]
	private LocalizedString m_collectablePickedUpString;

	[SerializeField]
	private LocalizedString m_mapUpdatedString;

	[SerializeField]
	private LocalizedString m_newArtifactSlotString;

	[SerializeField]
	private LocalizedString m_gameSavedString;

	[SerializeField]
	private LocalizedString m_photoTakenString;

	[SerializeField]
	private LocalizedString m_itemPicekdUpString;

	[SerializeField]
	private LocalizedString m_itemPurchasedString;

	private bool m_isShowing;

	private RectTransform m_rectTransform;

	private bool m_hasNewNotification;

	private Queue<NotificationQueueEntry> m_queue;

	private void Start()
	{
		m_rectTransform = GetComponent<RectTransform>();
		m_queue = new Queue<NotificationQueueEntry>();
		m_canvasGroup.alpha = 0f;
		m_rectTransform.pivot = new Vector2(0f, 1f);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Lore.LoreAddedToLoreInventory.Register(OnLoreEntryAdded);
		GlobalReferences.Instance.EventChannels.Phone.PhoneNumberAdded.Register(OnPhoneNumberAdded);
		GlobalReferences.Instance.EventChannels.Inventory.KeyAutoAddedToKeyRing.Register(OnKeyAutoAddedToKeyRing);
		GlobalReferences.Instance.EventChannels.Map.DiscoveredAppearsOnMap.Register(OnAppearsOnMapDiscovered);
		GlobalReferences.Instance.EventChannels.Inventory.AddArtifactItem.Register(OnArtifactItemAdded);
		GlobalReferences.Instance.EventChannels.Collectables.PickupCollectableDefinition.Register(OnCollectablePickedUp);
		GlobalReferences.Instance.EventChannels.Inventory.UnlockNewArtifactSlot.Register(OnArtifactSlotUnlocked);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Register(OnGameSaved);
		GlobalReferences.Instance.EventChannels.Photos.NewPhotoAdded.Register(OnPhotoTaken);
		GlobalReferences.Instance.EventChannels.Inventory.ItemPickedUp.Register(OnItemPickedUp);
		GlobalReferences.Instance.EventChannels.Inventory.ItemPurchased.Register(OnItemPurchased);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Lore.LoreAddedToLoreInventory.Unregister(OnLoreEntryAdded);
		GlobalReferences.Instance.EventChannels.Phone.PhoneNumberAdded.Unregister(OnPhoneNumberAdded);
		GlobalReferences.Instance.EventChannels.Inventory.KeyAutoAddedToKeyRing.Unregister(OnKeyAutoAddedToKeyRing);
		GlobalReferences.Instance.EventChannels.Map.DiscoveredAppearsOnMap.Unregister(OnAppearsOnMapDiscovered);
		GlobalReferences.Instance.EventChannels.Inventory.AddArtifactItem.Unregister(OnArtifactItemAdded);
		GlobalReferences.Instance.EventChannels.Collectables.PickupCollectableDefinition.Unregister(OnCollectablePickedUp);
		GlobalReferences.Instance.EventChannels.Inventory.UnlockNewArtifactSlot.Unregister(OnArtifactSlotUnlocked);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Unregister(OnGameSaved);
		GlobalReferences.Instance.EventChannels.Photos.NewPhotoAdded.Unregister(OnPhotoTaken);
		GlobalReferences.Instance.EventChannels.Inventory.ItemPickedUp.Unregister(OnItemPickedUp);
		GlobalReferences.Instance.EventChannels.Inventory.ItemPurchased.Unregister(OnItemPurchased);
	}

	private void OnLoreEntryAdded(LoreEntry loreEntry)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.LoreEntry,
			m_loreEntry = loreEntry
		});
		m_hasNewNotification = true;
	}

	private void OnPhoneNumberAdded(PhoneNumber phoneNumberData)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.PhoneNumber,
			m_phoneNumber = phoneNumberData
		});
		m_hasNewNotification = true;
	}

	private void OnKeyAutoAddedToKeyRing(ItemInstance key)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.Key,
			m_key = key
		});
		m_hasNewNotification = true;
	}

	private void OnArtifactItemAdded(AddItemEventData artifact)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.ArtifactAdded,
			m_artifact = artifact.m_itemDefinition
		});
		m_hasNewNotification = true;
	}

	private void OnCollectablePickedUp(CollectableDefinition collectable)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.CollectableAdded,
			m_collectable = collectable
		});
		m_hasNewNotification = true;
	}

	private void OnArtifactSlotUnlocked()
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.NewArtifactSlot
		});
		m_hasNewNotification = true;
	}

	private void OnGameSaved()
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.GameSaved
		});
		m_hasNewNotification = true;
	}

	private void OnPhotoTaken(PhotoData photo)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.PhotoTaken,
			m_photo = photo
		});
		m_hasNewNotification = true;
	}

	private void OnItemPickedUp(AddItemEventData addItemEvent)
	{
		if (!addItemEvent.m_supressNotification)
		{
			m_queue.Enqueue(new NotificationQueueEntry
			{
				m_notificationType = NotificationType.ItemPickedUp,
				m_addItemEventData = addItemEvent
			});
			m_hasNewNotification = true;
		}
	}

	private void OnItemPurchased(AddItemEventData addItemEvent)
	{
		if (!addItemEvent.m_supressNotification)
		{
			m_queue.Enqueue(new NotificationQueueEntry
			{
				m_notificationType = NotificationType.ItemPurchased,
				m_addItemEventData = addItemEvent
			});
			m_hasNewNotification = true;
		}
	}

	private void OnAppearsOnMapDiscovered(AppearsOnMap appearsOnMap)
	{
		m_queue.Enqueue(new NotificationQueueEntry
		{
			m_notificationType = NotificationType.MapUpdated,
			m_appearsOnMap = appearsOnMap
		});
		m_hasNewNotification = true;
	}

	private void Update()
	{
		if (!m_isShowing && m_queue.Count > 0)
		{
			NotificationQueueEntry popup = m_queue.Dequeue();
			StartCoroutine(ShowNotificationPopup(popup));
		}
	}

	private void PopulateFromEntry(NotificationQueueEntry entry)
	{
		bool active = false;
		bool active2 = false;
		m_image.sprite = null;
		switch (entry.m_notificationType)
		{
		case NotificationType.LoreEntry:
			if (entry.m_loreEntry != null)
			{
				m_titleText.text = m_loreAddedString.GetLocalizedString();
				m_bodyText.text = entry.m_loreEntry.Title;
				m_image.sprite = entry.m_loreEntry.LeadImage;
			}
			break;
		case NotificationType.PhoneNumber:
			if (entry.m_phoneNumber != null)
			{
				m_titleText.text = m_phoneNumberAddedString.GetLocalizedString();
				m_bodyText.text = entry.m_phoneNumber.PhoneName;
				m_image.sprite = m_phoneImage;
			}
			break;
		case NotificationType.Key:
			if (entry.m_key != null)
			{
				m_titleText.text = m_keyAddedString.GetLocalizedString();
				m_bodyText.text = entry.m_key.ItemDefinition.ItemName;
			}
			break;
		case NotificationType.MapUpdated:
			if (entry.m_appearsOnMap != null)
			{
				m_titleText.text = m_mapUpdatedString.GetLocalizedString();
				m_bodyText.text = entry.m_appearsOnMap.Settings.m_customText.GetLocalizedString();
				m_image.sprite = m_mapImage;
			}
			break;
		case NotificationType.ArtifactAdded:
			if (entry.m_artifact != null)
			{
				m_titleText.text = m_artifactAddedString.GetLocalizedString();
				m_bodyText.text = entry.m_artifact.ItemName;
				m_image.sprite = entry.m_artifact.InventorySprite;
			}
			break;
		case NotificationType.CollectableAdded:
			if (entry.m_collectable != null)
			{
				m_titleText.text = m_collectablePickedUpString.GetLocalizedString();
				m_bodyText.text = entry.m_collectable.CollectableName;
				m_image.sprite = entry.m_collectable.Sprite;
			}
			break;
		case NotificationType.NewArtifactSlot:
			m_titleText.text = m_newArtifactSlotString.GetLocalizedString();
			m_bodyText.text = "";
			m_image.sprite = m_artifactSlotImage;
			break;
		case NotificationType.GameSaved:
			m_titleText.text = m_gameSavedString.GetLocalizedString();
			m_bodyText.text = "";
			break;
		case NotificationType.PhotoTaken:
			m_titleText.text = m_photoTakenString.GetLocalizedString();
			m_bodyText.text = "";
			active2 = true;
			m_photoImage.texture = entry.m_photo.Texture;
			break;
		case NotificationType.ItemPickedUp:
		case NotificationType.ItemPurchased:
		{
			m_titleText.text = ((entry.m_notificationType == NotificationType.ItemPurchased) ? m_itemPurchasedString.GetLocalizedString() : m_itemPicekdUpString.GetLocalizedString());
			string text = entry.m_addItemEventData.m_itemDefinition.ItemName;
			if (entry.m_addItemEventData.m_itemDefinition.MaxStackSize > 1 || entry.m_addItemEventData.m_itemDefinition is AmmunitionItemDefinition)
			{
				text = text + " <color=white><b>x" + entry.m_addItemEventData.m_itemAmount + "</color></b>";
			}
			m_bodyText.text = text;
			m_image.sprite = entry.m_addItemEventData.m_itemDefinition.InventorySprite;
			break;
		}
		}
		m_photoContainer.SetActive(active2);
		m_image.gameObject.SetActive(m_image.sprite != null);
		m_promptContainer.SetActive(active);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectTransform);
	}

	private bool ShouldPause()
	{
		if (ShouldTryHide())
		{
			return true;
		}
		if (GlobalReferences.Instance.GameMenuState.IsInAnyMenu() && !GlobalReferences.Instance.GameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.Minigame) && !GlobalReferences.Instance.GameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.InGameMenu))
		{
			return !GlobalReferences.Instance.GameMenuState.IsInMenu(GameMenuState.GameMenu.ApplyInteract);
		}
		return false;
	}

	private bool ShouldTryHide()
	{
		return GameCutsceneManager.CutsceneActive;
	}

	private IEnumerator ShowNotificationPopup(NotificationQueueEntry popup)
	{
		m_hasNewNotification = false;
		yield return new WaitForSecondsRealtime(m_delayTime);
		while (ShouldPause())
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		m_isShowing = true;
		PopulateFromEntry(popup);
		if (popup.m_notificationType == NotificationType.PhotoTaken)
		{
			m_photoImage.color = Color.black;
			m_photoImage.DOColor(Color.white, m_holdTime);
		}
		m_canvasGroup.alpha = 0f;
		m_rectTransform.pivot = new Vector2(1f, 1f);
		yield return m_canvasGroup.DOFade(1f, m_animInTime).WaitForCompletion();
		while (ShouldPause() && !ShouldTryHide())
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		float timer = 0f;
		while (timer < m_holdTime && !m_hasNewNotification && !ShouldTryHide())
		{
			timer += Time.unscaledDeltaTime;
			yield return new WaitForEndOfFrame();
		}
		while (ShouldPause() && !ShouldTryHide())
		{
			yield return new WaitForSecondsRealtime(0.1f);
		}
		if (!m_hasNewNotification)
		{
			m_promptContainer.SetActive(value: false);
			m_canvasGroup.DOFade(0f, m_animOutTime * 0.5f).SetEase(Ease.OutQuad);
			yield return m_rectTransform.DOPivotX(0f, m_animOutTime).SetEase(Ease.InSine).WaitForCompletion();
		}
		m_canvasGroup.alpha = 0f;
		m_isShowing = false;
	}
}
