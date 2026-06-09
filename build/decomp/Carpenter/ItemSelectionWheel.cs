using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class ItemSelectionWheel : MonoBehaviour
{
	private delegate bool ShouldIncludeItem(ItemInstance item);

	[SerializeField]
	private ItemSelectionWheelSlice m_itemSelectionWheelSlicePrefab;

	[SerializeField]
	private int m_minSliceCount = 3;

	[SerializeField]
	private int m_maxItems = 12;

	[SerializeField]
	private RectTransform m_container;

	[SerializeField]
	private RectTransform m_cursor;

	[SerializeField]
	private RectTransform m_edgeCursor;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_actionText;

	[SerializeField]
	private InputPrompt m_actionButtonPrompt;

	[Header("Strings")]
	[SerializeField]
	private LocalizedString m_equipString;

	[SerializeField]
	private LocalizedString m_useString;

	[SerializeField]
	private LocalizedString m_cancelString;

	[Header("Input")]
	[SerializeField]
	private float m_selectThreshold = 0.8f;

	[SerializeField]
	private float m_cursorScalar = 100f;

	[SerializeField]
	private float m_centerHoverNullSelectionTimeMouse = 0.1f;

	private List<ItemSelectionWheelSlice> m_slices;

	private int m_activeSliceCount;

	private ItemSelectionWheelSlice m_selectedSlice;

	private Vector2 m_smoothedMouseMovement;

	private bool m_isShowing;

	private float m_hoveringOverCenterTime;

	private void Awake()
	{
		m_slices = new List<ItemSelectionWheelSlice>();
		for (int i = 0; i < m_maxItems; i++)
		{
			ItemSelectionWheelSlice itemSelectionWheelSlice = Object.Instantiate(m_itemSelectionWheelSlicePrefab, m_container, worldPositionStays: false);
			m_slices.Add(itemSelectionWheelSlice);
			itemSelectionWheelSlice.gameObject.SetActive(value: false);
		}
		m_container.gameObject.SetActive(value: false);
		m_actionText.text = m_cancelString.GetLocalizedString();
		m_itemNameText.text = "";
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Register(ShowItemWheel);
		GlobalReferences.Instance.EventChannels.Inventory.PerformSelectOnItemWheel.Register(SelectItem);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowItemWheel.Unregister(ShowItemWheel);
		GlobalReferences.Instance.EventChannels.Inventory.PerformSelectOnItemWheel.Unregister(SelectItem);
	}

	private void SelectItem()
	{
		ItemInstance itemInstance = ((m_selectedSlice != null) ? m_selectedSlice.ItemInstance : null);
		if (itemInstance != null)
		{
			GlobalReferences.Instance.EventChannels.Inventory.SelectedFromItemWheel.Raise(itemInstance);
		}
	}

	private void ShowItemWheel(bool showing)
	{
		if (m_isShowing != showing)
		{
			m_isShowing = showing;
			m_container.gameObject.SetActive(showing);
			if (showing)
			{
				m_hoveringOverCenterTime = 0f;
				m_smoothedMouseMovement = Vector2.zero;
				PopulateData(GlobalReferences.Instance.MainInventory);
				SetSelectedSlice(null);
			}
		}
	}

	private void Update()
	{
		if (m_isShowing)
		{
			UpdateSelection();
		}
	}

	private void UpdateSelection()
	{
		Vector2 input = Vector2.zero;
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
		{
			m_smoothedMouseMovement += Mouse.current.delta.value / Screen.width * 10f;
			m_smoothedMouseMovement.x = Mathf.Clamp(m_smoothedMouseMovement.x, -1f, 1f);
			m_smoothedMouseMovement.y = Mathf.Clamp(m_smoothedMouseMovement.y, -1f, 1f);
			input = m_smoothedMouseMovement;
		}
		else if (Gamepad.current != null)
		{
			input = Gamepad.current.rightStick.value;
			if (input.magnitude <= GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				input = Gamepad.current.leftStick.value;
			}
		}
		bool flag = true;
		if (input.magnitude > m_selectThreshold)
		{
			flag = false;
			input = input.normalized;
			float num = Vector2.SignedAngle(input, Vector2.up);
			if (num < 0f)
			{
				num += 360f;
			}
			float num2 = 360f / (float)m_activeSliceCount;
			ItemSelectionWheelSlice selectedSlice = null;
			for (int i = 0; i < m_activeSliceCount; i++)
			{
				float minAngle = num2 * (float)i - num2 * 0.5f;
				float maxAngle = num2 * (float)i + num2 * 0.5f;
				if (IsAngleBetween(minAngle, maxAngle, num))
				{
					selectedSlice = m_slices[i];
				}
			}
			m_cursor.gameObject.SetActive(value: false);
			m_edgeCursor.gameObject.SetActive(value: true);
			m_edgeCursor.rotation = Quaternion.Euler(0f, 0f, 0f - num);
			SetSelectedSlice(selectedSlice);
		}
		else
		{
			flag = true;
			if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
			{
				m_cursor.gameObject.SetActive(value: true);
				m_edgeCursor.gameObject.SetActive(value: false);
			}
			else if (m_selectedSlice != null)
			{
				m_cursor.gameObject.SetActive(value: false);
				m_edgeCursor.gameObject.SetActive(value: true);
				m_edgeCursor.rotation = Quaternion.Euler(0f, 0f, m_selectedSlice.transform.rotation.eulerAngles.z);
			}
			else
			{
				m_cursor.gameObject.SetActive(value: false);
				m_edgeCursor.gameObject.SetActive(value: false);
			}
			PositionCursor(input);
		}
		if (flag && m_selectedSlice != null)
		{
			m_hoveringOverCenterTime += Time.unscaledDeltaTime;
			if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.KeyboardMouse)
			{
				float centerHoverNullSelectionTimeMouse = m_centerHoverNullSelectionTimeMouse;
				if (m_hoveringOverCenterTime > centerHoverNullSelectionTimeMouse)
				{
					SetSelectedSlice(null);
				}
			}
		}
		else
		{
			m_hoveringOverCenterTime = 0f;
		}
	}

	private void PositionCursor(Vector2 input)
	{
		Vector2 anchoredPosition = input * m_cursorScalar;
		m_cursor.anchoredPosition = anchoredPosition;
	}

	private void SetSelectedSlice(ItemSelectionWheelSlice slice)
	{
		if (!(m_selectedSlice != slice))
		{
			return;
		}
		m_selectedSlice = slice;
		for (int i = 0; i < m_activeSliceCount; i++)
		{
			m_slices[i].SetSelected(slice == m_slices[i]);
		}
		bool active = false;
		if (slice != null && slice.ItemInstance != null)
		{
			if (slice.ItemInstance.CanUse(GlobalReferences.Instance.MainInventory, checkConditions: false))
			{
				m_actionText.text = m_useString.GetLocalizedString();
				active = true;
			}
			else if (slice.ItemInstance.ItemDefinition.CanBeActivated())
			{
				m_actionText.text = (slice.ItemInstance.Activated ? slice.ItemInstance.ItemDefinition.Activatable.DeactivateActionStringReference.GetLocalizedString() : slice.ItemInstance.ItemDefinition.Activatable.ActivateActionStringReference.GetLocalizedString());
				active = true;
			}
			else if (slice.ItemInstance.CanEquip(GlobalReferences.Instance.MainInventory))
			{
				m_actionText.text = m_equipString.GetLocalizedString();
				active = true;
			}
			m_itemNameText.text = slice.ItemInstance.ItemName;
		}
		else
		{
			m_actionText.text = m_cancelString.GetLocalizedString();
			m_itemNameText.text = "";
		}
		m_actionButtonPrompt.gameObject.SetActive(active);
	}

	private static bool IsAngleBetween(float minAngle, float maxAngle, float testAngle)
	{
		minAngle = (minAngle % 360f + 360f) % 360f;
		maxAngle = (maxAngle % 360f + 360f) % 360f;
		testAngle = (testAngle % 360f + 360f) % 360f;
		if (minAngle <= maxAngle)
		{
			if (testAngle > minAngle)
			{
				return testAngle <= maxAngle;
			}
			return false;
		}
		if (!(testAngle > minAngle))
		{
			return testAngle <= maxAngle;
		}
		return true;
	}

	private void PopulateData(PlayerMainInventory inventory)
	{
		List<ItemInstance> list = new List<ItemInstance>();
		List<ItemDefinition> itemTypesAlreadyShown = new List<ItemDefinition>();
		list.Add(null);
		AddItemsToList(inventory, list, itemTypesAlreadyShown, (ItemInstance x) => x.CanBeActivated() || x.ItemDefinition.CanUse(inventory, checkConditions: false));
		int count = list.Count;
		if (count < m_minSliceCount)
		{
			for (int i = count; i < m_minSliceCount; i++)
			{
				list.Add(null);
			}
		}
		m_activeSliceCount = list.Count;
		if (m_activeSliceCount <= 0)
		{
			return;
		}
		float sliceAngleSize = 360f / (float)m_activeSliceCount;
		for (int j = 0; j < m_activeSliceCount; j++)
		{
			ItemInstance itemInstance = list[j];
			m_slices[j].SetItem(itemInstance, sliceAngleSize, j);
			m_slices[j].gameObject.SetActive(value: true);
			if (itemInstance != null)
			{
				m_slices[j].SetEquipped(GlobalReferences.Instance.MainInventory.IsItemEquipped(itemInstance) || itemInstance.Activated);
			}
			else
			{
				m_slices[j].SetEquipped(isEquipped: false);
			}
		}
		for (int k = m_activeSliceCount; k < m_slices.Count; k++)
		{
			m_slices[k].gameObject.SetActive(value: false);
		}
	}

	private void AddItemsToList(PlayerMainInventory inventory, List<ItemInstance> itemsToShow, List<ItemDefinition> itemTypesAlreadyShown, ShouldIncludeItem check)
	{
		foreach (ItemInstance item in inventory.ItemList)
		{
			if (!itemTypesAlreadyShown.Contains(item.ItemDefinition) && check(item) && (item.CanBeActivated() || item.CanEquip(GlobalReferences.Instance.MainInventory) || item.CanUse(GlobalReferences.Instance.MainInventory, checkConditions: false)))
			{
				if (itemsToShow.Count >= m_maxItems)
				{
					Debug.LogError("Too many items to show in item wheel! Not all items wheel be shown!");
					break;
				}
				itemsToShow.Add(item);
				itemTypesAlreadyShown.Add(item.ItemDefinition);
			}
		}
	}
}
