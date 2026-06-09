using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SetupDeadPhysics : FsmStateAction
{
	private CharacterMovement m_characterMovement;

	private Rigidbody2D m_rigidbody2D;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterMovement = base.Owner.GetComponent<CharacterMovement>();
			m_rigidbody2D = base.Owner.GetComponent<Rigidbody2D>();
		}
	}

	public override void OnEnter()
	{
		if (m_characterMovement != null)
		{
			m_characterMovement.enabled = false;
		}
		if (m_rigidbody2D != null)
		{
			m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
		}
		Finish();
	}
}
