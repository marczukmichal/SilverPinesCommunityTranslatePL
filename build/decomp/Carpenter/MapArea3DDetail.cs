using UnityEngine;

public class MapArea3DDetail : MonoBehaviour
{
	private enum MapDetailRotationMode
	{
		None,
		Basic,
		Stairs
	}

	[SerializeField]
	private float m_anchorOffset;

	[SerializeField]
	private Transform m_childAnchor;

	[SerializeField]
	private MapDetailRotationMode m_rotationMode;

	public void Configure(MapDetailSettings settings)
	{
		if (!(m_childAnchor != null))
		{
			return;
		}
		Vector3 localPosition = m_childAnchor.transform.localPosition;
		Quaternion localRotation = Quaternion.identity;
		switch (settings.PositionAnchor)
		{
		case MapDetailPositioning.FarWall:
			localPosition.z -= m_anchorOffset;
			if (m_rotationMode == MapDetailRotationMode.Stairs)
			{
				localRotation = Quaternion.Euler(0f, 90f, 0f);
			}
			break;
		case MapDetailPositioning.CloseWall:
			localPosition.z += m_anchorOffset;
			if (m_rotationMode == MapDetailRotationMode.Stairs)
			{
				localRotation = Quaternion.Euler(0f, 90f, 0f);
			}
			break;
		}
		m_childAnchor.transform.localPosition = localPosition;
		if (m_rotationMode != 0)
		{
			m_childAnchor.transform.localRotation = localRotation;
		}
	}

	public void SetVisible(bool visible)
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = visible;
		}
	}
}
