using DG.Tweening;
using UnityEngine;

public class ComicPage : MonoBehaviour
{
	private ComicPanel[] m_panels;

	private Tween m_tween;

	private int m_activePanelIndex;

	public int ActivePanelIndex
	{
		get
		{
			return m_activePanelIndex;
		}
		set
		{
			if (value < m_activePanelIndex && m_activePanelIndex >= 0 && m_activePanelIndex < m_panels.Length)
			{
				m_panels[m_activePanelIndex].Hide();
			}
			m_activePanelIndex = value;
			for (int i = 0; i < m_panels.Length; i++)
			{
				if (i <= value)
				{
					m_panels[i].Show();
				}
			}
		}
	}

	public ComicPanel CurrentPanel => m_panels[m_activePanelIndex];

	public void Init()
	{
		m_activePanelIndex = -1;
		m_panels = GetComponentsInChildren<ComicPanel>(includeInactive: true);
		for (int i = 0; i < m_panels.Length; i++)
		{
			m_panels[i].Init();
		}
		base.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		if (m_tween != null)
		{
			m_tween.Kill();
		}
		base.gameObject.SetActive(value: true);
		GetComponent<CanvasGroup>().alpha = 1f;
	}

	public void EnableAllPanels()
	{
		ActivePanelIndex = m_panels.Length - 1;
	}

	public void Hide()
	{
		if (m_tween != null)
		{
			m_tween.Kill();
		}
		m_tween = GetComponent<CanvasGroup>().DOFade(0f, 0.3f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	public void Skip()
	{
		if (m_tween != null)
		{
			m_tween.Complete();
		}
		if (CurrentPanel != null)
		{
			CurrentPanel.Skip();
		}
	}

	public bool IsAnimating()
	{
		bool flag = false;
		bool flag2 = false;
		if (m_tween != null)
		{
			flag = m_tween.IsActive();
		}
		if (CurrentPanel != null)
		{
			flag2 = CurrentPanel.IsActive();
		}
		return flag || flag2;
	}

	private void OnDisable()
	{
		if (m_tween != null)
		{
			m_tween.Kill();
			m_tween = null;
		}
	}

	public bool HasMorePanels()
	{
		return ActivePanelIndex < m_panels.Length - 1;
	}

	public void Progress()
	{
		if (CurrentPanel.HasActiveSequence())
		{
			CurrentPanel.Skip();
			return;
		}
		int num = ActivePanelIndex + 1;
		if (num < m_panels.Length)
		{
			ActivePanelIndex = num;
		}
	}

	public void KillAudio()
	{
		if (CurrentPanel != null)
		{
			CurrentPanel.KillAudio();
		}
	}

	public void ProgressToPanel(string panelName)
	{
		int num = -1;
		for (int i = 0; i < m_panels.Length; i++)
		{
			if (m_panels[i].name.Equals(panelName))
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			ActivePanelIndex = Mathf.Max(ActivePanelIndex, num);
			return;
		}
		Debug.LogWarning("Comic Panel progression timeline event could not find matching page for " + panelName + " - just proceeding to next panel");
		Progress();
	}
}
