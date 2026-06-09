using FMODUnity;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Audio)]
public class SetBossMusicPhase : FsmStateAction
{
	public enum BossMusicPhase
	{
		Phase1,
		Phase2
	}

	[SerializeField]
	public BossMusicPhase m_phase;

	public override void OnEnter()
	{
		string text = "";
		switch (m_phase)
		{
		case BossMusicPhase.Phase1:
			text = "event:/Parameter Events/BossPhase1ControlEvent";
			break;
		case BossMusicPhase.Phase2:
			text = "event:/Parameter Events/BossPhase2ControlEvent";
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			RuntimeManager.PlayOneShot(text);
		}
	}
}
