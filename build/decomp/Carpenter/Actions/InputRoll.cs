using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Input)]
public class InputRoll : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("Event to trigger on roll")]
	public FsmEvent m_onRollEvent;

	[SerializeField]
	public float m_cooldownTime;

	private BaseCharacterInput m_characterInput;

	private float m_lastRollTime;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	public override void OnUpdate()
	{
		Check();
	}

	private void Check()
	{
		if (!(m_lastRollTime + m_cooldownTime > Time.time) && m_onRollEvent != null && m_characterInput.IsCrouching)
		{
			base.Fsm.Event(m_onRollEvent);
			m_lastRollTime = Time.time;
		}
	}
}
