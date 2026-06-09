using UnityEngine;

public class MapDetailMarker : MonoBehaviour
{
	[SerializeField]
	private MapDetailSettings m_settings;

	public MapDetailSettings Settings => m_settings;
}
