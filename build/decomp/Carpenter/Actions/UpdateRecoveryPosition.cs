using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class UpdateRecoveryPosition : FsmStateAction
{
	public bool m_onUpdate;

	protected CharacterRecoveryPosition m_recoveryPosition;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_recoveryPosition = base.Owner.GetComponent<CharacterRecoveryPosition>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_recoveryPosition.TryToUpdateRecoveryPosition();
	}

	public override void OnUpdate()
	{
		if (m_onUpdate)
		{
			m_recoveryPosition.TryToUpdateRecoveryPosition();
		}
	}
}
