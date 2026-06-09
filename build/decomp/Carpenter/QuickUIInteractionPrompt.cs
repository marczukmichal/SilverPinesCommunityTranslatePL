using UnityEngine;
using UnityEngine.EventSystems;

public class QuickUIInteractionPrompt : MonoBehaviour
{
	[SerializeField]
	private GameObject m_startSelected;

	private void OnEnable()
	{
		if (m_startSelected != null)
		{
			EventSystem.current.SetSelectedGameObject(m_startSelected);
		}
	}
}
