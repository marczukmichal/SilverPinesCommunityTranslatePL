using System;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class CharacterReloading : MonoBehaviour
{
	public enum ReloadState
	{
		None,
		Start,
		Active,
		End
	}

	[SerializeField]
	private SpriteAnim m_animation;

	[SerializeField]
	private CharacterArmAnimations m_armAnimation;

	private CharacterInventory m_inventory;

	private float m_reloadTimer;

	private float m_activeReloadTime;

	private BaseCharacterInput m_input;

	private CharacterAiming m_characterAiming;

	private ProjectileWeaponItemInstance m_reloadWeapon;

	private bool m_isCrouching;

	private bool m_isReloadQueued;

	private float m_reloadQueuedClearTimer;

	private ActiveUseState m_activeReloadState;

	private ReloadState m_reloadState;

	public UnityAction OnRequestedCharacterReload;

	public UnityAction OnReloadDone;

	private bool m_isLastReloadAction;

	private ReloadAnimations ReloadAnimations
	{
		get
		{
			if (m_isCrouching)
			{
				return m_reloadWeapon.WeaponSettings.CrouchedReloadAnimations;
			}
			return m_reloadWeapon.WeaponSettings.StandingReloadAnimations;
		}
	}

	public ActiveUseState ActiveReloadStatus => m_activeReloadState;

	private ReloadState State
	{
		get
		{
			return m_reloadState;
		}
		set
		{
			if (m_reloadState == value)
			{
				return;
			}
			m_reloadState = value;
			switch (m_reloadState)
			{
			case ReloadState.Start:
				if (ReloadAnimations.StartReloadAnimation != null)
				{
					m_animation.Play(ReloadAnimations.StartReloadAnimation);
				}
				m_armAnimation.Play(ReloadAnimations.StartReloadArmAnimation);
				break;
			case ReloadState.Active:
				StartReloadInstance();
				break;
			case ReloadState.End:
				if (ReloadAnimations.FinishReloadAnimation != null)
				{
					m_animation.Play(ReloadAnimations.FinishReloadAnimation);
				}
				m_armAnimation.Play(ReloadAnimations.FinishReloadArmAnimation);
				break;
			case ReloadState.None:
				OnReloadDone?.Invoke();
				m_armAnimation.Play(null);
				break;
			}
		}
	}

	public bool IsReloading => m_reloadState != ReloadState.None;

	private void Awake()
	{
		m_inventory = base.gameObject.GetComponent<CharacterInventory>();
		m_input = base.gameObject.GetCharacterInputComponent();
		m_characterAiming = GetComponent<CharacterAiming>();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.RequestCharacterReloadWeapon.Register(OnRequestCharacterReload);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.RequestCharacterReloadWeapon.Unregister(OnRequestCharacterReload);
	}

	public bool CanReload()
	{
		if (!IsReloading)
		{
			return m_inventory.Inventory.CanReloadEquippedItem();
		}
		return false;
	}

	public bool CanActiveReload()
	{
		if (m_reloadWeapon.WeaponSettings.ActiveReloadType != 0)
		{
			return true;
		}
		return false;
	}

	private void StartReloadInstance()
	{
		m_activeReloadTime = 0f;
		m_reloadTimer = 0f;
		m_activeReloadState = ActiveUseState.None;
		int num = m_reloadWeapon.WeaponSettings.AutoReloadAmount;
		if (num == -1)
		{
			num = m_reloadWeapon.GetMaxAmmoCapacity();
		}
		num = Mathf.Min(num, m_inventory.Inventory.GetAvailableAmmoAmount(m_reloadWeapon.LoadedAmmoType));
		m_isLastReloadAction = m_reloadWeapon.WeaponSettings.AutoReloadAmount == -1 || m_reloadWeapon.WeaponSettings.AutoReloadAmount >= m_reloadWeapon.GetEmptyAmmoCapacity() || num == m_inventory.Inventory.GetAvailableAmmoAmount(m_reloadWeapon.LoadedAmmoType);
		if (ReloadAnimations.MainReloadAnimation != null)
		{
			m_animation.Play(ReloadAnimations.MainReloadAnimation);
		}
		m_armAnimation.Play(ReloadAnimations.MainReloadArmAnimation);
		SendActiveReloadInfoUpdate(reloading: true);
	}

	public void StartReload(bool isCrouching)
	{
		m_isCrouching = isCrouching;
		ItemInstance equippedRangedItem = m_inventory.Inventory.EquippedRangedItem;
		if (CanReload() && equippedRangedItem != null && equippedRangedItem is ProjectileWeaponItemInstance projectileWeaponItemInstance)
		{
			m_reloadWeapon = projectileWeaponItemInstance;
			if (ReloadAnimations.StartReloadAnimation != null)
			{
				State = ReloadState.Start;
			}
			else
			{
				State = ReloadState.Active;
			}
			if (CanActiveReload())
			{
				BaseCharacterInput input = m_input;
				input.OnReloadAction = (UnityAction)Delegate.Combine(input.OnReloadAction, new UnityAction(ActiveReloadAction));
			}
			if (m_reloadWeapon.WeaponSettings.ReloadStartAudioEvent != null)
			{
				m_reloadWeapon.WeaponSettings.ReloadStartAudioEvent.Play(base.transform.position);
			}
			if (projectileWeaponItemInstance.WeaponSettings.ShellCasingEjectionType == ShellCasingEjectionType.OnReload && m_characterAiming.Weapon != null)
			{
				m_characterAiming.Weapon.EjectShellCasingsForReload();
			}
			projectileWeaponItemInstance.ResetFiredCount();
		}
		else
		{
			Debug.LogError("Abort reload - not in a valid state to start a reload!");
		}
	}

	public void CancelReload()
	{
		State = ReloadState.None;
		if (CanActiveReload())
		{
			BaseCharacterInput input = m_input;
			input.OnReloadAction = (UnityAction)Delegate.Remove(input.OnReloadAction, new UnityAction(ActiveReloadAction));
		}
		SendActiveReloadInfoUpdate(reloading: false);
	}

	private void ActiveReloadAction()
	{
		if (m_reloadState != ReloadState.Active || m_activeReloadState != 0 || m_reloadTimer < 0.1f)
		{
			return;
		}
		Vector2 reloadWindow = GetReloadWindow();
		if (m_reloadWeapon.WeaponSettings.ActiveReloadType == ActiveUseType.Wait)
		{
			if (m_reloadTimer >= reloadWindow.x)
			{
				m_activeReloadState = ActiveUseState.Success;
				m_animation.Resume();
				m_armAnimation.Resume();
				m_activeReloadTime = m_reloadTimer;
			}
		}
		else
		{
			if (m_reloadTimer >= reloadWindow.x && m_reloadTimer <= reloadWindow.y)
			{
				float num2 = (m_armAnimation.Speed = (m_animation.Speed = m_reloadWeapon.WeaponSettings.ActiveReloadAnimationSpeedOnSuccess));
				m_activeReloadState = ActiveUseState.Success;
			}
			else
			{
				float num2 = (m_armAnimation.Speed = (m_animation.Speed = m_reloadWeapon.WeaponSettings.ActiveReloadAnimationSpeedOnFailure));
				m_activeReloadState = ActiveUseState.Failed;
			}
			m_activeReloadTime = m_reloadTimer;
		}
	}

	private void Update()
	{
		if (m_isReloadQueued)
		{
			m_reloadQueuedClearTimer -= Time.deltaTime;
			if (m_reloadQueuedClearTimer <= 0f)
			{
				m_isReloadQueued = false;
			}
		}
		switch (m_reloadState)
		{
		case ReloadState.Start:
			if (!m_animation.IsPlaying())
			{
				State = ReloadState.Active;
			}
			break;
		case ReloadState.Active:
		{
			float reloadTimer = m_reloadTimer;
			float num = 1f;
			switch (m_activeReloadState)
			{
			case ActiveUseState.Success:
				num = m_reloadWeapon.WeaponSettings.ActiveReloadSpeedOnSuccess;
				break;
			case ActiveUseState.Failed:
				num = m_reloadWeapon.WeaponSettings.ActiveReloadSpeedOnFail;
				break;
			}
			if (m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.FasterReload, out float floatValue))
			{
				num *= 1f + floatValue;
			}
			m_reloadTimer += Time.deltaTime * num;
			Vector2 reloadWindow = GetReloadWindow();
			if (CanActiveReload() && m_reloadTimer >= reloadWindow.x && GlobalReferences.Instance.UserPreferences.AutomateItemInteractMinigame)
			{
				ActiveReloadAction();
			}
			if (m_reloadWeapon.WeaponSettings.ActiveReloadType == ActiveUseType.Wait && m_activeReloadState == ActiveUseState.None && m_reloadTimer >= reloadWindow.y)
			{
				m_animation.Pause();
				m_armAnimation.Pause();
				m_reloadTimer = reloadWindow.y;
			}
			if (m_reloadTimer >= m_reloadWeapon.WeaponSettings.AddAmmoReloadTime && reloadTimer < m_reloadWeapon.WeaponSettings.AddAmmoReloadTime)
			{
				m_inventory.Inventory.AutoReloadWeapon(m_reloadWeapon);
				if (m_characterAiming.Weapon != null)
				{
					m_characterAiming.Weapon.EjectMagazine();
				}
				GlobalReferences.Instance.EventChannels.Generic.WeaponInfo.Raise(new WeaponInfoData
				{
					m_updateType = WeaponInfoData.UpdateType.Reload,
					m_currentAmmoCount = ((m_reloadWeapon != null) ? m_reloadWeapon.AmmoCount : 0),
					m_maxAmmoCount = ((m_reloadWeapon != null) ? m_reloadWeapon.GetMaxAmmoCapacity() : 0),
					m_weaponInstance = m_reloadWeapon
				});
				if (m_reloadWeapon.WeaponSettings.ReloadAudioEvent != null)
				{
					m_reloadWeapon.WeaponSettings.ReloadAudioEvent.Play(base.transform.position);
				}
			}
			if (m_reloadTimer >= m_reloadWeapon.WeaponSettings.ReloadAnimationTime)
			{
				if (m_inventory.Inventory.CanReloadEquippedItem())
				{
					StartReloadInstance();
					SendActiveReloadInfoUpdate(reloading: true);
					break;
				}
				if (ReloadAnimations.FinishReloadAnimation != null)
				{
					State = ReloadState.End;
				}
				else
				{
					State = ReloadState.None;
				}
				if (CanActiveReload())
				{
					BaseCharacterInput input = m_input;
					input.OnReloadAction = (UnityAction)Delegate.Remove(input.OnReloadAction, new UnityAction(ActiveReloadAction));
				}
				if (m_reloadWeapon.WeaponSettings.ReloadCompleteAudioEvent != null)
				{
					m_reloadWeapon.WeaponSettings.ReloadCompleteAudioEvent.Play(base.transform.position);
				}
				SendActiveReloadInfoUpdate(reloading: false);
			}
			else
			{
				SendActiveReloadInfoUpdate(reloading: true);
			}
			break;
		}
		case ReloadState.End:
			if (!m_animation.IsPlaying())
			{
				State = ReloadState.None;
			}
			break;
		}
	}

	private void SendActiveReloadInfoUpdate(bool reloading)
	{
		GlobalReferences.Instance.EventChannels.Generic.WeaponInfo.Raise(new WeaponInfoData
		{
			m_updateType = WeaponInfoData.UpdateType.Reload,
			m_currentAmmoCount = ((m_reloadWeapon != null) ? m_reloadWeapon.AmmoCount : 0),
			m_maxAmmoCount = ((m_reloadWeapon != null) ? m_reloadWeapon.GetMaxAmmoCapacity() : 0),
			m_weaponInstance = m_reloadWeapon
		});
		ActiveUseStateInfoData activeUseStateInfoData = new ActiveUseStateInfoData
		{
			m_isActive = reloading
		};
		if (reloading)
		{
			activeUseStateInfoData.m_canActiveInteract = CanActiveReload();
			if (activeUseStateInfoData.m_canActiveInteract)
			{
				Vector2 reloadWindow = GetReloadWindow();
				activeUseStateInfoData.m_activeActionTimeMin = reloadWindow.x / m_reloadWeapon.WeaponSettings.AddAmmoReloadTime;
				activeUseStateInfoData.m_activeActiveTimeMax = reloadWindow.y / m_reloadWeapon.WeaponSettings.AddAmmoReloadTime;
			}
			activeUseStateInfoData.m_activeUseState = m_activeReloadState;
			activeUseStateInfoData.m_progress = m_reloadTimer / m_reloadWeapon.WeaponSettings.AddAmmoReloadTime;
			activeUseStateInfoData.m_activeActionTime = m_activeReloadTime / m_reloadWeapon.WeaponSettings.AddAmmoReloadTime;
			activeUseStateInfoData.m_isLastReloadAction = m_isLastReloadAction;
			activeUseStateInfoData.m_currentAmmoAmount = m_reloadWeapon.AmmoCount;
			activeUseStateInfoData.m_maxAmmoCapacity = m_reloadWeapon.GetMaxAmmoCapacity();
			activeUseStateInfoData.m_actionViewAssetReference = m_reloadWeapon.GetReloadActionViewAsset();
		}
		GlobalReferences.Instance.EventChannels.Generic.WeaponReloadInfo.Raise(activeUseStateInfoData);
	}

	private Vector2 GetReloadWindow()
	{
		Vector2 activeReloadWindow = m_reloadWeapon.WeaponSettings.ActiveReloadWindow;
		if (m_inventory != null && m_inventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.EasierReload, out float floatValue))
		{
			activeReloadWindow.x -= floatValue * 0.5f;
			activeReloadWindow.y += floatValue * 0.5f;
		}
		return activeReloadWindow;
	}

	private void OnRequestCharacterReload()
	{
		if (OnRequestedCharacterReload == null)
		{
			m_isReloadQueued = true;
			m_reloadQueuedClearTimer = 1f;
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.InGameMenu);
		}
		else
		{
			OnRequestedCharacterReload();
		}
	}

	public bool CheckForReloadQueued()
	{
		if (m_isReloadQueued)
		{
			m_isReloadQueued = false;
			return true;
		}
		return false;
	}
}
