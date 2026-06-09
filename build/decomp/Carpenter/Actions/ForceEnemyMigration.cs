using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Combat")]
public class ForceEnemyMigration : FsmStateAction
{
	private MigratableEnemy m_enemyMigration;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_enemyMigration = base.Owner.GetComponent<MigratableEnemy>();
		}
	}

	public override void OnEnter()
	{
		m_enemyMigration.ForceMigration = true;
		Finish();
	}
}
