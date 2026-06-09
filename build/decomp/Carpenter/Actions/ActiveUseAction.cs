using System;
using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory("Inventory")]
public class ActiveUseAction : FsmStateAction
{
	public FsmEvent m_completeMinigameEvent;

	public ActiveUseSettings m_settings;

	private BaseCharacterInput m_characterInput;

	private SpriteAnim m_animation;

	private CharacterArmAnimations m_armAnimation;

	private CharacterInventory m_inventory;

	private int m_doneCycles;

	private float m_activeUseTime;

	private float m_timer;

	private ActiveUseState m_activeUseState;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_characterInput = base.Owner.GetCharacterInputComponent();
			m_animation = base.Owner.GetComponent<SpriteAnim>();
			m_armAnimation = base.Owner.GetComponentInChildren<CharacterArmAnimations>();
			m_inventory = base.Owner.GetComponent<CharacterInventory>();
		}
	}

	private void ResetForNewCycle()
	{
		m_activeUseTime = 0f;
		m_timer = 0f;
		m_activeUseState = ActiveUseState.None;
	}

	public override void OnEnter()
	{
		m_doneCycles = 0;
		ResetForNewCycle();
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnReloadAction = (UnityAction)Delegate.Combine(characterInput.OnReloadAction, new UnityAction(ActiveAction));
		SendUpdateEvent(active: true);
	}

	public override void OnExit()
	{
		BaseCharacterInput characterInput = m_characterInput;
		characterInput.OnReloadAction = (UnityAction)Delegate.Remove(characterInput.OnReloadAction, new UnityAction(ActiveAction));
		SendUpdateEvent(active: false);
	}

	private Vector2 GetTimingWindow()
	{
		return m_settings.ActiveTimeWindow;
	}

	public override void OnUpdate()
	{
		_ = m_timer;
		float num = 1f;
		switch (m_activeUseState)
		{
		case ActiveUseState.Success:
			num = m_settings.ActiveUseSpeedOnSuccess;
			break;
		case ActiveUseState.Failed:
			num = m_settings.ActiveUseSpeedOnFail;
			break;
		}
		if (m_settings.ArtifactSpeedBonus == ActiveUseSettings.ArtifactSpeedBonusType.Healing && m_inventory != null && m_inventory.Inventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterHealItemUseAnimation))
		{
			num *= 2f;
		}
		m_timer += Time.deltaTime * num;
		Vector2 timingWindow = GetTimingWindow();
		if (CanDoActiveAction() && m_timer >= timingWindow.x && GlobalReferences.Instance.UserPreferences.AutomateItemInteractMinigame)
		{
			ActiveAction();
		}
		if (m_settings.ActiveUseType == ActiveUseType.Wait && m_activeUseState == ActiveUseState.None && m_timer >= timingWindow.y)
		{
			if ((bool)m_animation)
			{
				m_animation.Pause();
			}
			if ((bool)m_armAnimation)
			{
				m_armAnimation.Pause();
			}
			m_timer = timingWindow.y;
		}
		if (m_timer >= m_settings.CycleUseTime)
		{
			m_doneCycles++;
			if (m_doneCycles >= m_settings.CycleCount)
			{
				base.Fsm.Event(m_completeMinigameEvent);
				Finish();
			}
			else
			{
				ResetForNewCycle();
			}
		}
		SendUpdateEvent(active: true);
	}

	private bool CanDoActiveAction()
	{
		if (m_settings.ActiveUseType != 0)
		{
			return true;
		}
		return false;
	}

	private void SendUpdateEvent(bool active)
	{
		ActiveUseStateInfoData activeUseStateInfoData = new ActiveUseStateInfoData();
		activeUseStateInfoData.m_isActive = active;
		activeUseStateInfoData.m_canActiveInteract = CanDoActiveAction();
		if (activeUseStateInfoData.m_canActiveInteract)
		{
			Vector2 timingWindow = GetTimingWindow();
			activeUseStateInfoData.m_activeActionTimeMin = timingWindow.x / m_settings.CycleUseTime;
			activeUseStateInfoData.m_activeActiveTimeMax = timingWindow.y / m_settings.CycleUseTime;
		}
		activeUseStateInfoData.m_activeUseState = m_activeUseState;
		activeUseStateInfoData.m_progress = m_timer / m_settings.CycleUseTime;
		activeUseStateInfoData.m_activeActionTime = m_activeUseTime / m_settings.CycleUseTime;
		activeUseStateInfoData.m_isLastReloadAction = m_doneCycles == m_settings.CycleCount - 1;
		activeUseStateInfoData.m_actionViewAssetReference = m_settings.ActionViewAsset;
		activeUseStateInfoData.m_currentAmmoAmount = m_doneCycles;
		activeUseStateInfoData.m_maxAmmoCapacity = m_settings.CycleCount;
		GlobalReferences.Instance.EventChannels.Generic.UseItemActiveInfo.Raise(activeUseStateInfoData);
	}

	private void ActiveAction()
	{
		if (m_activeUseState != 0 || m_timer < 0.1f)
		{
			return;
		}
		Vector2 timingWindow = GetTimingWindow();
		if (m_settings.ActiveUseType == ActiveUseType.Wait)
		{
			if (m_timer >= timingWindow.x)
			{
				m_activeUseState = ActiveUseState.Success;
				if ((bool)m_animation)
				{
					m_animation.Resume();
				}
				if ((bool)m_armAnimation)
				{
					m_armAnimation.Resume();
				}
				m_activeUseTime = m_timer;
			}
		}
		else
		{
			if (m_timer >= timingWindow.x && m_timer <= timingWindow.y)
			{
				m_activeUseState = ActiveUseState.Success;
			}
			else
			{
				m_activeUseState = ActiveUseState.Failed;
			}
			m_activeUseTime = m_timer;
		}
	}
}
