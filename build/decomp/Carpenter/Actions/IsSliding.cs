using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class IsSliding : FsmStateAction
{
	protected CharacterMovement m_charMovement;

	[Tooltip("Event to trigger on sliding")]
	public FsmEvent m_isSlidingEvent;

	[Tooltip("Event to trigger on not sliding")]
	public FsmEvent m_notSlidingEvent;

	public float m_requiredTime;

	private float m_notSlidingTime;

	private float m_slidingTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_charMovement = base.Owner.GetComponent<CharacterMovement>();
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_ = m_charMovement != null;
	}
}
