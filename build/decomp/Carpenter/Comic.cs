using System;
using System.Runtime.InteropServices;
using AOT;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

[ExecuteAlways]
public class Comic : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential)]
	private class TimelineInfo
	{
		public StringWrapper LastMarker;
	}

	[SerializeField]
	private MusicSettings m_musicSettings;

	[SerializeField]
	private EventReference m_startFmodEvent;

	[SerializeField]
	private EventReference m_endFmodEvent;

	private TimelineInfo m_timelineInfo;

	private GCHandle m_timelineHandle;

	private EVENT_CALLBACK m_callbacks;

	private EventInstance m_audioInstance;

	private ComicController m_controller;

	private void Awake()
	{
		if (Application.isPlaying)
		{
			m_controller = GetComponentInParent<ComicController>();
		}
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			GameObject obj = new GameObject("!!ComicEditorSettings!!");
			obj.transform.SetParent(base.transform);
			obj.hideFlags = HideFlags.HideAndDontSave;
			obj.AddComponent<ComicEditorHelpScript>();
			return;
		}
		if (m_musicSettings != null)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(m_musicSettings);
		}
		if (!m_startFmodEvent.IsNull)
		{
			m_timelineInfo = new TimelineInfo();
			m_callbacks = EventCallbacks;
			m_audioInstance = RuntimeManager.CreateInstance(m_startFmodEvent);
			m_timelineHandle = GCHandle.Alloc(m_timelineInfo, GCHandleType.Pinned);
			m_audioInstance.setUserData(GCHandle.ToIntPtr(m_timelineHandle));
			m_audioInstance.setCallback(m_callbacks, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
			m_audioInstance.start();
		}
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			if (m_audioInstance.isValid())
			{
				m_audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				m_audioInstance.release();
			}
			GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(null);
			if (!m_endFmodEvent.IsNull)
			{
				RuntimeManager.PlayOneShot(m_endFmodEvent);
			}
		}
	}

	public void SkipToNextPage()
	{
		if (m_audioInstance.isValid())
		{
			m_audioInstance.setParameterByName("PageTurn", 1f);
		}
	}

	[MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
	private RESULT EventCallbacks(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
	{
		IntPtr userdata;
		RESULT userData = new EventInstance(instancePtr).getUserData(out userdata);
		if (userData != 0)
		{
			UnityEngine.Debug.LogError("Timeline Callback error: " + userData);
		}
		else if (userdata != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(userdata);
			TimelineInfo timelineInfo = (TimelineInfo)gCHandle.Target;
			switch (type)
			{
			case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
			{
				TIMELINE_MARKER_PROPERTIES tIMELINE_MARKER_PROPERTIES = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
				timelineInfo.LastMarker = tIMELINE_MARKER_PROPERTIES.name;
				if (m_controller != null)
				{
					m_controller.AudioTimelineEvent(tIMELINE_MARKER_PROPERTIES.name);
				}
				break;
			}
			case EVENT_CALLBACK_TYPE.DESTROYED:
				gCHandle.Free();
				break;
			}
		}
		return RESULT.OK;
	}
}
