using UnityEngine;

public class FollowGameObjectTransform : MonoBehaviour
{
	[SerializeField]
	private GameObjectAnchor m_followedAnchor;

	[SerializeField]
	private Vector3 m_offset;

	private void Update()
	{
		if ((bool)m_followedAnchor.Item)
		{
			Vector3 position = m_followedAnchor.Item.transform.position;
			position += m_offset;
			base.transform.position = position;
		}
	}
}
