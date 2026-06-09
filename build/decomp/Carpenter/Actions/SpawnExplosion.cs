using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SpawnExplosion : FsmStateAction
{
	[SerializeField]
	public ExplosionSettings m_explosionSettings;

	[SerializeField]
	public Transform m_explodeTransform;

	public override void OnEnter()
	{
		Explosion.SpawnExplosion(m_explosionSettings, m_explodeTransform.position, Quaternion.identity, base.Owner);
		Finish();
	}
}
