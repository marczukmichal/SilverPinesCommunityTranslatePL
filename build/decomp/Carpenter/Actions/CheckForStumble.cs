using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class CheckForStumble : FsmStateAction
{
	public FsmEvent m_stumbleEvent;

	public FsmEvent m_stumbleFallEvent;

	private CharacterStumble m_stumble;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_stumble = base.Owner.GetComponent<CharacterStumble>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
