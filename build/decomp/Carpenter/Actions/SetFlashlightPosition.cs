using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class SetFlashlightPosition : FsmStateAction
{
	public PlayerFlashlight.FlashlightPosition m_position;

	public float m_lerpTime;

	public bool m_onEnter;

	public bool m_onExit;

	public bool m_resetOnExit;

	protected PlayerFlashlight m_flashlight;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_flashlight = base.Owner.GetComponentInChildren<PlayerFlashlight>();
		}
	}

	public override void OnEnter()
	{
		if (m_onEnter)
		{
			m_flashlight.SetFlashlightPosition(m_position, m_lerpTime);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (m_onExit)
		{
			m_flashlight.SetFlashlightPosition(m_position, m_lerpTime);
		}
		if (m_resetOnExit)
		{
			m_flashlight.SetFlashlightPosition(PlayerFlashlight.FlashlightPosition.Normal, m_lerpTime);
		}
	}
}
