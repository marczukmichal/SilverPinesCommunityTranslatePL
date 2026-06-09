using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputQuickMap : FsmStateAction
{
	[Tooltip("Event for viewing map")]
	public FsmEvent m_onViewingQuickMapEvent;

	[Tooltip("Event for not viewing map")]
	public FsmEvent m_onNotViewingQuickMapEvent;

	private BaseCharacterInput m_characterInput;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnUpdate()
	{
	}
}
