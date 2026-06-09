using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Melee Move Set")]
public class MeleeMoveSetSettings : ScriptableObject
{
	[SerializeField]
	private string m_fsmEventOnAttack;

	[SerializeField]
	private string m_fsmEventOnAttackCrouched;

	[SerializeField]
	private string m_fsmEventOnBlock;

	[SerializeField]
	private string m_fsmEventOnSprintAttack;

	[SerializeField]
	private AnimationClip[] m_chargeStatesAnimations;

	public string FSMEventOnAttack => m_fsmEventOnAttack;

	public string FSMEventOnAttackCrouched => m_fsmEventOnAttackCrouched;

	public string FSMEventOnBlock => m_fsmEventOnBlock;

	public string FSMEventOnAttackSprinting => m_fsmEventOnSprintAttack;

	public AnimationClip GetAnimationForChargeState(int index)
	{
		if (index < m_chargeStatesAnimations.Length)
		{
			return m_chargeStatesAnimations[index];
		}
		return null;
	}
}
