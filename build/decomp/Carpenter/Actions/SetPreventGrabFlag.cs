using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Sync Grab")]
public class SetPreventGrabFlag : FsmStateAction
{
	private CharacterSyncGrab m_characterSyncGrab;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterSyncGrab = base.Owner.GetComponent<CharacterSyncGrab>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_characterSyncGrab.SetPreventGrabFlag(preventGrabs: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		m_characterSyncGrab.SetPreventGrabFlag(preventGrabs: false);
	}
}
