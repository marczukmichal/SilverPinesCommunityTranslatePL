using System;
using DG.Tweening;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterDirection))]
public class CharacterAiming : MonoBehaviour
{
	public enum AimType
	{
		RangedWeapon,
		Throw
	}

	[SerializeField]
	private GameObject m_aimingGameObjectContainer;

	[SerializeField]
	private Transform m_armsTransform;

	[SerializeField]
	private Transform m_backArmTransform;

	[SerializeField]
	private Transform m_frontArmTransform;

	[SerializeField]
	private Transform m_weaponAnimNodeTransform;

	[SerializeField]
	private Vector2 m_minMaxArmTiltRangedWeapon;

	[SerializeField]
	private Vector2 m_minMaxArmTiltThrow;

	[SerializeField]
	private float m_updateRate;

	[SerializeField]
	private float m_angleSnapSize;

	[SerializeField]
	private float m_aimTime = 0.01f;

	[SerializeField]
	private float m_startAimAngle = 20f;

	[SerializeField]
	private float m_startAimAngleThrow = 30f;

	[Header("Event Channels")]
	[SerializeField]
	private DynamicHintGameEventChannel m_dynamicHintEventChannel;

	[Header("Aim Assist")]
	[SerializeField]
	private AimTargetSet m_aimTargetsSet;

	[SerializeField]
	private float m_aimAssistMaxDistance;

	[SerializeField]
	private float m_aimAssistMinDotProduct;

	[SerializeField]
	private AnimationCurve m_aimAssistDistancePriorityCurve;

	[Header("Mouse stuff")]
	[SerializeField]
	private MouseAimInputData m_mouseAimInputData;

	[Header("Arms")]
	[SerializeField]
	private SpriteRenderer m_frontArmSpriteRenderer;

	[SerializeField]
	private SpriteRenderer m_backArmSpriteRenderer;

	[Header("Throwing Arms")]
	[SerializeField]
	private Sprite m_throwingArmSpriteFront;

	[SerializeField]
	private Sprite m_throwingArmSpriteBack;

	[SerializeField]
	private Vector3 m_throwAimFrontPosition;

	[SerializeField]
	private Vector3 m_throwAimFrontRotation;

	[SerializeField]
	private Vector3 m_throwAimBackPosition;

	[SerializeField]
	private Vector3 m_throwAimBackRotation;

	[Header("Flashlight")]
	[SerializeField]
	private Transform m_flashlightAimingParent;

	[SerializeField]
	private Transform m_flashlightNotAimingParent;

	[SerializeField]
	private Transform m_flashlightTransform;

	[Header("Body Breathing")]
	[SerializeField]
	private float m_breathingAmount = 0.03f;

	[SerializeField]
	private float m_breathingFrequency = 3f;

	public UnityAction<bool> m_onAiming;

	private ProjectileWeapon m_weapon;

	private Projectile m_heldThrowableProjectile;

	private float m_updateTimer;

	private CharacterInventory m_characterInventory;

	private CharacterDirection m_characterDirection;

	private CharacterStamina m_characterStamina;

	private CharacterEquipment m_characterEquipment;

	private CharacterInputPlayer m_inputPlayer;

	private SpriteAnim m_anim;

	private Vector2 m_aimInput;

	private Vector2 m_previousValidAimInput;

	private float m_previousAnimTime;

	private float m_activeRecoil;

	private float m_queuedRecoil;

	private float m_lastRecoilAddTime;

	private float m_lastFireTime;

	private float m_previousAimAngleNoRecoil;

	private float m_previousAimAngle;

	private float m_aimVelocity;

	private AimTarget m_previousAimAssistTarget;

	private Vector2 m_previousAimVector;

	private AimType m_aimType;

	private bool m_useAimAnimationTime;

	private bool m_allowPumpAnimation;

	private bool m_useAimForSecondary;

	private bool m_checkForBlock;

	private bool m_isAimBlocked;

	public UnityAction<bool> OnAimBlockedChanged;

	private float m_weaponSwayStabilisation;

	private float m_weaponSwayIntro;

