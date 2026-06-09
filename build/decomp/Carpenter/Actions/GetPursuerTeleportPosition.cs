using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("AI")]
public class GetPursuerTeleportPosition : FsmStateAction
{
	[RequiredField]
	[UIHint(UIHint.Variable)]
	[HutongGames.PlayMaker.Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

	public FsmFloat m_forwardDistance;

	public FsmEvent m_failEvent;

	private AISenses m_aiSenses;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_aiSenses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		Check();
	}

	private void Check()
	{
		if (!m_aiSenses)
		{
			return;
		}
		bool flag = false;
		Vector3 position = base.Owner.transform.position;
		if (m_aiSenses.CurrentTarget != null)
		{
			CharacterDirection component = m_aiSenses.CurrentTarget.GetComponent<CharacterDirection>();
			if (component != null)
			{
				Vector3 forwardVector = component.GetForwardVector();
				Vector3 vector = m_aiSenses.CurrentTarget.transform.position + forwardVector * m_forwardDistance.Value;
				vector.y += 2f;
				Vector2 vector2 = new Vector2(2f, 2f);
				RaycastHit2D raycastHit2D = Physics2D.BoxCast(vector, vector2, 0f, Vector2.down, 3f, GameLayers.CharacterNavigationMask);
				if ((bool)raycastHit2D.collider)
				{
					Vector2 point = raycastHit2D.point;
					point.y += vector2.y * 0.5f;
					if (Physics2D.OverlapCapsule(point, vector2 * 0.9f, CapsuleDirection2D.Vertical, 0f, GameLayers.CharacterNavigationMask) == null)
					{
						position.x = raycastHit2D.point.x;
						position.y = raycastHit2D.point.y + 0.2f;
						flag = true;
					}
				}
			}
		}
		m_vector3Variable.Value = position;
		if (!flag)
		{
			base.Fsm.Event(m_failEvent);
		}
	}
}
