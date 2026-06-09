using UnityEngine;
using UnityEngine.Events;

public class GameCutsceneManager : MonoBehaviour
{
	private static bool m_cutsceneActive;

	public static UnityAction<bool> OnCutsceneActiveChanged;

	public static bool CutsceneActive => m_cutsceneActive;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.GameCutscene.SetGameCutsceneEnabled.Register(OnSetActive);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.GameCutscene.SetGameCutsceneEnabled.Unregister(OnSetActive);
		m_cutsceneActive = false;
	}

	private void OnSetActive(bool active)
	{
		if (m_cutsceneActive != active)
		{
			m_cutsceneActive = active;
			OnCutsceneActiveChanged?.Invoke(active);
		}
	}
}
