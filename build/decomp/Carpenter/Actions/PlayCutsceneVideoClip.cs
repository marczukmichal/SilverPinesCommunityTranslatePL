using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Video)]
public class PlayCutsceneVideoClip : FsmStateAction
{
	public VideoMetadata m_videoToPlay;

	private bool m_registered;

	public override void Awake()
	{
		_ = base.Owner == null;
	}

	~PlayCutsceneVideoClip()
	{
		if (m_registered)
		{
			GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Unregister(OnVideoStateChanged);
		}
	}

	public override void OnEnter()
	{
		if (m_videoToPlay == null || m_videoToPlay.VideoClip == null)
		{
			Debug.LogError("PlayCutsceneVideoClip: failed to play video because of missing or broken VideoMetadata");
			Finish();
		}
		else if (GameDebugCommands.SKIP_STORY)
		{
			Finish();
		}
		else
		{
			GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayVideoCutscene.Raise(m_videoToPlay);
			GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Register(OnVideoStateChanged);
			m_registered = true;
		}
	}

	public override void OnExit()
	{
		GlobalReferences.Instance.EventChannels.VideoCutscenes.PlayingVideoCutsceneChanged.Unregister(OnVideoStateChanged);
		m_registered = false;
	}

	private void OnVideoStateChanged(bool isPlaying)
	{
		if (!isPlaying)
		{
			Finish();
		}
	}
}
