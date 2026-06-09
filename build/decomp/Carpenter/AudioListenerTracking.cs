using UnityEngine;

public class AudioListenerTracking : MonoBehaviour
{
	[SerializeField]
	private GameObjectAnchor m_audioListenerPositionAnchor;

	private void Update()
	{
		if (m_audioListenerPositionAnchor != null && m_audioListenerPositionAnchor.Item != null)
		{
			base.transform.position = m_audioListenerPositionAnchor.Item.transform.position;
		}
	}
}
