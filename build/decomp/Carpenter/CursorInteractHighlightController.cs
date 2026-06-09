using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorInteractHighlightController : MonoBehaviour
{
	[SerializeField]
	private GraphicRaycaster m_graphicRaycaster;

	[SerializeField]
	private int m_priority;

	private bool m_isHoveringInteractable;

	public int Priority => m_priority;

	public bool IsHoveringInteractable => m_isHoveringInteractable;

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Input.CursorInteractHighlightSet.Add(this);
		GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Input.CursorInteractHighlightSet.Remove(this);
		GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
	}

	private void Update()
	{
		bool flag = false;
		bool flag2 = false;
		if (GlobalReferences.Instance.GameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.Minigame) || GlobalReferences.Instance.GameMenuState.IsInMenuExclusively(GameMenuState.GameMenu.ApplyInteract))
		{
			flag2 = true;
		}
		flag = flag2 && Mouse.current != null && GameUtils.IsHoveringSelectable(Mouse.current.position.value, m_graphicRaycaster);
		if (flag != m_isHoveringInteractable)
		{
			m_isHoveringInteractable = flag;
			GlobalReferences.Instance.EventChannels.Input.RefreshMouseCursorState.Raise();
		}
	}
}
