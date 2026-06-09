using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Playables;

public class ComicPanel : MonoBehaviour
{
	[SerializeField]
	private EventReference m_audioEvent;

	private Tween m_tween;

	private PlayableDirector m_director;

	private Comic m_comic;

	private EventInstance m_audioEventInstance;

	private bool m_isShowing;

	[DebugCommand("comic_always_wait", "Never auto progress comic", "comic_always_wait <true/false>", typeof(bool), false)]
	public static bool COMIC_ALWAYS_WAIT;

	public void Init()
	{
		GetComponent<CanvasGroup>().alpha = 0f;
		base.gameObject.SetActive(value: false);
		m_director = GetComponent<PlayableDirector>();
		m_comic = GetComponentInParent<Comic>();
	}

	public void Show()
	{
		if (!m_isShowing)
		{
			m_isShowing = true;
			base.gameObject.SetActive(value: true);
			if (m_tween != null)
			{
				m_tween.Kill();
			}
			m_tween = GetComponent<CanvasGroup>().DOFade(1f, 0.3f);
			if (m_director != null)
			{
				m_director.Play();
			}
			if (!m_audioEvent.IsNull)
			{
				m_audioEventInstance = RuntimeManager.CreateInstance(m_audioEvent);
				m_audioEventInstance.start();
			}
		}
	}

	public void Hide()
	{
		if (m_isShowing)
		{
			if (m_tween != null)
			{
				m_tween.Kill();
			}
			m_tween = GetComponent<CanvasGroup>().DOFade(0f, 1f).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
			m_isShowing = false;
		}
	}

	public void Skip(bool allowFadeout = true)
	{
		if (m_tween != null)
		{
			m_tween.Complete();
		}
		if (m_director != null)
		{
			m_director.time = m_director.duration;
		}
		if (m_audioEventInstance.isValid())
		{
			m_audioEventInstance.stop((!allowFadeout) ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	public bool IsActive()
	{
		if (COMIC_ALWAYS_WAIT)
		{
			return true;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (m_tween != null)
		{
			flag = m_tween.IsActive();
		}
		if (m_director != null)
		{
			flag2 = m_director.state == PlayState.Playing;
		}
		if (m_audioEventInstance.isValid())
		{
			m_audioEventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.PLAYING)
			{
				flag3 = true;
			}
		}
		bool flag4 = !m_audioEventInstance.isValid();
		return flag || flag2 || flag3 || flag4;
	}

	public void KillAudio()
	{
		if (m_audioEventInstance.isValid())
		{
			m_audioEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			m_audioEventInstance.release();
		}
	}

	public bool HasActiveSequence()
	{
		if (m_director != null)
		{
			return m_director.state == PlayState.Playing;
		}
		return false;
	}

	private void OnDisable()
	{
		if (m_tween != null)
		{
			m_tween.Kill();
			m_tween = null;
		}
		if (m_director != null)
		{
			m_director.time = 0.0;
			m_director.Stop();
		}
		GetComponent<CanvasGroup>().alpha = 0f;
		m_isShowing = false;
		if (m_audioEventInstance.isValid())
		{
			m_audioEventInstance.release();
		}
	}
}
