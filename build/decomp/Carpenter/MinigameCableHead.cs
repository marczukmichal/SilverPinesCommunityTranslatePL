using System;
using System.Collections;
using Maro.UILineDrawer;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class MinigameCableHead : BaseMinigameHoldInteract, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public int m_plugIndex;
	}

	[Header("Movement")]
	[SerializeField]
	private float m_gamepadMovementSpeed = 10f;

	[SerializeField]
	private float m_mouseMovementSpeed = 1f;

	[Header("Input Prompt Settings")]
	[SerializeField]
	private LocalizedString m_moveInputPromptString = new LocalizedString("InteractPrompts", "Minigame_Move");

	[Header("Cable Settings")]
	[SerializeField]
	private MinigameCablesController m_controller;

	[SerializeField]
	private RectTransform m_anchor;

	[SerializeField]
	private float m_damping = 5f;

	[SerializeField]
	private float m_gravity = 500f;

	[SerializeField]
	private float m_maxLength = 100f;

	[SerializeField]
	private float m_forceMultiplier = 2000f;

	[SerializeField]
	private AnimationCurve m_forceByDistance;

	[Header("Cable Visuals")]
	[SerializeField]
	private RectTransform m_parentTransform;

	[SerializeField]
	private UILineDrawer m_lineTextureRenderer;

	[SerializeField]
	private int m_pointCount = 5;

	[SerializeField]
	private float m_slack = 1f;

	[Header("Plugs")]
	[SerializeField]
	private RectTransform[] m_plugs;

	[SerializeField]
	private float m_plugConnectDistance = 25f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_pickupAudio;

	[SerializeField]
	private AudioEvent m_pluginAudio;

	[SerializeField]
	private AudioEvent m_pullOutAudio;

	private RectTransform m_rectTransform;

	private BezierKnot2D[] m_pointsArray;

	private Vector2 m_velocity;

	private int m_plugIndex = -1;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public int PlugIndex => m_plugIndex;

	public bool IsHeld => m_interacting;

	private void Start()
	{
		m_rectTransform = GetComponent<RectTransform>();
		m_pointsArray = new BezierKnot2D[m_pointCount];
		if (m_plugIndex == -1)
		{
			ResetState();
		}
	}

	public void ResetState()
	{
		m_plugIndex = -1;
		if (m_persistentData != null)
		{
			m_persistentData.m_plugIndex = m_plugIndex;
		}
		if (m_rectTransform == null)
		{
			m_rectTransform = GetComponent<RectTransform>();
		}
		m_rectTransform.anchoredPosition = new Vector2(0f, 0f - m_maxLength);
		m_velocity = Vector2.zero;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lineTextureRenderer.gameObject.SetActive(value: true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_lineTextureRenderer.gameObject.SetActive(value: false);
	}

	protected override void OnHoldStart()
	{
		base.OnHoldStart();
		if (m_plugIndex != -1)
		{
			m_plugIndex = -1;
			if (m_persistentData != null)
			{
				m_persistentData.m_plugIndex = m_plugIndex;
			}
			if (m_pullOutAudio != null)
			{
				m_pullOutAudio.Play2D();
			}
		}
		else if (m_pickupAudio != null)
		{
			m_pickupAudio.Play2D();
		}
		m_parentTransform.SetAsLastSibling();
	}

	protected override void OnHoldEnd()
	{
		base.OnHoldEnd();
		for (int i = 0; i < m_plugs.Length; i++)
		{
			if (Vector2.Distance(base.transform.position, m_plugs[i].position) < m_plugs[i].rect.width * m_plugConnectDistance && m_controller.CanPlugInto(i))
			{
				m_plugIndex = i;
				if (m_persistentData != null)
				{
					m_persistentData.m_plugIndex = m_plugIndex;
				}
				base.transform.position = m_plugs[i].position;
				if (m_pluginAudio != null)
				{
					m_pluginAudio.Play2D();
				}
				break;
			}
		}
	}

	protected override Vector2 GetMouseRestorePosition()
	{
		return RectTransformUtility.WorldToScreenPoint(null, base.transform.position);
	}

	protected override void OnHoldingInput()
	{
		Vector2 zero = Vector2.zero;
		if (GlobalReferences.Instance.InputState.InputMode == InputState.Mode.Gamepad)
		{
			zero += Gamepad.current.leftStick.value * m_gamepadMovementSpeed;
		}
		else
		{
			zero += Mouse.current.delta.value * m_mouseMovementSpeed;
		}
		if (Mathf.Abs(zero.magnitude) > Mathf.Epsilon)
		{
			m_rectTransform.anchoredPosition += zero;
		}
	}

	private void UpdateLineRendering()
	{
		Vector2 vector = m_rectTransform.localPosition;
		Vector2 vector2 = m_anchor.localPosition;
		m_pointsArray[0] = new BezierKnot2D(vector);
		for (int i = 1; i < m_pointCount - 1; i++)
		{
			float num = (float)i / (float)(m_pointCount - 1);
			Vector3 vector3 = Vector3.Lerp(vector, vector2, num);
			float num2 = (math.cosh(num * 2f - 1f) - 1.54f) * 2f;
			vector3.y += num2 * m_slack;
			m_pointsArray[i] = new BezierKnot2D((Vector2)vector3);
		}
		m_pointsArray[m_pointCount - 1] = new BezierKnot2D(vector2);
		m_lineTextureRenderer.UpdatePoints(m_pointsArray);
	}

	protected override void Update()
	{
		base.Update();
		UpdateLineRendering();
		if (!m_interacting && m_plugIndex == -1)
		{
			Vector2 anchoredPosition = m_rectTransform.anchoredPosition;
			Vector2 vector = m_anchor.anchoredPosition - anchoredPosition;
			float magnitude = vector.magnitude;
			Vector2 obj = ((magnitude > 0.001f) ? (vector / magnitude) : Vector2.zero);
			float time = Mathf.Clamp01(magnitude / m_maxLength);
			float num = m_forceByDistance.Evaluate(time);
			Vector2 vector2 = obj * num * m_forceMultiplier;
			Vector2 vector3 = Vector2.down * m_gravity;
			Vector2 vector4 = vector2 + vector3;
			vector4 -= m_velocity * m_damping;
			m_velocity += vector4 * Time.deltaTime;
			anchoredPosition += m_velocity * Time.deltaTime;
			m_rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public override void PopulateCustomInputs(ref MinigameHoldCustomInputPrompt action1, ref MinigameHoldCustomInputPrompt action2)
	{
		if (m_interacting)
		{
			action1.m_inputAction = GameInputManager.GameInputActions.Minigame.Move2D;
			action1.m_inputString = m_moveInputPromptString;
		}
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	private IEnumerator DelayedInit()
	{
		yield return new WaitForEndOfFrame();
		if (m_plugIndex != -1)
		{
			base.transform.position = m_plugs[m_plugIndex].position;
		}
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			m_plugIndex = m_persistentData.m_plugIndex;
			StartCoroutine(DelayedInit());
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentData.m_plugIndex = m_plugIndex;
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
