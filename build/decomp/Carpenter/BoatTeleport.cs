using UnityEngine;

public class BoatTeleport : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_minMax;

	[SerializeField]
	private float m_margin;

	private void Update()
	{
		CheckBoundaries();
	}

	private void CheckBoundaries()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			Transform transform = item.transform;
			if (transform.position.x > m_minMax.y + m_margin)
			{
				TeleportTo(transform, m_minMax.x + m_margin);
			}
			else if (transform.position.x < m_minMax.x - m_margin)
			{
				TeleportTo(transform, m_minMax.y - m_margin);
			}
		}
	}

	private void TeleportTo(Transform playerTransform, float targetX)
	{
		Vector3 vector2 = (playerTransform.position = new Vector3(targetX, playerTransform.position.y, playerTransform.position.z));
		Camera.main.transform.position = new Vector3(vector2.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
		GlobalReferences.Instance.EventChannels.Camera.ForceCameraReset.Raise();
	}

	private void OnDrawGizmos()
	{
		Vector2 vector = new Vector2(m_minMax.x, 10f);
		Vector2 vector2 = new Vector2(m_minMax.x, -10f);
		Vector2 vector3 = new Vector2(m_minMax.y, 10f);
		Vector2 vector4 = new Vector2(m_minMax.y, -10f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector3, vector4);
		vector.x -= m_margin;
		vector2.x -= m_margin;
		vector3.x += m_margin;
		vector4.x += m_margin;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.color = Color.white;
	}
}
