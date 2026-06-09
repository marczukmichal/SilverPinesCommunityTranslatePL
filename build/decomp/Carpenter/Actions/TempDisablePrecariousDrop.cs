using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class TempDisablePrecariousDrop : FsmStateAction
{
	public float m_disableDuration = 2f;

	protected CharacterPrecariousDropDetection m_dropDetection;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_dropDetection = base.Owner.GetComponent<CharacterPrecariousDropDetection>();
		}
	}

	public override void OnEnter()
	{
		m_dropDetection.TempDisable(m_disableDuration);
	}

	public override void OnExit()
	{
		m_dropDetection.TempDisable(m_disableDuration);
	}
}
