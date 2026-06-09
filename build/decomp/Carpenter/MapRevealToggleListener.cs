using UnityEngine;

public class MapRevealToggleListener : MonoBehaviour
{
	[SerializeField]
	private MapRevealPickup m_pickup;

	[SerializeField]
	private MeshRenderer m_renderer;

	private void Reset()
	{
		m_renderer = GetComponent<MeshRenderer>();
	}

	private void OnEnable()
	{
		bool flag = GlobalReferences.Instance.MapDynamicData.HasPickedUpMapRevealPickup(m_pickup);
		if (m_renderer != null)
		{
			m_renderer.enabled = flag;
		}
	}
}
