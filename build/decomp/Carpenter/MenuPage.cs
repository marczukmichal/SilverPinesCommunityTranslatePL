using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPage : MonoBehaviour
{
	private enum AnimType
	{
		CanvasGroup,
		MovePivotLeft,
		MovePivotRight
	}

	[SerializeField]
	private bool m_startActive;

	[SerializeField]
	private MenuPageEventChannel m_requestPage;

	[SerializeField]
	private float m_animTime = 0.2f;

	[SerializeField]
	private GameObject m_selectOnOpen;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_onShowPageAudioEvent;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_availableInputs;

	[SerializeField]
	private AnimType m_animType;

	private CanvasGroup m_canvasGroup;

	private bool m_showing;

	private RectTransform m_rectTransform;

	public float AnimTime => m_animTime;

	public virtual MenuInputPrompts.AvaialbleInput[] AvailableInputs => m_availableInputs;

	public bool IsShowing => m_showing;

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (m_canvasGroup == null)
			{
				m_canvasGroup = GetComponent<CanvasGroup>();
			}
			return m_canvasGroup;
		}
	}

	private RectTransform LocalRectTransform
	{
		get
		{
			if (m_rectTransform == null)
			{
				m_rectTransform = GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	protected virtual void Awake()
	{
		if (!m_showing)
		{
			if (m_startActive)
			{
				m_showing = false;
				Show(instant: false, onAwake: true);
			}
			else
			{
				m_showing = true;
				Hide(instant: true);
			}
		}
	}

	private void KillTweens()
	{
		DOTween.Kill(CanvasGroup);
		DOTween.Kill(LocalRectTransform);
	}

	public virtual void Show(bool instant = false, bool onAwake = false)
	{
		if (m_showing)
		{
			return;
		}
		m_showing = true;
		base.gameObject.SetActive(value: true);
		CanvasGroup.alpha = 0f;
		CanvasGroup.interactable = true;
		CanvasGroup.blocksRaycasts = true;
		KillTweens();
		switch (m_animType)
		{
		case AnimType.CanvasGroup:
			if (instant)
			{
				CanvasGroup.alpha = 1f;
			}
			else
			{
				CanvasGroup.DOFade(1f, m_animTime);
			}
			break;
		case AnimType.MovePivotRight:
			if (instant)
			{
				LocalRectTransform.pivot = new Vector2(1f, LocalRectTransform.pivot.y);
			}
			else
			{
				LocalRectTransform.DOPivotX(1f, m_animTime);
			}
			break;
		case AnimType.MovePivotLeft:
			if (instant)
			{
				LocalRectTransform.pivot = new Vector2(-0.1f, LocalRectTransform.pivot.y);
			}
			else
			{
				LocalRectTransform.DOPivotX(-0.1f, m_animTime);
			}
			break;
		}
		SelectDefaultSelectable();
		if (m_onShowPageAudioEvent != null && !onAwake)
		{
			AudioEvent.Play2D(m_onShowPageAudioEvent);
		}
	}

	public virtual void SelectDefaultSelectable()
	{
		GameObject gameObject = null;
		if (m_selectOnOpen != null)
		{
			gameObject = m_selectOnOpen;
		}
		else
		{
			Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(includeInactive: false);
			Selectable selectable = null;
			Selectable[] array = componentsInChildren;
			foreach (Selectable selectable2 in array)
			{
				if (selectable2.navigation.mode != 0)
				{
					selectable = selectable2;
					break;
				}
			}
			if (selectable != null)
			{
				gameObject = selectable.gameObject;
			}
		}
		if (gameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(gameObject);
		}
	}

	public virtual void Hide(bool instant = false)
	{
		if (instant)
		{
			m_startActive = false;
		}
		if (!m_showing)
		{
			return;
		}
		KillTweens();
		m_showing = false;
		if (instant)
		{
			base.gameObject.SetActive(value: false);
			switch (m_animType)
			{
			case AnimType.CanvasGroup:
				CanvasGroup.alpha = 0f;
				break;
			case AnimType.MovePivotRight:
				LocalRectTransform.pivot = new Vector2(-0.1f, LocalRectTransform.pivot.y);
				break;
			case AnimType.MovePivotLeft:
				LocalRectTransform.pivot = new Vector2(1f, LocalRectTransform.pivot.y);
				break;
			}
		}
		else
		{
			switch (m_animType)
			{
			case AnimType.CanvasGroup:
				CanvasGroup.DOFade(0f, m_animTime).OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
				});
				break;
			case AnimType.MovePivotRight:
				LocalRectTransform.DOPivotX(-0.1f, m_animTime).OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
				});
				break;
			case AnimType.MovePivotLeft:
				LocalRectTransform.DOPivotX(1f, m_animTime).OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
				});
				break;
			}
		}
		CanvasGroup.interactable = false;
		CanvasGroup.blocksRaycasts = false;
	}

	public void RequestOpen()
	{
		m_requestPage.Raise(this);
	}

	public virtual bool CanExitPage()
	{
		return true;
	}

	public virtual bool OnBackInput()
	{
		return false;
	}

	public virtual bool CanChangeTabs()
	{
		return true;
	}
}
