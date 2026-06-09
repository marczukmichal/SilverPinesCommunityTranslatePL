using UnityEngine;

public class AttachToGameObjectAnchor : MonoBehaviour
{
	[SerializeField]
	private GameObjectAnchor m_anchor;

	private void OnEnable()
	{
		m_anchor.Set(base.gameObject);
	}

	private void OnDisable()
	{
		if (m_anchor.Item == base.gameObject)
		{
			m_anchor.Set(null);
		}
	}
}
