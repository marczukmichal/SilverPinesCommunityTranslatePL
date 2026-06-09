using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Video)]
public class PlayCutsceneComic : FsmStateAction
{
	public ComicMetadata m_comicToPlay;

	private bool m_registered;

	public override void Awake()
	{
		_ = base.Owner == null;
	}

	~PlayCutsceneComic()
	{
		if (m_registered)
		{
			GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Unregister(OnComicStateChanged);
		}
	}

	public override void OnEnter()
	{
		if (GameDebugCommands.SKIP_STORY)
		{
			Finish();
			return;
		}
		GlobalReferences.Instance.EventChannels.Comic.PlayComic.Raise(m_comicToPlay);
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Register(OnComicStateChanged);
		m_registered = true;
	}

	public override void OnExit()
	{
		GlobalReferences.Instance.EventChannels.Comic.ToggleComicViewActive.Unregister(OnComicStateChanged);
		m_registered = false;
	}

	private void OnComicStateChanged(bool isPlaying)
	{
		if (!isPlaying)
		{
			Finish();
		}
	}
}
