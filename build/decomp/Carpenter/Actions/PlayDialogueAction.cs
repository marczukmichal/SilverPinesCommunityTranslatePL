using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Dialogue")]
public class PlayDialogueAction : FsmStateAction
{
	public TriggerDialogueEventChannel m_triggerDialogueEventChannel;

	public BoolGameEventChannel m_dialogueEnterExitEventChannel;

	public Dialogue m_dialogue;

	private bool m_registered;

	public override void Awake()
	{
		_ = base.Owner == null;
	}

	~PlayDialogueAction()
	{
		if (m_registered)
		{
			m_dialogueEnterExitEventChannel.Unregister(OnDialogueDone);
		}
	}

	public override void OnEnter()
	{
		if (GameDebugCommands.SKIP_STORY)
		{
			Finish();
			return;
		}
		m_triggerDialogueEventChannel.Raise(m_dialogue);
		m_dialogueEnterExitEventChannel.Register(OnDialogueDone);
		m_registered = true;
	}

	public override void OnExit()
	{
		m_dialogueEnterExitEventChannel.Unregister(OnDialogueDone);
		m_registered = false;
	}

	private void OnDialogueDone(bool isDialogue)
	{
		if (!isDialogue)
		{
			Finish();
		}
	}
}
