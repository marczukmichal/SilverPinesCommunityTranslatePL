using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Audio)]
public class SetMusicOverride : FsmStateAction
{
	[SerializeField]
	public MusicSettings m_musicSettings;

	[SerializeField]
	public bool m_resetOnExit;

	public override void OnEnter()
	{
		GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(m_musicSettings);
		Finish();
	}

	public override void OnExit()
	{
		if (m_resetOnExit)
		{
			GlobalReferences.Instance.EventChannels.Audio.PlayMusicOverride.Raise(null);
		}
	}
}
