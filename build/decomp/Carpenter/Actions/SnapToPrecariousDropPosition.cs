using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory(ActionCategory.Movement)]
public class SnapToPrecariousDropPosition : FsmStateAction
{
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
		m_dropDetection.SnapToDetectedEdgePosition();
	}
}
