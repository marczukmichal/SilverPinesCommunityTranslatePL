using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Dialogue")]
public class TriggerVoicedAudioEvent : FsmStateAction
{
	public enum Target
	{
		Player,
		GameObject
	}

	public AudioVoicedEvent m_voiceEvent;

	public Target m_target;

	public GameObject m_targetGameObject;

	[HutongGames.PlayMaker.Tooltip("Event to trigger on voiceover stopped")]
	public FsmEvent m_onVoiceEventDone;

	public override void OnEnter()
	{
		GameObject gameObject = m_targetGameObject;
		bool isPlayer = m_target == Target.Player;
		if (m_target == Target.Player)
		{
			gameObject = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		}
		gameObject.GetComponent<CharacterIdentifier>();
		GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_voiceEvent, gameObject.transform, isPlayer));
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Register(OnVoiceEventDone);
	}

	public override void OnExit()
	{
		GlobalReferences.Instance.EventChannels.Audio.OnAudioVoicedEventFinished.Unregister(OnVoiceEventDone);
	}

	private void OnVoiceEventDone()
	{
		base.Fsm.Event(m_onVoiceEventDone);
		Finish();
	}
}