	private bool m_aimingActive;

	private bool m_weaponShownOnAnimNode;

	public Transform ArmsTransform => m_armsTransform;

	private Vector2 MinMaxArmTilt
	{
		get
		{
			if (m_weapon != null && m_weapon.WeaponItemInstance.WeaponSettings.UseMinMaxAimOverride)
			{
				return m_weapon.WeaponItemInstance.WeaponSettings.MinMaxAimOverride;
			}
			if (m_aimType == AimType.Throw)
			{
				return m_minMaxArmTiltThrow;
			}
			return m_minMaxArmTiltRangedWeapon;
		}
	}

	public float StartAimAngle
	{
		get
		{
			if (m_aimType == AimType.Throw)
			{
				return m_startAimAngleThrow;
			}
			return m_startAimAngle;
		}
	}

	public ProjectileWeapon Weapon => m_weapon;

	public Transform AimTransform
	{
		get
		{
			if (m_weapon != null)
			{
				return m_weapon.transform;
			}
			return m_armsTransform;
		}
	}

	public Vector2 PreviousAimVector => m_previousAimVector;

	public AimType ActiveAimType
	{
		get
		{
			return m_aimType;
		}
		set
		{
			m_aimType = value;
		}
	}

	public Vector2 AimInput
	{
		get
		{
			return m_aimInput;
		}
		set
		{
			m_aimInput = value;
		}
	}

	public bool UseAimAnimationTime
	{
		get
		{
			return m_useAimAnimationTime;
		}
		set
		{
			m_useAimAnimationTime = value;
		}
	}

	public bool AllowPumpAnimation
	{
		get
		{
			return m_allowPumpAnimation;
		}
		set
		{
			m_allowPumpAnimation = value;
			if (m_weapon != null)
			{
				m_weapon.AllowPumpAction = value;
			}
		}
	}

	public bool UseAimForSecondary
	{
		get
		{
			return m_useAimForSecondary;
		}
		set
		{
			m_useAimForSecondary = value;
		}
	}

	public bool MovementBlocked
	{
		get
		{
			if (m_weapon != null)
			{
				return Time.time - m_lastFireTime <= m_weapon.WeaponItemInstance.WeaponSettings.MovementBlockOnFireTime;
			}
			return false;
		}
	}

	public bool CheckForBlock
	{
		get
		{
			return m_checkForBlock;
		}
		set
		{
			if (m_checkForBlock != value)
			{
				m_checkForBlock = value;
			}
		}
	}

	public bool AimingActive
	{
		get
		{
			return m_aimingActive;
		}
		set
		{
			if (m_aimingActive != value)
			{
				m_aimingActive = value;
				if (m_aimingActive)
				{
					ActivateAiming();
				}
				else
				{
					DisableAiming();
				}
			}
		}
	}

	public bool AimBlocked
	{
		get
		{
			return m_isAimBlocked;
		}
		private set
		{
			if (m_isAimBlocked != value)
			{
				m_isAimBlocked = value;
				OnAimBlockedChanged?.Invoke(value);
			}
		}
	}

	public bool WeaponShownOnAnimNode
	{
		get
		{
			return m_weaponShownOnAnimNode;
		}
		set
		{
			m_weaponShownOnAnimNode = value;
		}
	}

	private void Awake()
	{
		m_characterInventory = GetComponent<CharacterInventory>();
		m_characterDirection = GetComponent<CharacterDirection>();
		m_characterStamina = GetComponent<CharacterStamina>();
		m_characterEquipment = GetComponent<CharacterEquipment>();
		m_inputPlayer = GetComponent<CharacterInputPlayer>();
		m_aimingGameObjectContainer.SetActive(m_aimingActive);
		m_anim = GetComponent<SpriteAnim>();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ItemInstanceDefinitionChanged.Register(OnItemDefinitionChanged);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ItemInstanceDefinitionChanged.Unregister(OnItemDefinitionChanged);
	}

	private void OnItemDefinitionChanged(ItemInstance itemInstance)
	{
		if (m_weapon != null && m_weapon.WeaponItemInstance == itemInstance)
		{
			DestroyActiveWeapon();
		}
	}

