using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class SetBlocking : FsmStateAction
{
	private CharacterMeleeBlock m_characterMeleeBlock;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterMeleeBlock = base.Owner.GetComponent<CharacterMeleeBlock>();
		}
	}

	public override void OnEnter()
	{
		m_characterMeleeBlock.StartBlock();
		Finish();
	}

	public override void OnExit()
	{
		m_characterMeleeBlock.EndBlock();
		Finish();
	}
}
