using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class AppearsOnMap : MonoBehaviour
{
	public enum MapIconPositioning
	{
		Centered,
		FarWall,
		CloseWall
	}

	[Serializable]
	public struct AppearsOnMapSettings
	{
		[SerializeField]
		public MapIconType m_iconType;

		[ReadOnly]
		[SerializeField]
		public string m_mapIconGUID;

		[SerializeField]
		public MapIconPositioning m_mapIconPositioning;

		[Range(0f, 360f)]
		[SerializeField]
		public float m_mapIconRotation;

		[SerializeField]
		public Sprite m_customIcon;

		[SerializeField]
		public LocalizedString m_customText;

		[SerializeField]
		public Sprite m_customHoverImage;

		public MapIconType IconType => m_iconType;

		public string MapIconGUID => m_mapIconGUID;

		public MapIconPositioning IconPositioning => m_mapIconPositioning;

		public float MapIconRotation => m_mapIconRotation;

		public Sprite CustomIcon => m_customIcon;

		public LocalizedString CustomText => m_customText;

		public Sprite CustomHoverImage => m_customHoverImage;
	}

	[Header("Discover Settings")]
	[SerializeField]
	private bool m_discoverOnEnable;

	[FormerlySerializedAs("m_discoverOnInteractorClose")]
	[SerializeField]
	private bool m_discoverOnPlayerClose = true;

	[SerializeField]
	private float m_discoverDistance = 5f;

	[SerializeField]
	private bool m_showNotificationOnDisocver;

	[SerializeField]
	private AppearsOnMapSettings m_settings;

	private bool m_alreadyDiscovered;

	public AppearsOnMapSettings Settings => m_settings;

	private void Start()
	{
		if (!string.IsNullOrEmpty(Settings.MapIconGUID) && Settings.IconType != 0)
		{
			ItemPickup component = GetComponent<ItemPickup>();
			if (component != null)
			{
				component.RegisterOnTryPickupEventListener(SetDiscovered);
			}
			LorePickup component2 = GetComponent<LorePickup>();
			if (component2 != null)
			{
				component2.RegisterOnPickupEventListener(SetDiscovered);
			}
		}
	}

	public void SetDoorOpen()
	{
		GlobalReferences.Instance.MapDynamicData.SetMapIconStateBit(Settings.m_mapIconGUID, MapIconState.DoorOpen);
		GlobalReferences.Instance.MapDynamicData.ClearMapIconStateBit(Settings.m_mapIconGUID, MapIconState.DoorLocked);
	}

	public void SetDoorLocked()
	{
		GlobalReferences.Instance.MapDynamicData.SetMapIconStateBit(Settings.m_mapIconGUID, MapIconState.DoorLocked);
		GlobalReferences.Instance.MapDynamicData.ClearMapIconStateBit(Settings.m_mapIconGUID, MapIconState.DoorOpen);
	}

	public void SetCleared()
	{
		GlobalReferences.Instance.MapDynamicData.SetMapIconStateBit(Settings.m_mapIconGUID, MapIconState.Cleared);
	}

	public void SetItemRevealed()
	{
		GlobalReferences.Instance.MapDynamicData.SetMapIconStateBit(Settings.m_mapIconGUID, MapIconState.ItemRevealed);
	}

	private void OnEnable()
	{
		if (m_discoverOnEnable)
		{
			StartCoroutine(DelayedSetFlag());
		}
	}

	private IEnumerator DelayedSetFlag()
	{
		yield return new WaitForEndOfFrame();
		SetDiscovered();
	}

	public void SetDiscovered()
	{
		if (m_alreadyDiscovered)
		{
			return;
		}
		MapIconState mapIconStateBitMask = GlobalReferences.Instance.MapDynamicData.GetMapIconStateBitMask(Settings.m_mapIconGUID);
		m_alreadyDiscovered = mapIconStateBitMask.HasFlag(MapIconState.Discovered);
		if (!m_alreadyDiscovered)
		{
			GlobalReferences.Instance.MapDynamicData.SetMapIconStateBit(Settings.m_mapIconGUID, MapIconState.Discovered);
			if (m_showNotificationOnDisocver)
			{
				GlobalReferences.Instance.EventChannels.Map.DiscoveredAppearsOnMap.Raise(this);
			}
			m_alreadyDiscovered = true;
		}
	}

	private void Update()
	{
		if (m_alreadyDiscovered || !m_discoverOnPlayerClose)
		{
			return;
		}
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			Vector2 a = item.transform.position;
			Vector2 b = base.transform.position;
			if (Vector2.Distance(a, b) < m_discoverDistance)
			{
				SetDiscovered();
			}
		}
	}

	public Vector3 GetWorldPosition()
	{
		Interactable interactable = GetComponentInParent<Interactable>();
		if (interactable == null)
		{
			interactable = GetComponentInChildren<Interactable>();
		}
		if (interactable != null)
		{
			return interactable.InteractPromptPosition;
		}
		return base.transform.position;
	}
}
