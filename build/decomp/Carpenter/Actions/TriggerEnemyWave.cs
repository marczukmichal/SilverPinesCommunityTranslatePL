using System;
using HutongGames.PlayMaker;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Utilities")]
public class TriggerEnemyWave : FsmStateAction
{
	public EnemyWave m_enemyWave;

	public FsmEvent m_waveCompleteEvent;

	public override void OnEnter()
	{
		EnemyWave enemyWave = m_enemyWave;
		enemyWave.OnWaveDone = (UnityAction)Delegate.Combine(enemyWave.OnWaveDone, new UnityAction(OnWaveDone));
		m_enemyWave.StartWave();
	}

	private void OnWaveDone()
	{
		base.Fsm.Event(m_waveCompleteEvent);
		Finish();
	}
}
