using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Character)]
public class MatchCharacterDirectionToVelocity : FsmStateAction
{
	protected CharacterDirection m_charDirection;

	protected CharacterMovement m_characterMovement;

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_charDirection = base.Owner.GetComponent<CharacterDirection>();
			m_characterMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnUpdate()
	{
		if (m_characterMovement.PreviousVelocity.x > 0f)
		{
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Right;
		}
		else if (m_characterMovement.PreviousVelocity.x < 0f)
		{
			m_charDirection.CurrentDirection = CharacterDirection.Facing.Left;
		}
	}
}
