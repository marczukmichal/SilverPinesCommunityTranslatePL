using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("World Map")]
public class SetWorldMapAnimating : FsmStateAction
{
	public bool m_enable;

	protected Map3D m_map3d;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_map3d = base.Owner.GetComponentInParent<Map3D>();
		}
	}

	public override void OnEnter()
	{
		m_map3d.IsAnimating = m_enable;
		Finish();
	}
}
