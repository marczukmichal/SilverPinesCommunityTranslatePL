using UnityEngine;
using UnityEngine.UI;

public class UIMinigameHighlight : MonoBehaviour
{
	[SerializeField]
	private Material m_selectedMaterial;

	[SerializeField]
	private Material m_canBeSelectedMaterial;

	[SerializeField]
	private Material m_normalMaterial;

	[SerializeField]
	private Graphic m_graphic;

	private void Reset()
	{
		m_graphic = GetComponent<Graphic>();
	}

	private void OnEnable()
	{
		m_graphic.material = m_canBeSelectedMaterial;
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Register(OnGameObjectHighlight);
	}

	private void OnDisable()
	{
		m_graphic.material = m_normalMaterial;
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Unregister(OnGameObjectHighlight);
	}

	public void SetSelected(bool selected)
	{
		GlobalReferences.Instance.EventChannels.Minigames.MinigameHighlightInteract.Raise(selected ? base.gameObject : null);
	}

	private void OnGameObjectHighlight(GameObject highlightObject)
	{
		if (highlightObject == null)
		{
			m_graphic.material = m_canBeSelectedMaterial;
		}
		else if (highlightObject == base.gameObject)
		{
			m_graphic.material = m_selectedMaterial;
		}
		else
		{
			m_graphic.material = m_normalMaterial;
		}
	}
}
