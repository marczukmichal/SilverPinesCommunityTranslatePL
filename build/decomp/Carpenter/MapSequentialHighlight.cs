using System;
using System.Collections;
using UnityEngine;

public class MapSequentialHighlight : BaseMapObjective
{
	[Serializable]
	public struct MapObjectHighlight
	{
		public GameObject m_object;

		public Map3DFloor m_floor;
	}

	[SerializeField]
	private MapObjectiveState m_objectiveState;

	[SerializeField]
	private MapObjectHighlight[] m_highlightList;

	[SerializeField]
	private float m_initialWaitDelay = 1f;

	[SerializeField]
	private float m_showHoverDelay = 1f;

	private Map3D m_map3d;

	public override void UpdateForObjectiveState(MapObjectiveState objectiveState, MapObjectiveState animate)
	{
		if (objectiveState.HasFlag(m_objectiveState))
		{
			if (animate == m_objectiveState)
			{
				base.gameObject.SetActive(value: true);
				StartCoroutine(DoHighlightList());
			}
			else
			{
				ShowStatic();
			}
		}
		else
		{
			Hide();
		}
	}

	private IEnumerator DoHighlightList()
	{
		m_map3d = GetComponentInParent<Map3D>();
		yield return new WaitForSecondsRealtime(m_initialWaitDelay);
		MapObjectHighlight[] highlightList = m_highlightList;
		for (int i = 0; i < highlightList.Length; i++)
		{
			MapObjectHighlight highlight = highlightList[i];
			yield return new WaitForEndOfFrame();
			if ((bool)highlight.m_floor)
			{
				m_map3d.SelectActiveFloor(highlight.m_floor);
			}
			yield return m_map3d.FocusCameraOnMapPositionAnimated(highlight.m_object.transform.position);
			m_map3d.IsAnimating = true;
			yield return new WaitForSecondsRealtime(m_showHoverDelay);
		}
		m_map3d.IsAnimating = false;
	}

	public void ShowStatic()
	{
		base.gameObject.SetActive(value: true);
		RefreshFloorState();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
