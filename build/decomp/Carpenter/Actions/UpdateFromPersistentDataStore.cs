using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("PersistentData")]
public class UpdateFromPersistentDataStore : FsmStateAction
{
	private PersistentDataIdentifier m_persistentDataIdentifier;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_persistentDataIdentifier = base.Owner.GetComponent<PersistentDataIdentifier>();
		}
	}

	public override void OnEnter()
	{
		m_persistentDataIdentifier.UpdateFromDatastore();
		Finish();
	}
}
