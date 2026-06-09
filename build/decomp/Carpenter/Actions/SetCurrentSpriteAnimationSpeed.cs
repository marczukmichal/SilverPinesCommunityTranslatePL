using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Animation)]
public class SetCurrentSpriteAnimationSpeed : FsmStateAction
{
	[SerializeField]
	public float m_speed = 1f;

	private SpriteAnim m_spriteAnim;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
		}
	}

	public override void OnEnter()
	{
		m_spriteAnim.Speed = m_speed;
	}
}
