using UnityEngine;
using UnityEngine.Events;

public class PlayerQuickMapControls : MonoBehaviour
{
	[SerializeField]
	private ProgressionVariable m_mapDisabledVariable;

	private CharacterInputPlayer m_input;

	private bool m_isShowingMap;

	public UnityAction<bool> m_onQuickMapActiveChanged;

	public bool ShowingMap
	{
		get
		{
			return m_isShowingMap;
		}
		set
		{
			if (m_isShowingMap != value)
			{
				m_isShowingMap = value;
				GlobalReferences.Instance.EventChannels.Map.ShowQuickMap.Raise(m_isShowingMap);
				m_onQuickMapActiveChanged?.Invoke(value);
			}
		}
	}

	public void Awake()
	{
		m_input = GetComponent<CharacterInputPlayer>();
	}

	private bool IsMapEnabled()
	{
		return !m_mapDisabledVariable.Value;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Register(Hide);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.InGameMenu.ShowMapMenuTab.Unregister(Hide);
		ShowingMap = false;
	}

	private void Hide()
	{
		ShowingMap = false;
		m_input.ClearQuickMapInput();
	}

	private void Update()
	{
		if (IsMapEnabled())
		{
			ShowingMap = m_input.IsUsingQuickMap;
		}
	}
}
