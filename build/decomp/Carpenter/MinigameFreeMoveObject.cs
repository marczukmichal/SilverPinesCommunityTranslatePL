using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MinigameFreeMoveObject : BaseMinigameHoldInteract, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public Vector2 m_anchoredPosition;

		public float m_rotation;
	}

	[Header("Movement")]
	[SerializeField]
	private float m_gamepadMovementSpeed = 10f;

	[SerializeField]
	private float m_mouseMovementSpeed = 1f;

	[Header("Rotation")]
	[SerializeField]
	private bool m_rotationEnabled = true;

	[SerializeField]
	private float m_gamepadRotationSpeed = 1f;

	[SerializeField]
	private float m_mouseRotationSpeed = 0.25f;

	[Header("Settings")]
	[SerializeField]
	private float m_alphaHitTestMinimumThreshold;

	[SerializeField]
	private Vector2 m_matchCenterOffset;

	[Header("Boundaries")]
	[SerializeField]
	private RectTransform m_boundingRectTransform;

	[Header("Input Prompt Settings")]
	[SerializeField]
	private LocalizedString m_moveInputPromptString = new LocalizedString("InteractPrompts", "Minigame_Move");

	[SerializeField]
	private LocalizedString m_rotateInputPromptString = new LocalizedString("InteractPrompts", "Minigame_Rotate");

	private RectTransform m_rectTransform;

	public UnityAction<MinigameFreeMoveObject> OnMovedCallback;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public Vector2 MatchCenterPosition => base.transform.TransformPoint(m_matchCenterOffset);

	public float Rotation => base.transform.rotation.eulerAngles.z;

	private void Start()
	{
		m_rectTransform = GetComponent<RectTransform>();
		Image component = GetComponent<Image>();
		if (component != null && m_alphaHitTestMinimumThreshold > 0f)
		{
			component.alphaHitTestMinimumThreshold = m_alphaHitTestMinimumThreshold;
		}
	}

	protected override void OnHoldStart()
	{
		base.OnHoldStart();
		base.transform.SetAsLastSibling();
	}

	protected override Vector2 GetMouseRestorePosition()
	{
		return RectTransformUtility.WorldToScreenPoint(null, base.transform.position);
	}

	protected override void OnHoldingInput()
	{
		if (m_rotationEnabled && GameInputManager.GameInputActions.UI.RotateSelected.IsPressed())
		{
			float num = 0f;
			num = ((GlobalReferences.Instance.InputState.InputMode != InputState.Mode.Gamepad) ? (num + Mouse.current.delta.value.x / (float)Screen.width * m_mouseRotationSpeed) : (num + Gamepad.current.leftStick.value.x * m_gamepadRotationSpeed));
			if (Mathf.Abs(num) > Mathf.Epsilon)
			{
				m_rectTransform.Rotate(0f, 0f, num);
				OnMovedCallback?.Invoke(this);
				if (m_persistentData != null)
				{
					m_persistentData.m_rotation = m_rectTransform.eulerAngles.z;
				}
			}
			return;
		}
		Vector2 zero = Vector2.zero;
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			zero += Gamepad.current.leftStick.value * m_gamepadMovementSpeed;
		}
		else
		{
			zero += Mouse.current.delta.value * m_mouseMovementSpeed;
		}
		if (!(Mathf.Abs(zero.magnitude) > Mathf.Epsilon))
		{
			return;
		}
		m_rectTransform.anchoredPosition += zero;
		if (m_boundingRectTransform != null)
		{
			Vector3[] array = new Vector3[4];
			m_boundingRectTransform.GetWorldCorners(array);
			Vector2 vector = m_rectTransform.position;
			Vector3 zero2 = Vector3.zero;
			if (vector.x < array[0].x)
			{
				zero2.x = array[0].x - vector.x;
			}
			if (vector.x > array[2].x)
			{
				zero2.x = array[2].x - vector.x;
			}
			if (vector.y < array[0].y)
			{
				zero2.y = array[0].y - vector.y;
			}
			if (vector.y > array[1].y)
			{
				zero2.y = array[1].y - vector.y;
			}
			m_rectTransform.position += zero2;
		}
		if (m_persistentData != null)
		{
			m_persistentData.m_anchoredPosition = m_rectTransform.anchoredPosition;
		}
		OnMovedCallback?.Invoke(this);
	}

	private void OnDrawGizmos()
	{
		Vector3 center = MatchCenterPosition;
		center.z = base.transform.position.z;
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(center, 3f);
		Gizmos.color = Color.white;
	}

	public override void PopulateCustomInputs(ref MinigameHoldCustomInputPrompt action1, ref MinigameHoldCustomInputPrompt action2)
	{
		if (m_interacting)
		{
			action1.m_inputAction = GameInputManager.GameInputActions.Minigame.Move2D;
			action1.m_inputString = m_moveInputPromptString;
			if (m_rotationEnabled)
			{
				action2.m_inputAction = GameInputManager.GameInputActions.Minigame.RotateObjectHold;
				action2.m_inputString = m_rotateInputPromptString;
			}
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			RectTransform component = GetComponent<RectTransform>();
			component.anchoredPosition = m_persistentData.m_anchoredPosition;
			component.rotation = Quaternion.Euler(0f, 0f, m_persistentData.m_rotation);
		}
		else
		{
			m_persistentData = new PersistentData();
			RectTransform component2 = GetComponent<RectTransform>();
			m_persistentData.m_anchoredPosition = component2.anchoredPosition;
			m_persistentData.m_rotation = component2.rotation.eulerAngles.z;
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
