using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MapScriptRingHighlight : BaseMapObjective
{
	[SerializeField]
	private SpriteRenderer m_circleHighlight;

	[SerializeField]
	private TextMeshPro m_text;

	[SerializeField]
	private bool m_disableCameraFocus;

	[SerializeField]
	private MapObjectiveState m_objectiveState;

	[SerializeField]
	private Map3DFloor m_floor;

	[Range(0.1f, 5f)]
	[SerializeField]
	private float m_initialWaitDelay = 1f;

	[SerializeField]
	private AudioEvent m_audioEvent;

	private Map3D m_map3d;

	private IEnumerator AnimationCoroutine()
	{
		m_map3d = GetComponentInParent<Map3D>();
		m_map3d.IsAnimating = true;
		if (m_circleHighlight != null)
		{
			Color color = m_circleHighlight.color;
			color.a = 0f;
			m_circleHighlight.color = color;
		}
		if (m_text != null)
		{
			m_text.maxVisibleCharacters = 0;
		}
		yield return new WaitForEndOfFrame();
		if (m_floor != null)
		{
			m_map3d.SelectActiveFloor(m_floor);
		}
		yield return new WaitForSecondsRealtime(m_initialWaitDelay);
		if (!m_disableCameraFocus)
		{
			yield return m_map3d.FocusCameraOnMapPositionAnimated(base.transform.position);
			m_map3d.IsAnimating = true;
			yield return new WaitForSecondsRealtime(0.5f);
		}
		if (m_audioEvent != null)
		{
			m_audioEvent.Play2D();
		}
		if (m_circleHighlight != null)
		{
			yield return m_circleHighlight.DOFade(1f, 1f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		}
		if (m_text != null)
		{
			int charCount = 0;
			while (charCount < m_text.text.Length)
			{
				charCount = (m_text.maxVisibleCharacters = charCount + 1);
				yield return new WaitForSeconds(0.01f + Random.Range(0f, 0.01f));
			}
		}
		m_map3d.IsAnimating = false;
	}

	public override void UpdateForObjectiveState(MapObjectiveState objectiveState, MapObjectiveState animate)
	{
		if (objectiveState.HasFlag(m_objectiveState))
		{
			if (animate == m_objectiveState)
			{
				ShowAnimation();
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

	public void ShowAnimation()
	{
		base.gameObject.SetActive(value: true);
		StartCoroutine(AnimationCoroutine());
		RefreshFloorState();
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
