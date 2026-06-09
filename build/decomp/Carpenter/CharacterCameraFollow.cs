using UnityEngine;

public class CharacterCameraFollow : MonoBehaviour
{
	[SerializeField]
	private Transform m_followTransform;

	[SerializeField]
	private CameraTargetData m_cameraTargetData;

	[SerializeField]
	private CameraFollowSettings m_defaultCameraFollowSettings;

	[SerializeField]
	private Vector3 m_baseOffset;

	[SerializeField]
	private Collider2D m_collider;

	[SerializeField]
	private float m_zoomSpeed = 0.2f;

	private CameraFollowSettings m_overrideCameraFollowSettings;

	[SerializeField]
	private Vector2 m_aimOffset;

	private BaseCharacterInput m_input;

	private CharacterDirection m_direction;

	private CharacterAiming m_aiming;

	private Rigidbody2D m_rigidbody;

	private float m_zoomScale = 1f;

	public CameraFollowSettings ActiveSettings
	{
		get
		{
			if (m_overrideCameraFollowSettings != null)
			{
				return m_overrideCameraFollowSettings;
			}
			return m_defaultCameraFollowSettings;
		}
		set
		{
			if (value == m_defaultCameraFollowSettings)
			{
				m_overrideCameraFollowSettings = null;
			}
			else
			{
				m_overrideCameraFollowSettings = value;
			}
		}
	}

	public Vector2 AimOffset
	{
		get
		{
			return m_aimOffset;
		}
		set
		{
			m_aimOffset = value;
		}
	}

	private void Awake()
	{
		m_input = base.gameObject.GetCharacterInputComponent();
		m_direction = GetComponent<CharacterDirection>();
		m_aiming = GetComponent<CharacterAiming>();
		m_rigidbody = GetComponent<Rigidbody2D>();
		m_zoomScale = 1f;
		SetCameraTargetData();
	}

	private void Update()
	{
		m_zoomScale = Mathf.MoveTowards(m_zoomScale, ActiveSettings.ZoomScale, Time.deltaTime * m_zoomSpeed);
	}

	private void FixedUpdate()
	{
		SetCameraTargetData();
	}

	private void OnDisable()
	{
		m_cameraTargetData.m_active = false;
	}

	public void SetInstantMove()
	{
		SetCameraTargetData();
	}

	private void SetCameraTargetData()
	{
		if (Vector2.Distance(m_rigidbody.position, base.transform.position) > 2f)
		{
			return;
		}
		m_cameraTargetData.m_targetObject = base.gameObject;
		m_cameraTargetData.m_position = m_followTransform.transform.position;
		m_cameraTargetData.m_characterBounds = m_collider.bounds;
		m_cameraTargetData.m_baseOffset = m_baseOffset;
		Vector3 zero = Vector3.zero;
		if (m_aimOffset.magnitude > 0f)
		{
			float num = 1f;
			if (m_aiming != null && m_aiming.enabled && m_aiming.Weapon != null)
			{
				foreach (ItemInstance attachedUpgrade in m_aiming.Weapon.WeaponItemInstance.AttachedUpgrades)
				{
					ProjectileWeaponUpgradeDefinition projectileWeaponUpgradeDefinition = attachedUpgrade.ItemDefinition as ProjectileWeaponUpgradeDefinition;
					if (projectileWeaponUpgradeDefinition.Settings.Zoom)
					{
						num = projectileWeaponUpgradeDefinition.Settings.ZoomScale;
					}
				}
			}
			zero.x += m_aimOffset.x * ActiveSettings.HorizontalOffsetScalar * num;
			zero.y += m_aimOffset.y * ((m_aimOffset.y > 0f) ? ActiveSettings.UpOffsetScalar : ActiveSettings.DownOffsetScalar);
		}
		zero.x += ActiveSettings.PositionOffset.x * (ActiveSettings.OffsetIgnoreDirection ? 1f : m_direction.GetForwardVector().x);
		zero.y += ActiveSettings.PositionOffset.y;
		m_cameraTargetData.m_userOffset = zero;
		m_cameraTargetData.m_zoomScale = m_zoomScale;
		if (m_direction != null)
		{
			m_cameraTargetData.m_facing = m_direction.CurrentDirection;
		}
		else
		{
			m_cameraTargetData.m_facing = CharacterDirection.Facing.None;
		}
		m_cameraTargetData.m_active = true;
	}
}
