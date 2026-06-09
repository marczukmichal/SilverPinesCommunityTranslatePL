using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryKeyRingPanel : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private GameObject m_keyTemplate;

	[SerializeField]
	private InventoryItemInfoPanel m_infoPanel;

	[SerializeField]
	private Transform m_keyListParent;

	[SerializeField]
	private GameObject m_useKeyButton;

	[Header("Animation")]
	[SerializeField]
	private float m_rotationSpeed = 360f;

	[SerializeField]
	private float m_scaleFocused = 1f;

	[SerializeField]
	private float m_scaleSmallest = 0.4f;

	[SerializeField]
	private Color m_colorFocused = Color.white;

	[SerializeField]
	private Color m_colorUnfocused = Color.gray;

	[SerializeField]
	private Color m_wrongItemColorFlash;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_audioShowKeyRingEvent;

	[SerializeField]
	private AudioEvent m_audioUseKeySuccessEvent;

	[SerializeField]
	private AudioEvent m_audioUseKeyFailEvent;

	[SerializeField]
	private AudioEvent m_audioSelectKeyEvent;

	private KeyRingItemInstance m_keyRing;

	private GameObject[] m_keyUIObjects;

	private int m_selectedIndex;

	private float m_lastInputMoveTime;

	private float m_currentT;

	private bool m_isMoving;

	private bool m_isItemAnimating;

	private bool m_doneCorrectKey;

	private ItemInstance m_lastSelectedKey;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowKeyring.Register(ShowEvent);
		Hide();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowKeyring.Unregister(ShowEvent);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.UI.Navigate.performed -= UINavigateAction;
			GameInputManager.GameInputActions.UI.Cancel.performed -= CloseKeyRingInput;
			GameInputManager.GameInputActions.UI.UseItem.performed -= UseKeyInput;
		}
	}

	private void ShowEvent(ItemInstance itemInstance)
	{
		if (itemInstance == null)
		{
			Hide();
		}
		else if (itemInstance is KeyRingItemInstance keyRing)
		{
			Show(keyRing);
		}
	}

	public void Show(KeyRingItemInstance keyRing)
	{
		m_keyRing = keyRing;
		m_doneCorrectKey = false;
		m_container.SetActive(value: true);
		GameInputManager.GameInputActions.UI.Navigate.performed += UINavigateAction;
		GameInputManager.GameInputActions.UI.Cancel.performed += CloseKeyRingInput;
		GameInputManager.GameInputActions.UI.UseItem.performed += UseKeyInput;
		Populate();
		AudioEvent.Play2D(m_audioShowKeyRingEvent);
		m_useKeyButton.SetActive(GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Item != null);
	}

	private void Populate()
	{
		m_selectedIndex = 0;
		int keyCount = m_keyRing.GetKeyCount();
		if (m_keyUIObjects != null)
		{
			GameObject[] keyUIObjects = m_keyUIObjects;
			for (int i = 0; i < keyUIObjects.Length; i++)
			{
				UnityEngine.Object.Destroy(keyUIObjects[i]);
			}
		}
		m_keyUIObjects = new GameObject[keyCount];
		for (int j = 0; j < keyCount; j++)
		{
			ItemInstance key = m_keyRing.GetKey(j);
			GameObject gameObject = UnityEngine.Object.Instantiate(m_keyTemplate, m_keyListParent, worldPositionStays: false);
			gameObject.name = key.ItemDefinition.name;
			gameObject.GetComponent<Image>().sprite = key.ItemDefinition.InventorySprite;
			m_keyUIObjects[j] = gameObject;
			if (key == m_lastSelectedKey)
			{
				m_selectedIndex = j;
			}
		}
		m_currentT = (float)m_selectedIndex / (float)m_keyUIObjects.Length * 360f;
		SelectedKeyChanged();
	}

	public void Hide()
	{
		m_container.SetActive(value: false);
		GameInputManager.GameInputActions.UI.Navigate.performed -= UINavigateAction;
		GameInputManager.GameInputActions.UI.Cancel.performed -= CloseKeyRingInput;
		GameInputManager.GameInputActions.UI.UseItem.performed -= UseKeyInput;
	}

	public void SelectedKeyChanged()
	{
		if (m_keyRing.GetKeyCount() != 0)
		{
			ItemInstance key = m_keyRing.GetKey(m_selectedIndex);
			m_infoPanel.SetActiveItem(key.ItemDefinition);
			m_lastSelectedKey = key;
		}
	}

	private void Update()
	{
		if (m_keyUIObjects != null && m_container.activeSelf && !m_isItemAnimating)
		{
			float num = (float)m_selectedIndex / (float)m_keyUIObjects.Length * 360f;
			float num2 = (m_currentT = Mathf.MoveTowardsAngle(m_currentT, num, Time.unscaledDeltaTime * m_rotationSpeed));
			m_isMoving = m_currentT != num;
			float num3 = 180f - num2;
			num3 %= 360f;
			float target = 180f;
			Vector2 vector = default(Vector2);
			for (int i = 0; i < m_keyUIObjects.Length; i++)
			{
				vector.x = (Mathf.Sin(MathF.PI / 180f * num3) + 1f) * 0.5f;
				vector.y = (Mathf.Cos(MathF.PI / 180f * num3) + 1f) * 0.5f;
				RectTransform component = m_keyUIObjects[i].GetComponent<RectTransform>();
				component.anchorMin = vector;
				component.anchorMax = vector;
				float t = Mathf.Abs(Mathf.DeltaAngle(num3, target) / 180f);
				float num4 = Mathf.Lerp(m_scaleFocused, m_scaleSmallest, t);
				component.localScale = new Vector3(num4, num4, num4);
				m_keyUIObjects[i].GetComponent<Image>().color = Color.Lerp(m_colorFocused, m_colorUnfocused, t);
				num3 += 360f / (float)m_keyUIObjects.Length;
				num3 %= 360f;
			}
		}
	}

	public void CycleKeyLeft()
	{
		if (!m_isMoving && !m_isItemAnimating && !m_doneCorrectKey)
		{
			int keyCount = m_keyRing.GetKeyCount();
			if (keyCount > 1)
			{
				m_selectedIndex++;
				m_selectedIndex %= keyCount;
				SelectedKeyChanged();
				AudioEvent.Play2D(m_audioSelectKeyEvent);
			}
		}
	}

	public void CycleKeyRight()
	{
		if (m_isMoving || m_isItemAnimating || m_doneCorrectKey)
		{
			return;
		}
		int keyCount = m_keyRing.GetKeyCount();
		if (keyCount > 1)
		{
			m_selectedIndex--;
			if (m_selectedIndex < 0)
			{
				m_selectedIndex = keyCount - 1;
			}
			SelectedKeyChanged();
			AudioEvent.Play2D(m_audioSelectKeyEvent);
		}
	}

	private void UseKeyInput(InputAction.CallbackContext input)
	{
		UseKey();
	}

	public void UseKey()
	{
		if (m_isItemAnimating)
		{
			return;
		}
		IApplyItem item = GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Item;
		if (item != null)
		{
			ItemInstance key = m_keyRing.GetKey(m_selectedIndex);
			bool flag = item.CanApplyItem(key);
			GlobalReferences.Instance.EventChannels.Generic.TryApplyItemSuccessFail.Raise(flag);
			if (flag)
			{
				m_doneCorrectKey = true;
				GlobalReferences.Instance.EventChannels.Inventory.ApplyItemToInteractable.Raise(key);
				StartCoroutine(AnimateApplyItemCorrectItemCoroutine(m_keyUIObjects[m_selectedIndex]));
			}
			else
			{
				StartCoroutine(AnimateApplyItemWrongItemCoroutine(m_keyUIObjects[m_selectedIndex]));
			}
		}
	}

	private IEnumerator AnimateApplyItemWrongItemCoroutine(GameObject keyObject)
	{
		m_isItemAnimating = true;
		Image image = keyObject.GetComponent<Image>();
		image.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.25f).SetUpdate(isIndependentUpdate: true);
		yield return image.DOColor(m_wrongItemColorFlash, 0.25f).SetLoops(4, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
			.WaitForCompletion();
		image.color = Color.white;
		image.transform.localScale = Vector3.one;
		m_isItemAnimating = false;
	}

	private IEnumerator AnimateApplyItemCorrectItemCoroutine(GameObject keyObject)
	{
		m_isItemAnimating = true;
		Image image = keyObject.GetComponent<Image>();
		yield return image.transform.DOPunchScale(new Vector3(-0.25f, -0.25f, -0.25f), 0.5f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		image.transform.localScale = Vector3.one;
		m_isItemAnimating = false;
	}

	private void CloseKeyRingInput(InputAction.CallbackContext input)
	{
		CloseKeyRing();
	}

	public void CloseKeyRing()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowKeyring.Raise(null);
	}

	private void UINavigateAction(InputAction.CallbackContext input)
	{
		if (!(Time.unscaledTime - m_lastInputMoveTime < 0.1f) && !m_isMoving)
		{
			Vector2 vector = input.ReadValue<Vector2>();
			if (vector.x > 0.5f)
			{
				m_lastInputMoveTime = Time.unscaledTime;
				CycleKeyRight();
			}
			else if (vector.x < -0.5f)
			{
				m_lastInputMoveTime = Time.unscaledTime;
				CycleKeyLeft();
			}
		}
	}
}
