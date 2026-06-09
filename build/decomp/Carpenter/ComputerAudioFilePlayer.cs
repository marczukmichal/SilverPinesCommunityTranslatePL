using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComputerAudioFilePlayer : MonoBehaviour, IFileViewer
{
	private enum State
	{
		Stopped,
		Playing,
		Paused
	}

	[SerializeField]
	private AudioSource m_audioSource;

	[SerializeField]
	private Slider m_playSlider;

	[SerializeField]
	private TextMeshProUGUI m_currentTime;

	[SerializeField]
	private TextMeshProUGUI m_totalTime;

	private AudioClip m_audioClip;

	private bool m_ignoreValueChanged;

	private State m_audioState;

	private State AudioState
	{
		get
		{
			return m_audioState;
		}
		set
		{
			if (m_audioState == value)
			{
				return;
			}
			bool num = m_audioState == State.Playing;
			bool flag = value == State.Playing;
			if (num != flag)
			{
				if (flag)
				{
					GetComponent<ComputerWindow>().AttachedDesktop.AddProcess();
				}
				else
				{
					GetComponent<ComputerWindow>().AttachedDesktop.RemoveProcess();
				}
			}
			switch (value)
			{
			case State.Stopped:
				SetTime(m_currentTime, 0f);
				SetSliderValue(0f);
				m_audioSource.Stop();
				break;
			case State.Paused:
				m_audioSource.Pause();
				break;
			case State.Playing:
				if (AudioState == State.Paused)
				{
					m_audioSource.UnPause();
					break;
				}
				m_audioSource.clip = m_audioClip;
				m_audioSource.time = 0f;
				m_audioSource.Play();
				break;
			}
			m_audioState = value;
		}
	}

	private void Start()
	{
		PlayAudio();
	}

	private void OnDisable()
	{
		AudioState = State.Stopped;
	}

	public void PlayAudio()
	{
		if (AudioState != State.Playing)
		{
			AudioState = State.Playing;
		}
	}

	public void PauseAudio()
	{
		if (AudioState != State.Paused)
		{
			AudioState = State.Paused;
		}
	}

	public void StopAudio()
	{
		if (AudioState != 0)
		{
			AudioState = State.Stopped;
		}
	}

	public void Update()
	{
		if (AudioState == State.Playing)
		{
			if (!m_audioSource.isPlaying)
			{
				AudioState = State.Stopped;
				return;
			}
			SetTime(m_currentTime, m_audioSource.time);
			SetSliderValue(m_audioSource.time / m_audioSource.clip.length);
		}
	}

	public void AttachFile(ComputerFile file)
	{
		ComputerFileAudio computerFileAudio = file as ComputerFileAudio;
		if ((bool)computerFileAudio)
		{
			m_audioClip = computerFileAudio.Audio;
			SetTime(m_totalTime, m_audioClip.length);
		}
	}

	public int GetMemoryUsage()
	{
		return 20480 + ((AudioState == State.Playing || AudioState == State.Paused) ? 40960 : 0);
	}

	private void SetTime(TextMeshProUGUI text, float seconds)
	{
		text.text = string.Concat(str2: ((int)(seconds % 60f)).ToString("00"), str0: ((int)(seconds / 60f)).ToString("0"), str1: ":");
	}

	private void SetSliderValue(float newValue)
	{
		m_ignoreValueChanged = true;
		m_playSlider.value = newValue;
		m_ignoreValueChanged = false;
	}

	public void SetAudioValueFromSlider(float newValue)
	{
		if (!m_ignoreValueChanged)
		{
			if (AudioState != State.Playing)
			{
				AudioState = State.Playing;
			}
			m_audioSource.time = newValue * m_audioSource.clip.length;
		}
	}
}
