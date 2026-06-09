using TMPro;
using UnityEngine;

public class BuildInfoDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private bool m_dontDestroyOnLoad;

	private static BuildInfoDisplay m_instance;

	private static bool m_hideBuildInfo;

	[DebugCommand("build_info_disabled", "Disables build info display", "build_info_disabled <true/false>", typeof(bool), false)]
	protected static bool UIDisabled
	{
		get
		{
			return m_hideBuildInfo;
		}
		set
		{
			if (m_hideBuildInfo != value)
			{
				if (value)
				{
					m_hideBuildInfo = true;
				}
				else
				{
					m_hideBuildInfo = false;
				}
				BuildInfoDisplay[] array = Object.FindObjectsByType<BuildInfoDisplay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
				for (int i = 0; i < array.Length; i++)
				{
					array[i].gameObject.SetActive(!m_hideBuildInfo);
				}
			}
		}
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		m_instance = this;
		_ = Application.isEditor;
		if (m_dontDestroyOnLoad)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		m_text.text = BuildInfo.VerboseString;
		if (0 == 0)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