	public void ResetAim()
	{
		if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Right)
		{
			m_previousAimAngle = StartAimAngle;
		}
		else
		{
			m_previousAimAngle = 0f - StartAimAngle;
		}
	}

	private void ActivateAiming()
	{
		UpdateEquippedItem();
		if (!m_checkForBlock)
		{
			m_isAimBlocked = false;
		}
		m_aimingGameObjectContainer.SetActive(!AimBlocked);
		m_updateTimer = 0f;
		m_activeRecoil = 0f;
		m_queuedRecoil = 0f;
		m_weaponSwayIntro = 0f;
		float t = 0f;
		if (m_characterStamina != null)
		{
			t = Mathf.Min(m_characterStamina.AvailableStamina / m_characterStamina.MaxStamina, 0.5f);
		}
		if (m_weapon != null)
		{
			m_weaponSwayStabilisation = Mathf.Lerp(0f, m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayMaxStability, t);
		}
		else
		{
			m_weaponSwayStabilisation = 0f;
		}
		if (m_useAimAnimationTime)
		{
			m_previousAnimTime = 0.4f;
			m_anim.SetNormalizedTime(m_previousAnimTime);
		}
		if (m_dynamicHintEventChannel != null && m_inputPlayer != null && !m_inputPlayer.IsForceAiming())
		{
			m_dynamicHintEventChannel.Raise(DynamicHintEvent.StartAiming);
		}
		m_mouseAimInputData.IsAiming = true;
		if (m_flashlightTransform != null)
		{
			m_flashlightTransform.SetParent(m_flashlightAimingParent, worldPositionStays: false);
		}
		m_onAiming?.Invoke(arg0: true);
		UpdateAim(checkForBlock: false);
	}

	private void DisableAiming()
	{
		if (m_aimType == AimType.Throw)
		{
			m_characterEquipment.DisableActiveWeapon();
		}
		m_aimingGameObjectContainer.SetActive(value: false);
		m_mouseAimInputData.IsAiming = false;
		if (m_flashlightTransform != null)
		{
			m_flashlightTransform.SetParent(m_flashlightNotAimingParent, worldPositionStays: false);
		}
		m_onAiming?.Invoke(arg0: false);
	}

	private void DestroyActiveWeapon()
	{
		if ((bool)m_weapon)
		{
			UnityEngine.Object.Destroy(m_weapon.gameObject);
			m_weapon = null;
		}
		if (m_heldThrowableProjectile != null)
		{
			m_heldThrowableProjectile.GetComponent<DynamicallySpawnedObject>().ReleaseSafe();
			m_heldThrowableProjectile = null;
		}
	}

	private void UpdateEquippedItem()
	{
		AimType activeAimType = AimType.RangedWeapon;
		if (m_weapon != null && m_weapon.WeaponItemInstance == m_characterInventory.Inventory.EquippedRangedItem)
		{
			m_weapon.UpdateUpgradeVisibility();
			return;
		}
		DestroyActiveWeapon();
		ItemInstance equippedPrimaryItem = m_characterInventory.Inventory.EquippedPrimaryItem;
		if (equippedPrimaryItem != null && equippedPrimaryItem is ProjectileWeaponItemInstance)
		{
			activeAimType = AimType.RangedWeapon;
			if (equippedPrimaryItem.ItemDefinition is ProjectileWeaponItemDefinition)
			{
				ProjectileWeaponItemDefinition projectileWeaponItemDefinition = equippedPrimaryItem.ItemDefinition as ProjectileWeaponItemDefinition;
				if (projectileWeaponItemDefinition.WeaponGameObject != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(projectileWeaponItemDefinition.WeaponGameObject, m_frontArmTransform);
					gameObject.transform.localPosition = projectileWeaponItemDefinition.WeaponGameObject.transform.localPosition;
					gameObject.transform.localRotation = projectileWeaponItemDefinition.WeaponGameObject.transform.localRotation;
					gameObject.transform.localScale = projectileWeaponItemDefinition.WeaponGameObject.transform.localScale;
					m_weapon = gameObject.GetComponent<ProjectileWeapon>();
					m_weapon.Setup(equippedPrimaryItem as ProjectileWeaponItemInstance);
					m_weapon.AllowPumpAction = AllowPumpAnimation;
					ApplyWeaponArmSettings(m_weapon);
				}
				else
				{
					Debug.LogError("No wielded projectile weapon prefab defined for weapon type: " + projectileWeaponItemDefinition.name);
				}
			}
		}
		else if (equippedPrimaryItem != null && equippedPrimaryItem is SecondaryWeaponItemInstance secondaryWeaponItemInstance)
		{
			activeAimType = AimType.Throw;
			ApplyWeaponArmSettings(secondaryWeaponItemInstance);
			SecondaryWeaponItemDefinition weaponDefinition = secondaryWeaponItemInstance.WeaponDefinition;
			if (weaponDefinition.AssetReference != null)
			{
				DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(weaponDefinition.ThrownProjectileSettings.ProjectilePrefab, persistent: false, Vector3.zero);
				ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
				onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, (UnityAction<GameObject>)delegate(GameObject spawnedObject)
				{
					spawnedObject.transform.SetParent(m_backArmTransform);
					spawnedObject.transform.SetLocalPositionAndRotation(weaponDefinition.HeldPosition, Quaternion.Euler(weaponDefinition.HeldRotation));
					m_heldThrowableProjectile = spawnedObject.GetComponent<Projectile>();
					m_heldThrowableProjectile.enabled = false;
					m_heldThrowableProjectile.gameObject.SetActive(value: true);
					m_heldThrowableProjectile.SetVisualsActive(active: true);
				});
				DynamicallySpawnedObject.Spawn(eventData);
			}
			else
			{
				Debug.LogError("No wielded projectile weapon prefab defined for weapon type: " + weaponDefinition.name);
			}
		}
		ActiveAimType = activeAimType;
	}

	public void ShowWeaponOnWeaponAnimNode(bool enable, float zOffset, bool allowSwap)
	{
		m_weaponShownOnAnimNode = enable;
		if (allowSwap)
		{
			UpdateEquippedItem();
		}
		if (m_weapon != null)
		{
			ProjectileWeaponItemDefinition projectileWeaponItemDefinition = m_weapon.WeaponItemInstance.ItemDefinition as ProjectileWeaponItemDefinition;
			m_weapon.transform.DOKill();
			if (enable)
			{
				m_weapon.transform.SetParent(m_weaponAnimNodeTransform);
				m_weapon.transform.localPosition = new Vector3(0f, 0f, zOffset);
				m_weapon.transform.localRotation = Quaternion.identity;
			}
			else
			{
				m_weapon.transform.SetParent(m_frontArmTransform);
				m_weapon.transform.localPosition = projectileWeaponItemDefinition.WeaponGameObject.transform.localPosition;
				m_weapon.transform.localRotation = projectileWeaponItemDefinition.WeaponGameObject.transform.localRotation;
			}
			m_weapon.transform.localScale = projectileWeaponItemDefinition.WeaponGameObject.transform.localScale;
		}
	}

	public void ApplyWeaponArmSettings(ProjectileWeapon weapon)
	{
		m_frontArmSpriteRenderer.sprite = weapon.FrontArmSprite;
		m_backArmSpriteRenderer.sprite = weapon.BacktArmSprite;
		m_frontArmSpriteRenderer.enabled = m_frontArmSpriteRenderer.sprite != null;
		m_backArmSpriteRenderer.enabled = m_backArmSpriteRenderer.sprite != null;
		m_frontArmSpriteRenderer.transform.localPosition = weapon.FrontArmPosition;
		m_backArmSpriteRenderer.transform.localPosition = weapon.BackArmPosition;
		Quaternion quaternion2 = (m_frontArmSpriteRenderer.transform.localRotation = (m_backArmSpriteRenderer.transform.localRotation = Quaternion.identity));
	}

	private void ApplyWeaponArmSettings(SecondaryWeaponItemInstance secondaryThrowable)
	{
		m_frontArmSpriteRenderer.sprite = m_throwingArmSpriteFront;
		m_backArmSpriteRenderer.sprite = m_throwingArmSpriteBack;
		m_frontArmSpriteRenderer.enabled = m_frontArmSpriteRenderer.sprite != null;
		m_backArmSpriteRenderer.enabled = m_backArmSpriteRenderer.sprite != null;
		m_frontArmSpriteRenderer.transform.localPosition = m_throwAimFrontPosition;
		m_backArmSpriteRenderer.transform.localPosition = m_throwAimBackPosition;
		m_frontArmSpriteRenderer.transform.localRotation = Quaternion.Euler(m_throwAimFrontRotation);
		m_backArmSpriteRenderer.transform.localRotation = Quaternion.Euler(m_throwAimBackRotation);
	}

	public bool CheckForRangedWeaponMismatch()
	{
		ItemInstance equippedRangedItem = m_characterInventory.Inventory.EquippedRangedItem;
		if (!(m_weapon == null) || equippedRangedItem == null)
		{
			if (m_weapon != null)
			{
				return m_weapon.WeaponItemInstance != equippedRangedItem;
			}
			return false;
		}
		return true;
	}

	private Vector2 ApplyAimAssist(Vector2 aimInput)
	{
		Vector2 vector = m_armsTransform.position;
		AimTarget aimTarget = null;
		float num = 0f;
		foreach (AimTarget item in m_aimTargetsSet)
		{
			if (item.IsHidden)
			{
				continue;
			}
			Vector2 direction = item.Position - vector;
			float num2 = Vector2.Distance(item.Position, vector);
			if (num2 > m_aimAssistMaxDistance)
			{
				continue;
			}
			float num3 = Vector2.Dot(direction.normalized, aimInput.normalized);
			if (!(num3 < m_aimAssistMinDotProduct) && !(Physics2D.Raycast(vector, direction, num2, GameLayers.EnvironmentMask).collider != null))
			{
				float time = num2 / m_aimAssistMaxDistance;
				float num4 = num3 * m_aimAssistDistancePriorityCurve.Evaluate(time);
				if (num4 > num)
				{
					aimTarget = item;
					num = num4;
				}
			}
		}
		if (aimTarget != null && !aimTarget.IsAimingAtTargetBounds(vector, aimInput))
		{
			aimInput = (aimTarget.Position - vector).normalized;
			m_previousAimAssistTarget = aimTarget;
		}
		return aimInput;
	}

	private float GetClosestAnimFrameTime(float animTime)
	{
		float length = m_anim.GetCurrentAnimation().length;
		return Mathf.Round(animTime * length) / length;
	}

	private void UpdateAim(bool checkForBlock = true)
	{
		m_updateTimer = m_updateRate;
		Vector2 vector = m_aimInput;
		m_previousAimAssistTarget = null;
		if (vector.x < 0f && m_characterDirection.DesiredDirection == CharacterDirection.Facing.Right)
		{
			vector = m_previousValidAimInput;
		}
		else if (vector.x > 0f && m_characterDirection.DesiredDirection == CharacterDirection.Facing.Left)
		{
			vector = m_previousValidAimInput;
		}
		else
		{
			m_previousValidAimInput = vector;
		}
		if (vector.magnitude == 0f)
		{
			vector = ((m_characterDirection.DesiredDirection != CharacterDirection.Facing.Right) ? new Vector2(-1f, 0f) : new Vector2(1f, 0f));
		}
		else
		{
			if (m_characterDirection.DesiredDirection == CharacterDirection.Facing.Right)
			{
				vector.x = Mathf.Clamp(vector.x, 0.01f, 0.99f);
			}
			else
			{
				vector.x = Mathf.Clamp(vector.x, -0.99f, -0.01f);
			}
			vector.y = Mathf.Clamp(vector.y, -0.99f, 0.99f);
		}
		vector.Normalize();
		bool flag = vector.x > 0f;
		float num = Quaternion.FromToRotation(m_characterDirection.GetForwardVector(), vector).eulerAngles.z;
		if (num >= 180f)
		{
			num -= 360f;
		}
		m_previousAimAngleNoRecoil = num;
		num = ((m_characterDirection.CurrentDirection != CharacterDirection.Facing.Right) ? (num - m_activeRecoil) : (num + m_activeRecoil));
		if (m_angleSnapSize > 0f)
		{
			num = num.SnapToGrid(m_angleSnapSize);
		}
		float num2 = Mathf.SmoothDampAngle(m_previousAimAngle, num, ref m_aimVelocity, m_aimTime, 1000f, m_updateRate);
		float num3 = 0f;
		float num4 = 1f - m_weaponSwayStabilisation;
		if (m_weapon != null)
		{
			num3 += Mathf.Sin(Time.time * m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayFrequency) * m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayScale * num4;
			num3 += Mathf.Cos(Time.time * m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayFrequency2) * m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayScale2 * num4;
			num3 *= m_weaponSwayIntro;
		}
		if (m_characterInventory != null)
		{
			if (m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.ReducedWeaponSway, out var floatValue, out var _))
			{
				float num5 = 1f - floatValue;
				num3 *= num5;
			}
			if (m_characterInventory.ArtifactInventory.HasActiveArtifactEffect(GlobalReferences.Instance.ArtifactEffects.Generic.BetterWeaponHandling))
			{
				num3 *= 0.4f;
			}
		}
		num2 += num3;
		if (flag)
		{
			num2 = num2.ClampToVector2(MinMaxArmTilt);
			m_armsTransform.rotation = Quaternion.Euler(0f, 0f, num2);
		}
		else
		{
			Vector2 minMax = new Vector2(MinMaxArmTilt.y * -1f, MinMaxArmTilt.x * -1f);
			num2 = num2.ClampToVector2(minMax);
			m_armsTransform.rotation = Quaternion.Euler(0f, 0f, num2);
		}
		m_previousAimVector = (flag ? Vector2.right : Vector2.left);
		m_previousAimVector = m_previousAimVector.Rotate(num2);
		m_previousAimAngle = num2;
		float num6 = num2.Remap(MinMaxArmTilt.x, MinMaxArmTilt.y, 1f, 0f);
		num6 += Mathf.Sin(Time.time * m_breathingFrequency) * m_breathingAmount;
		num6 = Mathf.Clamp01(num6);
		if (flag)
		{
			num6 = 1f - num6;
		}
		if (m_useAimAnimationTime)
		{
			float length = m_anim.GetCurrentAnimation().length;
			float num7 = 1f / length;
			float closestAnimFrameTime = GetClosestAnimFrameTime(num6);
			if (Mathf.Abs(closestAnimFrameTime - m_previousAnimTime) > num7)
			{
				m_anim.SetNormalizedTime(closestAnimFrameTime);
				m_previousAnimTime = closestAnimFrameTime;
			}
			m_anim.SetSpeed(0f);
		}
		if (checkForBlock && m_weapon != null)
		{
			bool flag3 = (AimBlocked = GameUtils.BoxCastAgainstEnvironment(boxSize: new Vector2(0.1f, 0.1f), position: (Vector2)m_armsTransform.position + m_previousAimVector * m_weapon.WeaponItemInstance.WeaponSettings.WallBlockedDistance * 0.25f, forwardDir: m_previousAimVector, layerMask: GameLayers.EnvironmentMask, checkDistance: m_weapon.WeaponItemInstance.WeaponSettings.WallBlockedDistance * 0.75f));
		}
	}

	private void Update()
	{
		if (!m_aimingActive)
		{
			return;
		}
		if (m_updateTimer <= 0f)
		{
			UpdateAim(m_checkForBlock);
		}
		m_updateTimer -= Time.deltaTime;
		if (m_activeRecoil > 0f)
		{
			float time = Time.time - m_lastRecoilAddTime;
			float num = m_weapon.WeaponItemInstance.WeaponSettings.RecoilRecoveryCurve.Evaluate(time) * m_weapon.WeaponItemInstance.WeaponSettings.RecoilRecoveryCurveScalar;
			m_activeRecoil -= Time.deltaTime * num;
			m_activeRecoil = Mathf.Max(0f, m_activeRecoil);
		}
		if (m_queuedRecoil > 0f)
		{
			_ = Time.time;
			_ = m_lastRecoilAddTime;
			float b = Mathf.Max(10f, m_queuedRecoil * m_weapon.WeaponItemInstance.WeaponSettings.RecoilApplicationRateScalar) * Time.deltaTime;
			float num2 = Mathf.Min(m_queuedRecoil, b);
			float num3 = m_previousAimAngleNoRecoil;
			if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Left)
			{
				num3 *= -1f;
			}
			float a = MinMaxArmTilt.y - num3;
			m_activeRecoil += num2;
			m_activeRecoil = Mathf.Min(a, m_activeRecoil);
			m_queuedRecoil -= num2;
		}
		if (m_weapon != null)
		{
			m_weaponSwayStabilisation += Time.deltaTime / m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayStabilisationDuration;
			m_weaponSwayStabilisation = Mathf.Min(m_weapon.WeaponItemInstance.WeaponSettings.WeaponSwayMaxStability, m_weaponSwayStabilisation);
			m_weaponSwayIntro += Time.deltaTime;
			m_weaponSwayIntro = Mathf.Min(1f, m_weaponSwayIntro);
		}
	}

	public void SetWeaponFiring(bool firing)
	{
		if (m_aimType == AimType.RangedWeapon && (bool)m_weapon)
		{
			m_weapon.SetFiring(firing);
		}
	}

	public void AddRecoil(float recoilAmount)
	{
		m_queuedRecoil += recoilAmount;
		m_lastRecoilAddTime = Time.time;
	}

	public void RecordFire()
	{
		m_lastFireTime = Time.time;
		m_weaponSwayStabilisation = 0f;
		m_weaponSwayIntro = 0f;
	}

	public void TweenPumpArm(ProjectileWeaponSettings weaponSettings, Vector3 baseArmPosition)
	{
		Sequence s = DOTween.Sequence(m_backArmTransform);
		s.Append(m_backArmTransform.DOLocalMoveX(baseArmPosition.x + weaponSettings.PumpDistance, weaponSettings.PumpInTime));
		s.AppendInterval(weaponSettings.PumpMidActionDelay);
		s.Append(m_backArmTransform.DOLocalMoveX(baseArmPosition.x, weaponSettings.PumpOutTime));
	}

	private void OnDrawGizmos()
	{
		if (m_previousAimAssistTarget != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(m_armsTransform.position, m_previousAimAssistTarget.transform.position);
			Gizmos.color = Color.white;
		}
		if (m_armsTransform != null)
		{
			Gizmos.color = Color.magenta;
			Vector2 vector = m_armsTransform.position;
			vector += m_previousAimVector * 4f;
			Gizmos.DrawLine(m_armsTransform.position, vector);
			Gizmos.color = Color.white;
		}
	}

	public void PerformPumpFromAnimation(bool ejectShellCasing)
	{
		if (m_weapon != null)
		{
			m_weapon.PerformPumpFromAnimation(ejectShellCasing);
		}
	}

	public bool IsSecondaryWeaponType()
	{
		if (m_characterInventory.Inventory.EquippedSecondaryItem != null)
		{
			return true;
		}
		return false;
	}

	public Vector2 SnapThrowableToAimVector(Vector2 point)
	{
		Vector2 vector = m_armsTransform.position;
		vector.x += PreviousAimVector.x * 0.25f;
		vector.y += PreviousAimVector.y * 0.25f;
		Vector2 vector2 = m_armsTransform.position;
		vector2.x += PreviousAimVector.x * 0.5f;
		vector2.y += PreviousAimVector.y * 0.5f;
		Vector2 vector3 = vector2 - vector;
		float value = Vector2.Dot(point - vector, vector3) / vector3.sqrMagnitude;
		value = Mathf.Clamp01(value);
		return vector + value * vector3;
	}
}
