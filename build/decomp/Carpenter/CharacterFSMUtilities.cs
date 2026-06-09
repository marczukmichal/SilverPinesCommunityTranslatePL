using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class CharacterFSMUtilities : MonoBehaviour
{
	[SerializeField]
	private PlayMakerFSM m_characterFSM;

	[SerializeField]
	private PlayMakerFSM m_aiFSM;

	[SerializeField]
	private StatusEffectDefinition[] m_preventSprintStatusEffects;

	[SerializeField]
	private bool m_hasParryState;

	private StatusEffectReceiver m_statusEffectReceiver;

	private CharacterHitReact m_hitReact;

	private CharacterHealth m_health;

	private CharacterStamina m_stamina;

	private CharacterMovement m_movement;

	private static readonly string s_onDeadEvent = "Dead";

	private static readonly string s_onKnockedDownEvent = "OnKnockedDown";

	private static readonly string s_onParry = "OnHit/Parry";

	public UnityAction OnHitOverride;

	public UnityAction OnDeadOverride;

	public UnityAction OnKnockedDownOverride;

	private ICharacterFSMUtilitiesHitReactOverrider m_reactOverrider;

	private bool m_isDead;

	private bool m_hasBeenHit;

	private bool m_isKnockedDown;

	private bool m_hasNewHit;

	private bool m_allowHitReactDuringKnockdown;

	public bool AllowHitReactDuringKnockdown
	{
		get
		{
			return m_allowHitReactDuringKnockdown;
		}
		set
		{
			m_allowHitReactDuringKnockdown = value;
		}
	}

	public void SetOverrideClass(ICharacterFSMUtilitiesHitReactOverrider overrider)
	{
		m_reactOverrider = overrider;
	}

	private void Awake()
	{
		m_statusEffectReceiver = GetComponent<StatusEffectReceiver>();
		m_hitReact = GetComponent<CharacterHitReact>();
		m_health = GetComponent<CharacterHealth>();
		m_stamina = GetComponent<CharacterStamina>();
		m_movement = GetComponent<CharacterMovement>();
	}

	private void Start()
	{
		if (m_health != null)
		{
			m_isDead = m_health.IsDead;
		}
	}

	private void OnEnable()
	{
		if (m_hitReact != null)
		{
			CharacterHitReact hitReact = m_hitReact;
			hitReact.OnHit = (UnityAction)Delegate.Combine(hitReact.OnHit, new UnityAction(OnHit));
		}
	}

	private void OnDisable()
	{
		if (m_hitReact != null)
		{
			CharacterHitReact hitReact = m_hitReact;
			hitReact.OnHit = (UnityAction)Delegate.Remove(hitReact.OnHit, new UnityAction(OnHit));
		}
	}

	private void OnHit()
	{
		m_hasNewHit = true;
	}

	public bool CanSprint()
	{
		if (m_statusEffectReceiver != null)
		{
			StatusEffectDefinition[] preventSprintStatusEffects = m_preventSprintStatusEffects;
			foreach (StatusEffectDefinition definition in preventSprintStatusEffects)
			{
				if (m_statusEffectReceiver.IsStatusEffectActive(definition))
				{
					return false;
				}
			}
		}
		if (m_movement != null && m_movement.AttachedStairs != null)
		{
			return false;
		}
		return true;
	}

	private void Update()
	{
		bool flag = m_health != null && m_health.IsDead;
		bool flag2 = m_hitReact != null && m_hitReact.HasBeenHit();
		bool flag3 = m_statusEffectReceiver != null && m_statusEffectReceiver.IsStatusEffectActive(GlobalReferences.Instance.StatusEffects.Generic.Knockdown);
		bool flag4 = false;
		if (flag && flag != m_isDead)
		{
			flag4 = true;
		}
		if (flag2 && flag2 != m_hasBeenHit)
		{
			flag4 = true;
		}
		if (flag3 && flag3 != m_isKnockedDown)
		{
			flag4 = true;
		}
		bool num = flag != m_isDead || flag2 != m_hasBeenHit || m_isKnockedDown != flag3 || m_hasNewHit;
		if (flag4)
		{
			if (flag)
			{
				if (OnDeadOverride != null)
				{
					OnDeadOverride();
				}
				else
				{
					m_characterFSM.SendEvent(s_onDeadEvent);
				}
			}
			else if (flag3 && !AllowHitReactDuringKnockdown)
			{
				if (OnKnockedDownOverride != null)
				{
					OnKnockedDownOverride();
				}
				else
				{
					m_characterFSM.SendEvent(s_onKnockedDownEvent);
				}
			}
			else if (flag2)
			{
				if (m_hitReact.LastHit.DamageCategory == DamageCategory.Parry && m_hasParryState)
				{
					m_characterFSM.SendEvent(s_onParry);
				}
				else
				{
					bool flag5 = false;
					if (m_reactOverrider != null)
					{
						flag5 = m_reactOverrider.HandleHitReact();
					}
					if (!flag5)
					{
						if (OnHitOverride != null)
						{
							OnHitOverride();
						}
						else
						{
							m_characterFSM.SendEvent(m_hitReact.GetHitReactEvent());
						}
					}
				}
			}
		}
		if (num)
		{
			m_isDead = flag;
			m_hasBeenHit = flag2;
			m_isKnockedDown = flag3;
			m_hasNewHit = false;
		}
	}

	public void SetVariableBoolForCharacter(string name)
	{
		m_characterFSM.FsmVariables.GetFsmBool(name).Value = true;
	}

	public void UnsetVariableBoolForCharacter(string name)
	{
		m_characterFSM.FsmVariables.GetFsmBool(name).Value = false;
	}

	public void SetVariableBoolForAI(string name)
	{
		m_aiFSM.FsmVariables.GetFsmBool(name).Value = true;
	}

	public void UnsetVariableBoolForAI(string name)
	{
		m_aiFSM.FsmVariables.GetFsmBool(name).Value = false;
	}

	public void SendEventToCharacter(string eventName)
	{
		m_characterFSM.SendEvent(eventName);
	}
}
