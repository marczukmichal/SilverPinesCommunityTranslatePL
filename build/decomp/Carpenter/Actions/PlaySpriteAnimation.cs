using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Animation)]
public class PlaySpriteAnimation : FsmStateAction
{
	public enum AnimSpeedScalingMode
	{
		None,
		MeleeWeapon,
		MeleeGeneral,
		Sprint,
		WeaponHandling,
		RangedRateOfFire
	}

	public FsmOwnerDefault m_gameObject;

	[SerializeField]
	public AnimationClip m_animationClip;

	[UIHint(UIHint.Variable)]
	[SerializeField]
	public FsmObject m_animationVariable;

	[SerializeField]
	public float m_speed = 1f;

	[SerializeField]
	public bool m_randomStartPoint;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_trackTimeVariable;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_startTime;

	[HutongGames.PlayMaker.Tooltip("Event to send after the animation is finished.")]
	public FsmEvent m_finishEvent;

	public AnimSpeedScalingMode m_animSpeedScalingMode;

	public bool m_applyAnimNodePositionOnExit;

	private SpriteAnim m_spriteAnim;

	private AnimationEventsHelper m_animHelper;

	private CharacterEquipment m_characterMelee;

	private CharacterMovement m_movement;

	private AnimationClip m_playingClip;

	private CharacterArmAnimations m_armAnimations;

	protected virtual AnimationClip GetAnimationClip()
	{
		if (m_animationVariable != null && !m_animationVariable.IsNone && m_animationVariable.Value is AnimationClip)
		{
			return m_animationVariable.Value as AnimationClip;
		}
		return m_animationClip;
	}

	public override void OnEnter()
	{
		GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(m_gameObject);
		m_spriteAnim = ownerDefaultTarget.GetComponent<SpriteAnim>();
		m_animHelper = ownerDefaultTarget.GetComponent<AnimationEventsHelper>();
		m_characterMelee = ownerDefaultTarget.GetComponent<CharacterEquipment>();
		m_movement = ownerDefaultTarget.GetComponent<CharacterMovement>();
		m_playingClip = GetAnimationClip();
		if (m_animHelper != null)
		{
			m_animHelper.DisableMovementFromNode();
		}
		m_armAnimations = base.Owner.GetComponentInChildren<CharacterArmAnimations>(includeInactive: true);
		float num = m_speed;
		if (m_animSpeedScalingMode == AnimSpeedScalingMode.MeleeWeapon && m_characterMelee != null)
		{
			num = m_characterMelee.GetMeleeAnimationSpeed();
		}
		switch (m_animSpeedScalingMode)
		{
		case AnimSpeedScalingMode.MeleeWeapon:
		case AnimSpeedScalingMode.MeleeGeneral:
		{
			CharacterInventory component3 = ownerDefaultTarget.GetComponent<CharacterInventory>();
			if (!(component3 != null))
			{
				break;
			}
			float num2 = 1f;
			if (component3.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterMelee, out float floatValue3))
			{
				num2 += floatValue3;
			}
			if (component3.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterMeleeLowHP, out float floatValue4))
			{
				CharacterHealth component4 = ownerDefaultTarget.GetComponent<CharacterHealth>();
				if (component4 != null && component4.IsWoundedOrLower())
				{
					num2 += floatValue4;
				}
			}
			num *= num2;
			break;
		}
		case AnimSpeedScalingMode.Sprint:
		{
			CharacterInventory component2 = ownerDefaultTarget.GetComponent<CharacterInventory>();
			if (component2 != null && component2.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterSprint, out float floatValue2))
			{
				num *= 1f + floatValue2;
			}
			break;
		}
		case AnimSpeedScalingMode.WeaponHandling:
		{
			CharacterInventory component5 = ownerDefaultTarget.GetComponent<CharacterInventory>();
			if (component5 != null && component5.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.BetterWeaponHandling))
			{
				num *= 1.6f;
			}
			break;
		}
		case AnimSpeedScalingMode.RangedRateOfFire:
		{
			CharacterInventory component = ownerDefaultTarget.GetComponent<CharacterInventory>();
			if (component != null && component.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.RateOfFire, out float floatValue))
			{
				num *= 1f + floatValue;
			}
			break;
		}
		}
		m_spriteAnim.Play(m_playingClip, num);
		if (m_armAnimations != null)
		{
			m_armAnimations.Speed = num;
		}
		if (m_randomStartPoint)
		{
			m_spriteAnim.SetNormalizedTime(Random.Range(0f, 1f));
		}
		if (m_startTime != null && m_startTime.Value != 0f)
		{
			m_spriteAnim.SetTime(m_startTime.Value);
		}
		else if (m_animHelper != null)
		{
			float nextAnimStartTime = m_animHelper.GetNextAnimStartTime(m_playingClip);
			if (nextAnimStartTime != 0f)
			{
				m_spriteAnim.SetNormalizedTime(nextAnimStartTime);
			}
			m_animHelper.SetNextAnimStartTime(null, 0f);
		}
		if (m_playingClip == null)
		{
			Debug.LogWarning("PlaySpriteAnimation animation clip is null for state (" + base.State.Name + ")");
		}
		if (m_playingClip == null || m_playingClip.isLooping)
		{
			base.Fsm.Event(m_finishEvent);
			Finish();
		}
	}

	public override void OnUpdate()
	{
		if (!m_spriteAnim.IsPlaying(m_playingClip))
		{
			base.Fsm.Event(m_finishEvent);
			if (m_applyAnimNodePositionOnExit)
			{
				m_movement.ApplyMovementFromAnimRootNode();
			}
			if (m_trackTimeVariable != null)
			{
				m_trackTimeVariable.Value = m_spriteAnim.Time;
			}
			Finish();
		}
	}
}
