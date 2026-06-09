using System.Collections.Generic;
using UnityEngine;

public class MapLinesManager : MonoBehaviour
{
	[SerializeField]
	private LineRenderer m_lineRendererPrefab;

	private List<LineRenderer> m_activeLines = new List<LineRenderer>();

	public void ClearLines()
	{
		foreach (LineRenderer activeLine in m_activeLines)
		{
			Object.Destroy(activeLine.gameObject);
		}
		m_activeLines.Clear();
	}

	public void AddLine(MapLoreEntryIcon mapIconA, MapLoreEntryIcon mapIconB)
	{
		LineRenderer lineRenderer = Object.Instantiate(m_lineRendererPrefab, base.transform);
		Vector3[] positions = new Vector3[2] { mapIconA.MapWorldPosition, mapIconB.MapWorldPosition };
		lineRenderer.SetPositions(positions);
		Vector3 mapWorldPosition = mapIconA.MapWorldPosition;
		Vector3 mapWorldPosition2 = mapIconB.MapWorldPosition;
		mapWorldPosition.y -= 0.25f;
		mapWorldPosition2.y -= 0.25f;
		Vector3[] positions2 = new Vector3[2] { mapWorldPosition, mapWorldPosition2 };
		lineRenderer.transform.GetChild(0).GetComponent<LineRenderer>().SetPositions(positions2);
	}
}
