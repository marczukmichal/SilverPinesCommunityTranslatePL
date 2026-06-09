using HutongGames.PlayMaker;

[ActionCategory(ActionCategory.Movement)]
public class TurnCharacterMovementAction : ForcedCharacterMovementAction
{
	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	protected override float CalculateXVelocity()
	{
		if (m_interactor != null && m_interactor.IsInteracting)
		{
			return 0f;
		}
		return base.CalculateXVelocity();
	}
}
