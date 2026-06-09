using UnityEngine;

public class InventoryStashPanel : MonoBehaviour
{
	[SerializeField]
	private InteractableAnchor m_itemBoxInteractableAnchor;

	[SerializeField]
	private GameObject m_container;

	private void OnEnable()
	{
		bool active = false;
		if (m_itemBoxInteractableAnchor.Item != null)
		{
			active = true;
		}
		m_container.SetActive(active);
	}
}
