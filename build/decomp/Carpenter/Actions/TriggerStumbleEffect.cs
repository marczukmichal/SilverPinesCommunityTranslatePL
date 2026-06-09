using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class TriggerStumbleEffect : FsmStateAction
{
	private CharacterStumble m_stumble;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_stumble = base.Owner.GetComponent<CharacterStumble>();
		}
	}

	public override void OnEnter()
	{
		m_stumble.ApplyStumbeSeperationToOthers();
		Finish();
	}
}
