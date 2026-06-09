using UnityEngine;

public class FieldOfViewViewer : MonoBehaviour
{
	[DebugCommand("field_of_view", "Enable/disable field of view rendering", "field_of_view <true/false>", typeof(bool), true)]
	public static bool FIELD_OF_VIEW_ENABLED = true;

	[DebugCommand("field_of_view_debug", "Enable/disable field of view rendering debug", "field_of_view_debug <true/false>", typeof(bool), false)]
	public static bool FIELD_OF_VIEW_DEBUG = false;

	[SerializeField]
	private float m_timeBetweenUpdates = 0.1f;

	private float m_updateTimer;

	private void UpdateFieldOfViewAreaStates()
	{
		foreach (FieldOfViewArea item in GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet)
		{
			item.UpdateVisibilityFromViewer(this);
		}
	}

	private void Start()
	{
		UpdateFieldOfViewAreaStates();
		foreach (FieldOfViewArea item in GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet)
		{
			item.ForceUpdateAlpha();
		}
	}

	private void Update()
	{
		m_updateTimer += Time.deltaTime;
		if (m_updateTimer > m_timeBetweenUpdates)
		{
			m_updateTimer = 0f;
			UpdateFieldOfViewAreaStates();
		}
	}
}
