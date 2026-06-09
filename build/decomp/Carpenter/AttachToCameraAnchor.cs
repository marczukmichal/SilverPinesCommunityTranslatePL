using UnityEngine;

public class AttachToCameraAnchor : MonoBehaviour
{
	[SerializeField]
	private CameraAnchor m_anchor;

	private void OnEnable()
	{
		m_anchor.Set(base.gameObject.GetComponent<Camera>());
	}

	private void OnDisable()
	{
		if (m_anchor.Item == base.gameObject)
		{
			m_anchor.Set(null);
		}
	}
}
