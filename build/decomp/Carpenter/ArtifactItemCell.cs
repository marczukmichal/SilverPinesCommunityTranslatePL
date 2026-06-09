using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArtifactItemCell : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public enum State
	{
		Unknown,
		Inactive,
		Active
	}

	[SerializeField]
	private Image m_itemIcon;

	[SerializeField]
	private Image m_backgroundIcon;

	[SerializeField]
	private Image m_newHighlight;

	[Header("Icon Colors")]
	[SerializeField]
	private Color m_activeColor;

	[SerializeField]
	private Color m_inactiveColor;

	[SerializeField]
	private Color m_unknownColor;

	[Header("Background Colors")]
	[SerializeField]
	private Color m_backgroundActiveColor;

	[SerializeField]
	private Color m_backgroundInactiveColor;

	[SerializeField]
	private Color m_backgroundUnknownColor;

	[SerializeField]
	private ButtonExtended m_button;

	private ArtifactItemDefinition m_itemDefinition;

	public UnityAction<ArtifactItemCell> OnStartHoveredArtifact;

	public UnityAction<ArtifactItemCell> OnStopHoveredArtifact;

	public UnityAction<ArtifactItemCell> OnArtifactClicked;

	private State m_state;

	private ArtifactItemInstance m_artifactItemInstance;

	public ArtifactItemDefinition ItemDefinition => m_itemDefinition;

	public State CellState => m_state;

	public ArtifactItemInstance ArtifactItemInstance
	{
		get
		{
			return m_artifactItemInstance;
		}
		set
		{
			m_artifactItemInstance = value;
		}
	}

	private void Awake()
	{
		m_button.onClick.AddListener(OnClicked);
	}

	private void OnClicked()
	{
		OnArtifactClicked(this);
	}

	private void OnEnable()
	{
		UpdateVisuals();
	}

	public void SetState(State state)
	{
		if (m_state != state)
		{
			m_state = state;
			UpdateVisuals();
		}
	}

	private void UpdateVisuals()
	{
		Color color = m_unknownColor;
		Color color2 = m_backgroundUnknownColor;
		float num = 0.8f;
		switch (m_state)
		{
		case State.Active:
			color = m_activeColor;
			color2 = m_backgroundActiveColor;
			num = 1f;
			break;
		case State.Inactive:
			color = m_inactiveColor;
			color2 = m_backgroundInactiveColor;
			break;
		}
		m_itemIcon.color = color;
		m_itemIcon.transform.localScale = Vector3.one * num;
		m_backgroundIcon.color = color2;
		m_newHighlight.gameObject.SetActive(value: false);
	}

	public void SetData(ArtifactItemDefinition definition)
	{
		m_itemDefinition = definition;
		m_itemIcon.sprite = definition.InventorySprite;
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (GlobalReferences.Instance.InputState.InputMode != InputState.Mode.KeyboardMouse)
		{
			OnStartHoveredArtifact(this);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnStopHoveredArtifact(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (GlobalReferences.Instance.InputState.InputMode != InputState.Mode.Gamepad)
		{
			OnStartHoveredArtifact(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnStopHoveredArtifact(this);
	}

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
	}

	public IEnumerator DOAnimation()
	{
		if (!base.isActiveAndEnabled)
		{
			yield return new WaitUntil(() => base.isActiveAndEnabled);
		}
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < 4; i++)
		{
			m_itemIcon.color = m_inactiveColor;
			m_backgroundIcon.color = m_backgroundInactiveColor;
			m_newHighlight.gameObject.SetActive(value: true);
			yield return new WaitForSecondsRealtime(0.2f);
			m_itemIcon.color = m_unknownColor;
			m_backgroundIcon.color = m_backgroundUnknownColor;
			m_newHighlight.gameObject.SetActive(value: false);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		m_itemIcon.color = m_inactiveColor;
		m_backgroundIcon.color = m_backgroundInactiveColor;
		m_newHighlight.gameObject.SetActive(value: false);
		EventSystem.current.SetSelectedGameObject(base.gameObject);
	}
}
