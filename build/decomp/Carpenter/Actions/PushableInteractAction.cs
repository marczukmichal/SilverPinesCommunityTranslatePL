using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory("Interactor")]
public class PushableInteractAction : BaseCharacterHorizontalMovementAction
{
	private enum AnimState
	{
		None,
		Idle,
		Pushing,
		Pulling
	}

	public FsmEvent m_doneEvent;

	public AnimationClip m_pushAnimationClip;

	public AnimationClip m_pullAnimationClip;

	public AnimationClip m_idleAnimationClip;

	private CharacterInteractor m_interactor;

	private SpriteAnim m_spriteAnim;

	private PushableInteract m_pushableInteract;

	private AnimState m_animState;

	private void SetNewAnimState(AnimState state)
	{
		if (m_animState != state)
		{
			m_animState = state;
			switch (m_animState)
			{
			case AnimState.Idle:
				m_spriteAnim.Play(m_idleAnimationClip);
				break;
			case AnimState.Pushing:
				m_spriteAnim.Play(m_pushAnimationClip);
				break;
			case AnimState.Pulling:
				m_spriteAnim.Play(m_pullAnimationClip);
				break;
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
			m_spriteAnim = base.Owner.GetComponentInChildren<SpriteAnim>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_pushableInteract = m_interactor.ActiveInteractable as PushableInteract;
		m_pushableInteract.IsUsed = true;
		m_spriteAnim.Play(m_idleAnimationClip);
		m_animState = AnimState.Idle;
		if (m_pushableInteract == null)
		{
			EndPush();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_pushableInteract.PushableObject.SetMoveSpeed(0f);
		m_pushableInteract.PushableObject.DisableMovement();
		m_pushableInteract.IsUsed = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_pushableInteract.PushableObject.LockedInPlace)
		{
			EndPush();
			return;
		}
		AnimState newAnimState = AnimState.Idle;
		if (m_input.MovementInput.x > 0.5f)
		{
			bool flag = m_direction.CurrentDirection == CharacterDirection.Facing.Left;
			if (flag && !m_pushableInteract.IsPullBlocked())
			{
				if (m_pushableInteract.IsPullBlocked())
				{
					m_pushableInteract.PushableObject.SetMoveSpeed(0f);
				}
				else
				{
					m_pushableInteract.PushableObject.SetMoveSpeed(0f - m_pushableInteract.PushVelocity);
					newAnimState = AnimState.Pulling;
				}
			}
			else if (!flag)
			{
				m_pushableInteract.PushableObject.SetMoveSpeed(0f - m_pushableInteract.PushVelocity);
				newAnimState = AnimState.Pushing;
			}
		}
		else if (m_input.MovementInput.x < -0.5f)
		{
			bool flag = m_direction.CurrentDirection == CharacterDirection.Facing.Right;
			if (flag)
			{
				if (m_pushableInteract.IsPullBlocked())
				{
					m_pushableInteract.PushableObject.SetMoveSpeed(0f);
				}
				else
				{
					m_pushableInteract.PushableObject.SetMoveSpeed(m_pushableInteract.PushVelocity);
					newAnimState = AnimState.Pulling;
				}
			}
			else if (!flag)
			{
				m_pushableInteract.PushableObject.SetMoveSpeed(m_pushableInteract.PushVelocity);
				newAnimState = AnimState.Pushing;
			}
		}
		else
		{
			m_pushableInteract.PushableObject.TrySlowDown(m_pushableInteract.PushVelocity);
		}
		SetNewAnimState(newAnimState);
		if (m_pushableInteract.PushableObject.CheckForForcedExit(m_pushableInteract.PushVelocity + 0.1f))
		{
			EndPush();
		}
	}

	protected override float CalculateXVelocity()
	{
		Vector3 position = m_interactor.transform.position;
		position.x = m_pushableInteract.transform.position.x;
		position += m_direction.GetForwardVector() * -0.5f;
		return (position - m_interactor.transform.position).x / Time.fixedDeltaTime;
	}

	private void EndPush()
	{
		base.Fsm.Event(m_doneEvent);
		Finish();
	}
}
