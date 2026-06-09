using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Event Channels")]
public class RaiseLevelTransitionEventChannel : FsmStateAction
{
	public LevelMetadata m_levelMetadata;

	public bool m_ignoreFade;

	public override void OnEnter()
	{
		LevelTransitionEventData value = new LevelTransitionEventData
		{
			m_targetSceneAssetReference = m_levelMetadata.TargetSceneAssetReference,
			m_type = LevelTransitionEventType.Transition,
			m_fadeType = (m_ignoreFade ? LevelTransitionFadeType.Ignore : LevelTransitionFadeType.Normal),
			m_levelMetadata = m_levelMetadata
		};
		GlobalReferences.Instance.EventChannels.LevelTransition.LevelTransition.Raise(value);
	}
}
