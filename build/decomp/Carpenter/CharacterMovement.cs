using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMovement : MonoBehaviour, IPersistentComponent
{
	public enum DefaultMovementBehavior
	{
		Stop,
		Maintain
	}

	public enum MovementBlockingType
	{
		None,
		Enemy,
		Player,
		EnemyMassive
	}

	public enum MovementCollisionMask
	{
		Normal,
		EnemySpecial
	}

	private struct OverlapResult
	{
		public float m_positiveOverlap;

		public float m_negativeOverlap;
	}

	private struct PhysicsCastData
	{
		public bool m_active;

		public Vector2 m_position;

		public Vector2 m_direction;

		public Vector2 m_size;

		public float m_distance;
	}

	public struct CollisionInfo
	{
		public bool m_above;

		public bool m_below;

		public bool m_left;

		public bool m_right;

		public bool m_isNearGround;

		public bool m_overlappingHorizontal;

		public bool m_overlappingVertical;

		public bool m_climbingSlope;

		public bool m_descendingSlope;

		public bool m_slidingDownMaxSlope;

		public bool m_blockedBySlope;

		public Stairs m_touchedStairs;

		public float m_slopeAngle;

		public float m_slopeAngleOld;

		public Vector2 m_slopeNormal;

		public Vector2 m_moveAmountOld;

		public int m_faceDir;

		public void Reset()
		{
			m_above = (m_below = false);
			m_left = (m_right = false);
			m_isNearGround = false;
			m_overlappingHorizontal = (m_overlappingVertical = false);
			m_climbingSlope = false;
			m_descendingSlope = false;
			m_slidingDownMaxSlope = false;
			m_blockedBySlope = false;
			m_slopeNormal = Vector2.zero;
			m_touchedStairs = null;
			m_slopeAngleOld = m_slopeAngle;
			m_slopeAngle = 0f;
		}
	}

	[Serializable]
	private class PersistentData
	{
		public Vector3 m_position;

		public Quaternion m_rotation;

		public Vector2 m_velocity;

		public bool m_hasPosition;
	}

	[Flags]
	public enum AvailableStairsDirection
	{
		None = 0,
		TravelDown = 1,
		TravelUp = 2,
		Both = 3
	}

	[Header("Raycast Controller")]
	[SerializeField]
	protected Collider2D m_collider;

	[SerializeField]
	private float m_skinWidth = 0.1f;

	[Header("Character Movement")]
	[SerializeField]
	private MovementBlockingType m_movementBlockingType;

	[SerializeField]
	private float m_maxSlopeAngle = 80f;

	[Header("Falling / Jumping")]
	[SerializeField]
	private float m_gravity = -10f;

	[SerializeField]
	private float m_coyoteTime;

	[SerializeField]
	private float m_isNearGroundDistance = 0.3f;

	[Header("Animations")]
	[SerializeField]
	private SpriteAnimNodeFollower m_animRootNode;

	[Header("Movement")]
	[SerializeField]
	private CharacterMovementSettings m_defaultMovementSettings;

	[SerializeField]
	private DefaultMovementBehavior m_defaultMovementBehavior;

	[Header("Z Movement")]
	[Tooltip("How far the character is pushed behind the platform they are standing on")]
	[SerializeField]
	private float m_zOffset = 0.1f;

	[Tooltip("Apply a random offset to the zOffset value to prevent z fighting for character")]
	[SerializeField]
	private bool m_useRandomZOffsetAdjustment = true;

	private float m_randomZOffset;

	[SerializeField]
	private float m_zMaxMoveSpeed = 10f;

	[Header("Character Size Values")]
	[SerializeField]
	private float m_verticalSize = 0.2f;

	[SerializeField]
	private float m_characterCollisionOffset = -0.2f;

	[SerializeField]
	private float m_horizontalFootProportion = 0.3f;

	private float m_zDepthMoveSpeedOverride = -1f;

	[Header("Predicted Position")]
	[SerializeField]
	private float m_predictedPositionTrackSpeed = 1f;

	[Header("Persistent Position")]
	[SerializeField]
	private bool m_persistentPosition = true;

	[Header("Restrictions")]
	[SerializeField]
	private bool m_useMovementRestrictions;

	[SerializeField]
	private Vector2 m_minMaxX;

	[Header("Collision Mask")]
	[SerializeField]
	private MovementCollisionMask m_collisionMaskMode;

	private bool m_allowCharacterPassthrough;

	private CharacterMovementSettings m_movementSettingsOverride;

	public CollisionInfo m_collisions;

	private Vector2 m_previousVelocity;

	private float m_offGroundTimer;

	private float m_forceFallTimer;

	private float m_snapToGroundIgnoreYTimer;

	private bool m_ignoreNextUpdate;

	private CharacterStance m_stance;

	private CharacterTraversalUtils m_traversal;

	private bool m_movementOverriden;

	private bool m_collisionDisabled;

	public UnityAction<float> OnVerticalCollision;

	public UnityAction<float> OnHorizontalCollision;

	private Rigidbody2D m_rigidbody;

	private HashSet<Collider2D> m_touchedColliders = new HashSet<Collider2D>();

	private Collider2D m_ignoredCollider;

	private BaseCharacterInput m_input;

	private GameplayWaterBounds m_activeSwimmingWaterBound;

	private CharacterDirection m_direction;

	private float m_isJumpingTimer;

	private float m_stepUpTimer;

	private RaycastHit2D[] m_raycastHits = new RaycastHit2D[32];

	private PhysicsCastData m_verticalCastData;

	private PhysicsCastData m_horizontalBodyCastData;

	private PhysicsCastData m_horizontalFootCastData;

	private float m_allowedStepHeight = 0.5f;

	private PhysicsCastData m_horizontalCharacterCastData;

	private PhysicsCastData m_descendSlopeCastData;

	private PhysicsCastData m_ascendSlopeCastData;

	[DebugCommand("movement_debug", "Enable movement debug info overlay", "movement_debug <true/false>", typeof(bool), false)]
	private static bool m_showMovementDebug;

	private SurfaceType m_waterSurface;

	public UnityAction<SurfaceType> m_onWaterSurfaceChanged;

	private SurfaceType m_currentSurfaceType;

	private Collider2D m_currentSurfaceCollider;

	private float m_zStateOffset;

	private float m_zForceDepthOverride;

	private bool m_fixedDepthMovement;

	private Vector2 m_predictedPositionOffset;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private float m_stairsForceAttachTimer = 0.2f;

	private Stairs m_attachedStairs;

	private List<Stairs> m_overlappingStairs;

	private AvailableStairsDirection m_stairsDirection;

	private bool m_isUsingStairs;

	private bool m_forcedMovementStairsBehaviour;

	public Collider2D Collider => m_collider;

	public MovementBlockingType BlockingType
	{
		get
		{
			return m_movementBlockingType;
		}
		set
		{
			m_movementBlockingType = value;
		}
	}

	public float VerticalSize => m_verticalSize;

	private float ZMaxMoveSpeed
	{
		get
		{
			if (m_zDepthMoveSpeedOverride > 0f)
			{
				return m_zDepthMoveSpeedOverride;
			}
			return m_zMaxMoveSpeed;
		}
	}

	public float ZDepthMoveSpeedOverride
	{
		get
		{
			return m_zDepthMoveSpeedOverride;
		}
		set
		{
			m_zDepthMoveSpeedOverride = value;
		}
	}

	public bool AllowCharacterPassthrough
	{
		get
		{
			return m_allowCharacterPassthrough;
		}
		set
		{
			m_allowCharacterPassthrough = value;
		}
	}

	public CharacterMovementSettings OverrideMovementSettings => m_movementSettingsOverride;

	public CharacterMovementSettings ActiveMovementSettings
	{
		get
		{
			if (m_movementSettingsOverride != null)
			{
				return m_movementSettingsOverride;
			}
			return m_defaultMovementSettings;
		}
	}

	public LayerMask CollisionMask
	{
		get
		{
			int num = ((m_collisionMaskMode != MovementCollisionMask.EnemySpecial) ? GameLayers.CharacterNavigationMask : GameLayers.EnemySpecialNavigationMask);
			if (!m_isUsingStairs && IsGrounded)
			{
				num &= ~GameLayers.StairsMask;
			}
			return num;
		}
	}

	public Vector2 PreviousVelocity => m_previousVelocity;

	public float OffGroundTimer => m_offGroundTimer;

	public bool IsGrounded
	{
		get
		{
			if (!m_collisions.m_below && !(m_offGroundTimer <= 0f) && !(m_offGroundTimer < m_coyoteTime))
			{
				return m_snapToGroundIgnoreYTimer > 0f;
			}
			return true;
		}
	}

	public bool IsForceFall => m_forceFallTimer > 0f;

	public bool MovementOverriden
	{
		get
		{
			return m_movementOverriden;
		}
		set
		{
			m_movementOverriden = value;
		}
	}

	public SurfaceType CurrentWaterSurface => m_waterSurface;

	public SurfaceSettings CurrentSurfaceSettings
	{
		get
		{
			if (m_waterSurface != null)
			{
				return m_waterSurface.Surface;
			}
			if (m_attachedStairs != null)
			{
				return m_attachedStairs.Surface;
			}
			if (m_currentSurfaceType != null)
			{
				return m_currentSurfaceType.Surface;
			}
			return GlobalReferences.Instance.DefaultSurfaceSettings;
		}
	}

	public Collider2D CurrentSurfaceCollider => m_currentSurfaceCollider;

	public float DepthStateOffset
	{
		get
		{
			return m_zStateOffset;
		}
		set
		{
			m_zStateOffset = value;
		}
	}

	public float ZForceDepthOverride
	{
		get
		{
			return m_zForceDepthOverride;
		}
		set
		{
			m_zForceDepthOverride = value;
		}
	}

	public bool FixedDepthMovement
	{
		get
		{
			return m_fixedDepthMovement;
		}
		set
		{
			m_fixedDepthMovement = value;
		}
	}

	public Vector2 PredictedPositionOffsetSmoothed => m_predictedPositionOffset;

	public List<Stairs> OverlappingStairs
	{
		get
		{
			return m_overlappingStairs;
		}
		set
		{
			m_overlappingStairs = value;
		}
	}

	public AvailableStairsDirection StairsDirection
	{
		get
		{
			return m_stairsDirection;
		}
		set
		{
			m_stairsDirection = value;
		}
	}

	public Stairs AttachedStairs => m_attachedStairs;

	public bool ForcedMovementStairsBehavior
	{
		get
		{
			return m_forcedMovementStairsBehaviour;
		}
		set
		{
			m_forcedMovementStairsBehaviour = value;
		}
	}

	public void SetMovementSettingsOverride(CharacterMovementSettings settings)
	{
		m_movementSettingsOverride = settings;
	}

	public void SetPreviousVelocity(Vector2 velocity)
	{
		m_previousVelocity = velocity;
	}

	public void SetOffGround()
	{
		m_collisions.Reset();
		m_offGroundTimer = m_coyoteTime;
		m_forceFallTimer = 0.1f;
	}

	public void SetActiveWaterBound(GameplayWaterBounds waterBounds)
	{
		m_activeSwimmingWaterBound = waterBounds;
	}

	public void SetIgnoredCollider(Collider2D collider)
	{
		m_ignoredCollider = collider;
	}

	public bool ShouldIgnoreColliderForRaycast(RaycastHit2D hit)
	{
		if (hit.collider != null)
		{
			if (m_ignoredCollider != null && m_ignoredCollider == hit.collider)
			{
				return true;
			}
			if (AttachedStairs != null && AttachedStairs.ShouldIgnoreCollider(hit.collider))
			{
				return true;
			}
			if (AttachedStairs != null && hit.collider.gameObject.layer == GameLayers.StairsLayer)
			{
				Stairs component = hit.collider.GetComponent<Stairs>();
				if (component != null && component != AttachedStairs)
				{
					Vector2.Dot(Vector2.down, hit.normal);
					if (m_isJumpingTimer <= 0f)
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public void TempIgnoreCollider(Collider2D collider, float timer)
	{
		StartCoroutine(TempIgnoreColliderCoroutine(collider, timer));
	}

	private IEnumerator TempIgnoreColliderCoroutine(Collider2D collider, float timer)
	{
		SetIgnoredCollider(collider);
		yield return new WaitForSeconds(timer);
		SetIgnoredCollider(null);
	}

	public bool IsTouchingCollider(Collider2D collider)
	{
		return m_touchedColliders.Contains(collider);
	}

	public void StartJump()
	{
		m_isJumpingTimer = 0.5f;
	}

	private void Awake()
	{
		m_predictedPositionOffset = Vector2.zero;
		m_randomZOffset = UnityEngine.Random.Range(0f, m_zOffset * 0.04f);
		m_stance = GetComponent<CharacterStance>();
		m_traversal = GetComponent<CharacterTraversalUtils>();
		m_rigidbody = GetComponent<Rigidbody2D>();
		m_input = base.gameObject.GetCharacterInputComponent();
		m_touchedColliders = new HashSet<Collider2D>();
		m_overlappingStairs = new List<Stairs>();
		m_direction = GetComponent<CharacterDirection>();
	}

	public void Start()
	{
		m_offGroundTimer = 0f;
		m_collisions.m_faceDir = 1;
	}

	private void OnEnable()
	{
		if (m_stance != null)
		{
			CharacterStance stance = m_stance;
			stance.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Combine(stance.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(OnStanceChanged));
		}
	}

	private void OnDisable()
	{
		if (m_stance != null)
		{
			CharacterStance stance = m_stance;
			stance.m_onStanceChanged = (UnityAction<CharacterStance.Stance>)Delegate.Remove(stance.m_onStanceChanged, new UnityAction<CharacterStance.Stance>(OnStanceChanged));
		}
	}

	private void Update()
	{
		Vector2 previousVelocity = m_previousVelocity;
		m_predictedPositionOffset = Vector2.MoveTowards(m_predictedPositionOffset, previousVelocity, Time.deltaTime * m_predictedPositionTrackSpeed);
		if (m_collisions.m_below)
		{
			m_offGroundTimer = 0f;
		}
		else if (!m_collisions.m_isNearGround)
		{
			m_offGroundTimer += Time.deltaTime;
		}
		if (m_forceFallTimer > 0f)
		{
			m_forceFallTimer -= Time.deltaTime;
		}
		if (m_snapToGroundIgnoreYTimer > 0f)
		{
			m_snapToGroundIgnoreYTimer -= Time.deltaTime;
		}
		if (m_isJumpingTimer > 0f)
		{
			m_isJumpingTimer -= Time.deltaTime;
		}
		if (m_stepUpTimer > 0f)
		{
			m_stepUpTimer -= Time.deltaTime;
		}
		if (m_stairsForceAttachTimer > 0f)
		{
			m_stairsForceAttachTimer -= Time.deltaTime;
		}
		if (m_attachedStairs == null && IsGrounded)
		{
			CheckForStairs(m_input.MovementInput);
		}
		UpdatePersistentData();
	}

	private void FixedUpdate()
	{
		if (!m_movementOverriden)
		{
			float xMoveAmount = 0f;
			switch (m_defaultMovementBehavior)
			{
			case DefaultMovementBehavior.Stop:
				xMoveAmount = 0f;
				break;
			case DefaultMovementBehavior.Maintain:
				xMoveAmount = m_previousVelocity.x;
				break;
			}
			MoveHorizontally(xMoveAmount);
		}
		if (AttachedStairs != null)
		{
			CorrectToStairsHeight();
		}
	}

	private void OnStanceChanged(CharacterStance.Stance newStance)
	{
		m_collisionDisabled = newStance == CharacterStance.Stance.NoCollision;
	}

	private float DoOverlapCast(Vector2 rayOrigin, Vector2 boxSize, Vector2 direction, float rayLength, Color debugColor)
	{
		float result = 0f;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.SetLayerMask(GameLayers.EnvironmentMask);
		int num = Physics2D.BoxCast(rayOrigin, boxSize, 0f, direction, contactFilter, m_raycastHits, rayLength);
		Debug.DrawRay(rayOrigin, direction * rayLength, debugColor);
		for (int i = 0; i < num; i++)
		{
			if (!GameUtils.ShouldIgnoreRaycastHitDueToPlatformEffector(m_raycastHits[i]) && !ShouldIgnoreColliderForRaycast(m_raycastHits[i]))
			{
				Climbable component = m_raycastHits[i].collider.GetComponent<Climbable>();
				if ((object)component == null || component.Mode != Climbable.ClimbableMode.ClimbThrough)
				{
					result = rayLength - m_raycastHits[i].distance;
					break;
				}
			}
		}
		return result;
	}

	public void MoveHorizontally(float xMoveAmount)
	{
		Vector2 moveAmount = new Vector2(xMoveAmount, m_previousVelocity.y);
		moveAmount.y += m_gravity * Time.deltaTime;
		Move(moveAmount, standingOnPlatform: false);
	}

	public void Move(Vector2 moveAmount, bool standingOnPlatform)
	{
		ClearSurface();
		m_touchedColliders.Clear();
		m_collisions.Reset();
		if (m_ignoreNextUpdate)
		{
			m_ignoreNextUpdate = false;
			return;
		}
		moveAmount *= Time.deltaTime;
		m_collisions.m_moveAmountOld = moveAmount;
		if (m_snapToGroundIgnoreYTimer > 0f)
		{
			moveAmount.y = 0f;
		}
		if (!m_collisionDisabled)
		{
			VerticalCollisions(ref moveAmount);
			if (!m_activeSwimmingWaterBound && m_isJumpingTimer <= 0f)
			{
				DescendSlopeCheck(ref moveAmount);
			}
			AscendSlopeCheck(ref moveAmount);
			if (Mathf.Abs(moveAmount.x) > Mathf.Epsilon)
			{
				HorizontalCollisions(ref moveAmount);
				CharacterCollisions(ref moveAmount);
			}
		}
		if (m_collisions.m_left)
		{
			moveAmount.x = Mathf.Min(moveAmount.x, 0f);
		}
		if (m_collisions.m_right)
		{
			moveAmount.x = Mathf.Max(moveAmount.x, 0f);
		}
		if (m_collisions.m_blockedBySlope)
		{
			moveAmount.x = 0f;
			moveAmount.y = Mathf.Min(0f, moveAmount.y);
		}
		base.transform.Translate(moveAmount, Space.World);
		if (standingOnPlatform)
		{
			m_collisions.m_below = true;
		}
		if (m_collisions.m_above || m_collisions.m_below)
		{
			OnVerticalCollision?.Invoke(m_previousVelocity.y);
		}
		if (m_collisions.m_left || m_collisions.m_right)
		{
			OnHorizontalCollision?.Invoke(m_previousVelocity.x);
		}
		if (GameUtils.IsPlayer(base.gameObject))
		{
			GlobalReferences.Instance.DataStore.Data.m_stats.m_distanceTravelled += Mathf.Abs(moveAmount.x);
		}
		m_previousVelocity = moveAmount / Time.deltaTime;
		if (m_stepUpTimer > 0f)
		{
			m_previousVelocity.y = 0f;
		}
		CheckForNearGround();
		CheckForWaterSurfaces();
		UpdateDepth();
		if (m_rigidbody != null)
		{
			m_rigidbody.linearVelocity = Vector2.zero;
		}
		if (m_isUsingStairs)
		{
			if (m_collisions.m_touchedStairs != AttachedStairs && m_stairsForceAttachTimer <= 0f)
			{
				AttachToStairs(null);
			}
		}
		else if (m_collisions.m_touchedStairs != null)
		{
			AttachToStairs(m_collisions.m_touchedStairs);
		}
	}

	private int PerformMovementBoxCast(PhysicsCastData castData, bool onlyCharacters = false)
	{
		ContactFilter2D contactFilter = default(ContactFilter2D);
		if (onlyCharacters)
		{
			contactFilter.SetLayerMask(1 << GameLayers.MovementLayer);
		}
		else
		{
			contactFilter.SetLayerMask(CollisionMask);
		}
		return Physics2D.BoxCast(castData.m_position, castData.m_size, 0f, castData.m_direction, contactFilter, m_raycastHits, castData.m_distance);
	}

	private PhysicsCastData GetVerticalCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 position = bounds.center;
		Vector2 vector = ((!(moveAmount.y > 0f)) ? Vector2.down : Vector2.up);
		Vector2 size = new Vector2(m_verticalSize, bounds.size.y * 0.5f);
		position -= m_skinWidth * 0.5f * vector;
		position.y += size.y * 0.5f * vector.y;
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = vector;
		result.m_distance = Mathf.Abs(moveAmount.y) + m_skinWidth * 0.5f;
		result.m_size = size;
		return result;
	}

	private void VerticalCollisions(ref Vector2 moveAmount)
	{
		float num = Mathf.Sign(moveAmount.y);
		m_verticalCastData = GetVerticalCastData(moveAmount);
		int hitCount = PerformMovementBoxCast(m_verticalCastData);
		RaycastHit2D bestHit = GetBestHit(hitCount);
		if ((bool)bestHit)
		{
			if (bestHit.distance < m_skinWidth)
			{
				moveAmount.y = 0f;
			}
			else
			{
				moveAmount.y = (bestHit.distance - m_skinWidth) * num;
			}
			if (moveAmount.y <= 0f)
			{
				TryRegisterFloorSurface(bestHit);
			}
			m_collisions.m_below = num == -1f;
			m_collisions.m_above = num == 1f;
			m_touchedColliders.Add(bestHit.collider);
		}
	}

	private PhysicsCastData GetBodyHorizontalCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 position = bounds.center;
		Vector2 vector = ((!(moveAmount.x > 0f)) ? Vector2.left : Vector2.right);
		Vector2 size = new Vector2(m_skinWidth + bounds.size.x * 0.5f, bounds.size.y * (1f - m_horizontalFootProportion));
		position -= m_skinWidth * 0.5f * vector;
		position.x += size.x * 0.5f * vector.x;
		position.y += size.y * 0.25f;
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = vector;
		result.m_distance = Mathf.Abs(moveAmount.x) + m_skinWidth * 0.5f;
		result.m_size = size;
		return result;
	}

	private PhysicsCastData GetFootHorizontalCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 position = bounds.center;
		Vector2 vector = ((!(moveAmount.x > 0f)) ? Vector2.left : Vector2.right);
		Vector2 size = new Vector2(m_skinWidth, bounds.size.y * m_horizontalFootProportion);
		position -= m_skinWidth * 0.5f * vector;
		position.x += size.x * 0.5f * vector.x;
		position.y = bounds.min.y;
		position.y += size.y * 0.5f;
		position.y += m_skinWidth;
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = vector;
		result.m_distance = Mathf.Abs(moveAmount.x) + m_skinWidth * 0.5f + bounds.size.x * 0.5f;
		result.m_size = size;
		return result;
	}

	private void HorizontalCollisions(ref Vector2 moveAmount)
	{
		float num = Mathf.Sign(moveAmount.x);
		Mathf.Sign(moveAmount.y);
		m_horizontalBodyCastData = GetBodyHorizontalCastData(moveAmount);
		m_horizontalFootCastData = GetFootHorizontalCastData(moveAmount);
		int hitCount = PerformMovementBoxCast(m_horizontalBodyCastData);
		RaycastHit2D bestHit = GetBestHit(hitCount);
		Bounds bounds = Collider.bounds;
		if (m_useMovementRestrictions)
		{
			if (moveAmount.x > 0f)
			{
				if (bounds.max.x + moveAmount.x > m_minMaxX.y)
				{
					moveAmount.x = 0f;
					return;
				}
			}
			else if (moveAmount.x < 0f && bounds.min.x + moveAmount.x < m_minMaxX.x)
			{
				moveAmount.x = 0f;
				return;
			}
		}
		if ((bool)bestHit && !GameUtils.ShouldIgnoreRaycastHitDueToPlatformEffector(bestHit))
		{
			if (bestHit.distance < m_skinWidth)
			{
				moveAmount.x = 0f;
			}
			else
			{
				moveAmount.x = (bestHit.distance - m_skinWidth) * num;
			}
			m_collisions.m_left = num == -1f;
			m_collisions.m_right = num == 1f;
			m_touchedColliders.Add(bestHit.collider);
			return;
		}
		float num2 = bounds.size.x * 0.5f;
		hitCount = PerformMovementBoxCast(m_horizontalFootCastData);
		RaycastHit2D bestHit2 = GetBestHit(hitCount);
		if (!bestHit2 || GameUtils.ShouldIgnoreRaycastHitDueToPlatformEffector(bestHit2))
		{
			return;
		}
		float num3 = Vector2.Angle(bestHit2.normal, Vector2.up);
		bool flag = false;
		if ((num3 < m_maxSlopeAngle || num3 > 90f) && Mathf.Sign(bestHit2.normal.x) != Mathf.Sign(moveAmount.x))
		{
			flag = true;
		}
		if (flag)
		{
			return;
		}
		float num4 = bestHit2.distance - num2;
		Vector2 position = bounds.min;
		if (num > 0f)
		{
			position.x += m_horizontalFootCastData.m_size.x * 0.5f;
		}
		else
		{
			position.x -= m_horizontalFootCastData.m_size.x * 0.5f;
		}
		position.y += bounds.size.y * m_horizontalFootProportion;
		PhysicsCastData physicsCastData = default(PhysicsCastData);
		physicsCastData.m_size = new Vector2(m_horizontalFootCastData.m_size.x, 0.01f);
		physicsCastData.m_distance = m_horizontalFootCastData.m_size.y;
		physicsCastData.m_direction = Vector2.down;
		physicsCastData.m_position = position;
		PhysicsCastData castData = physicsCastData;
		hitCount = PerformMovementBoxCast(castData);
		RaycastHit2D bestHit3 = GetBestHit(hitCount);
		bool flag2 = false;
		if ((bool)bestHit3 && Vector2.Dot(Vector2.up, bestHit3.normal) > 0.5f)
		{
			flag2 = true;
		}
		float num5 = bestHit3.point.y - base.transform.position.y;
		if (flag2 && m_collisions.m_below && num5 < m_allowedStepHeight && m_isJumpingTimer <= 0f && m_stepUpTimer <= 0f)
		{
			if (bestHit2.distance < m_verticalSize)
			{
				m_stepUpTimer = 1f;
				moveAmount.y = num5 + m_skinWidth + 0.02f;
				if (num > 0f)
				{
					moveAmount.x += 0.05f;
				}
				else
				{
					moveAmount.x -= 0.05f;
				}
			}
		}
		else
		{
			if (num4 < m_skinWidth)
			{
				moveAmount.x = 0f;
			}
			else
			{
				moveAmount.x = (num4 - m_skinWidth) * num;
			}
			m_collisions.m_left = num == -1f;
			m_collisions.m_right = num == 1f;
			m_touchedColliders.Add(bestHit2.collider);
		}
	}

	private PhysicsCastData GetCharacterHorizontalCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 position = bounds.center;
		Vector2 vector = ((!(moveAmount.x > 0f)) ? Vector2.left : Vector2.right);
		Vector2 size = new Vector2(m_skinWidth + bounds.size.x * 0.5f + m_characterCollisionOffset, bounds.size.y * 0.5f);
		position -= m_skinWidth * 0.5f * vector;
		position.x += size.x * 0.5f * vector.x;
		position.y += size.y * 0.25f;
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = vector;
		result.m_distance = Mathf.Abs(moveAmount.x) + m_skinWidth * 0.5f;
		result.m_size = size;
		return result;
	}

	private bool ValidateHitCharacter(RaycastHit2D hit, Vector2 moveAmount)
	{
		CharacterMovement componentInParent = hit.collider.GetComponentInParent<CharacterMovement>();
		if (componentInParent != null)
		{
			Vector2 vector = base.transform.position;
			Vector2 vector2 = componentInParent.transform.position;
			if (moveAmount.x < 0f && vector.x < vector2.x)
			{
				return false;
			}
			if (moveAmount.x > 0f && vector.x > vector2.x)
			{
				return false;
			}
		}
		return true;
	}

	private void CharacterCollisions(ref Vector2 moveAmount)
	{
		float num = Mathf.Sign(moveAmount.x);
		Mathf.Sign(moveAmount.y);
		m_horizontalCharacterCastData = GetCharacterHorizontalCastData(moveAmount);
		int hitCount = PerformMovementBoxCast(m_horizontalCharacterCastData, onlyCharacters: true);
		RaycastHit2D bestHit = GetBestHit(hitCount, checkForCharacters: true);
		_ = Collider.bounds;
		if ((bool)bestHit && ValidateHitCharacter(bestHit, moveAmount))
		{
			if (bestHit.distance < m_skinWidth)
			{
				moveAmount.x = 0f;
			}
			else
			{
				moveAmount.x = (bestHit.distance - m_skinWidth) * num;
			}
			m_collisions.m_left = num == -1f;
			m_collisions.m_right = num == 1f;
			m_touchedColliders.Add(bestHit.collider);
		}
	}

	private PhysicsCastData GetDescendSlopeCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 position = bounds.center;
		Vector2 down = Vector2.down;
		position.y = bounds.min.y;
		float num = 0.1f;
		Vector2 size = new Vector2(m_verticalSize, m_skinWidth);
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = down;
		result.m_distance = Mathf.Abs(moveAmount.y) + num * 0.5f;
		result.m_size = size;
		return result;
	}

	private void DescendSlopeCheck(ref Vector2 moveAmount)
	{
		if (Mathf.Approximately(moveAmount.x, 0f))
		{
			m_descendSlopeCastData.m_active = false;
			return;
		}
		Mathf.Sign(moveAmount.y);
		m_descendSlopeCastData = GetDescendSlopeCastData(moveAmount);
		int hitCount = PerformMovementBoxCast(m_descendSlopeCastData);
		RaycastHit2D bestHit = GetBestHit(hitCount);
		if (!bestHit)
		{
			return;
		}
		float num = Vector2.Angle(bestHit.normal, Vector2.up);
		if (num >= 0.1f && num < m_maxSlopeAngle && Mathf.Sign(bestHit.normal.x) == Mathf.Sign(moveAmount.x))
		{
			m_collisions.m_slopeAngle = num;
			m_collisions.m_slopeNormal = bestHit.normal;
			m_collisions.m_below = true;
			m_collisions.m_descendingSlope = true;
			Vector2 normalized = Vector2.Perpendicular(bestHit.normal).normalized;
			if (normalized.y > 0f)
			{
				normalized *= -1f;
			}
			moveAmount = normalized * Mathf.Abs(moveAmount.x);
			TryRegisterFloorSurface(bestHit);
		}
	}

	private PhysicsCastData GetAscendSlopeCastData(Vector2 moveAmount)
	{
		Bounds bounds = m_collider.bounds;
		Vector2 down = Vector2.down;
		Vector2 position = bounds.center;
		position.y = bounds.min.y;
		position.y += 0.5f;
		Vector2 size = new Vector2(m_verticalSize, m_skinWidth);
		PhysicsCastData result = default(PhysicsCastData);
		result.m_active = true;
		result.m_position = position;
		result.m_direction = down;
		result.m_distance = 0.5f;
		result.m_size = size;
		return result;
	}

	private void AscendSlopeCheck(ref Vector2 moveAmount)
	{
		if (Mathf.Approximately(moveAmount.x, 0f))
		{
			m_ascendSlopeCastData.m_active = false;
			return;
		}
		Mathf.Sign(moveAmount.y);
		m_ascendSlopeCastData = GetAscendSlopeCastData(moveAmount);
		int hitCount = PerformMovementBoxCast(m_ascendSlopeCastData);
		RaycastHit2D bestHit = GetBestHit(hitCount);
		if (!bestHit)
		{
			return;
		}
		float num = Vector2.Angle(bestHit.normal, Vector2.up);
		if (!(num >= 0.1f) || !(num < m_maxSlopeAngle))
		{
			return;
		}
		if (Mathf.Sign(bestHit.normal.x) != Mathf.Sign(moveAmount.x))
		{
			m_collisions.m_slopeAngle = num;
			m_collisions.m_slopeNormal = bestHit.normal;
			m_collisions.m_below = true;
			m_collisions.m_climbingSlope = true;
			Vector2 normalized = Vector2.Perpendicular(bestHit.normal).normalized;
			if (normalized.y < 0f)
			{
				normalized *= -1f;
			}
			moveAmount = normalized * Mathf.Abs(moveAmount.x);
		}
		TryRegisterFloorSurface(bestHit);
	}

	private RaycastHit2D GetBestHit(int hitCount, bool checkForCharacters = false)
	{
		for (int i = 0; i < hitCount; i++)
		{
			RaycastHit2D raycastHit2D = m_raycastHits[i];
			if (ShouldIgnoreColliderForRaycast(raycastHit2D) || !(raycastHit2D.collider != null))
			{
				continue;
			}
			if (checkForCharacters)
			{
				CharacterMovement componentInParent = raycastHit2D.collider.GetComponentInParent<CharacterMovement>();
				MovementBlocker componentInParent2 = raycastHit2D.collider.GetComponentInParent<MovementBlocker>();
				MovementBlockingType movementBlockingType = MovementBlockingType.None;
				if (componentInParent != null)
				{
					movementBlockingType = componentInParent.BlockingType;
					if (componentInParent.AllowCharacterPassthrough && m_movementBlockingType != MovementBlockingType.EnemyMassive)
					{
						continue;
					}
				}
				else if (componentInParent2 != null)
				{
					movementBlockingType = componentInParent2.BlockingType;
				}
				if (componentInParent == this || ((componentInParent != null || componentInParent2 != null) && (movementBlockingType == MovementBlockingType.None || BlockingType == MovementBlockingType.None || movementBlockingType == BlockingType || (movementBlockingType == MovementBlockingType.Enemy && BlockingType == MovementBlockingType.EnemyMassive) || (BlockingType == MovementBlockingType.Enemy && movementBlockingType == MovementBlockingType.EnemyMassive) || (AllowCharacterPassthrough && movementBlockingType != MovementBlockingType.EnemyMassive))))
				{
					continue;
				}
			}
			return raycastHit2D;
		}
		return default(RaycastHit2D);
	}

	public bool SnapToGround()
	{
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.SetLayerMask(CollisionMask);
		Vector2 origin = base.transform.position;
		origin.y += 0.5f;
		int hitCount = Physics2D.BoxCast(size: new Vector2(0.5f, 0.01f), origin: origin, angle: 0f, direction: Vector2.down, contactFilter: contactFilter, results: m_raycastHits, distance: 1f);
		RaycastHit2D bestHit = GetBestHit(hitCount);
		if ((bool)bestHit)
		{
			Vector3 position = base.transform.position;
			position.y = bestHit.point.y;
			base.transform.position = position;
			m_collisions.m_below = true;
			m_offGroundTimer = 0f;
			m_snapToGroundIgnoreYTimer = 0.05f;
			return true;
		}
		return false;
	}

	public void ApplyMovementFromAnimRootNode()
	{
		if (m_animRootNode != null)
		{
			Vector3 position = base.transform.position;
			position += m_animRootNode.transform.localPosition;
			PositionAndUndoAnimRootNode(position);
		}
		else
		{
			Debug.LogWarning(base.gameObject.name + " is missing AnimRootNode in CharacterMovement component", this);
		}
	}

	public void PositionAndUndoAnimRootNode(Vector3 position)
	{
		if (Vector3.Distance(position, base.transform.position) > 0.1f)
		{
			m_ignoreNextUpdate = true;
		}
		Rigidbody2D rigidbody = m_rigidbody;
		Vector3 vector2 = (base.transform.position = position);
		rigidbody.position = vector2;
		m_animRootNode.DoResetFrame();
	}

	public void TeleportToPosition(Vector3 position, bool snapToGround)
	{
		if (snapToGround)
		{
			Vector3 vector = position;
			vector.y += 0.1f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, 10f, GameLayers.EnvironmentMask);
			if ((bool)raycastHit2D)
			{
				position = raycastHit2D.point;
			}
		}
		base.transform.position = position;
		m_previousVelocity = Vector2.zero;
	}

	private void OnGUI()
	{
		if (m_showMovementDebug && GameUtils.IsPlayer(base.gameObject))
		{
			Vector2 previousVelocity = m_previousVelocity;
			GUILayout.Label("Previous Velocity: " + previousVelocity.ToString());
			GUILayout.Space(4f);
			GUILayout.Label("Slope Angle: " + m_collisions.m_slopeAngle);
			GUILayout.Label("Climbing Slope: " + m_collisions.m_climbingSlope);
			GUILayout.Label("Descending Slope: " + m_collisions.m_descendingSlope);
			GUILayout.Label("Sliding: " + m_collisions.m_slidingDownMaxSlope);
			GUILayout.Space(4f);
			GUILayout.Label("-- Collisions Detected -- ");
			GUILayout.Label("Right: " + m_collisions.m_right);
			GUILayout.Label("Left: " + m_collisions.m_left);
			GUILayout.Label("Above: " + m_collisions.m_above);
			GUILayout.Label("Below: " + m_collisions.m_below);
			GUILayout.Label("Near Ground: " + m_collisions.m_isNearGround);
			GUILayout.Label("-- Overlaps -- ");
			GUILayout.Label("Horizontal: " + m_collisions.m_overlappingHorizontal + " Vertical: " + m_collisions.m_overlappingVertical);
			GUILayout.Label("-- Surface -- ");
			if (m_currentSurfaceCollider != null && CurrentSurfaceSettings != null)
			{
				GUILayout.Label("Surface: " + CurrentSurfaceSettings.name + " | " + m_currentSurfaceCollider.name);
			}
			if (m_waterSurface != null)
			{
				GUILayout.Label("Water Surface: " + m_waterSurface.name);
			}
		}
	}

	private void ClearSurface()
	{
		m_currentSurfaceType = null;
		m_currentSurfaceCollider = null;
	}

	private void TryRegisterFloorSurface(RaycastHit2D hit)
	{
		Collider2D collider = hit.collider;
		if (collider != null)
		{
			SurfaceType component = collider.GetComponent<SurfaceType>();
			if ((object)component != null)
			{
				m_currentSurfaceCollider = collider;
				m_currentSurfaceType = component;
			}
			Stairs component2 = hit.collider.GetComponent<Stairs>();
			if (component2 != null)
			{
				m_collisions.m_touchedStairs = component2;
			}
		}
	}

	private void CheckForNearGround()
	{
		if (m_collisions.m_below)
		{
			m_collisions.m_isNearGround = true;
			return;
		}
		Bounds bounds = Collider.bounds;
		float num = Mathf.Abs(bounds.extents.y) + m_isNearGroundDistance;
		Vector2 vector = bounds.center;
		Vector2 size = new Vector2(bounds.extents.x, m_skinWidth);
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(vector, size, 0f, Vector2.down, num, CollisionMask);
		Debug.DrawRay(vector, Vector2.down * num, Color.magenta);
		if (raycastHit2D.collider != null)
		{
			m_collisions.m_isNearGround = true;
		}
	}

	private void CheckForWaterSurfaces()
	{
		SurfaceType surfaceType = null;
		Collider2D[] array = Physics2D.OverlapBoxAll(Collider.bounds.center, Collider.bounds.size, 0f, 1 << GameLayers.WaterLayer);
		for (int i = 0; i < array.Length; i++)
		{
			SurfaceType component = array[i].GetComponent<SurfaceType>();
			if ((bool)array[i].GetComponent<SurfaceType>())
			{
				surfaceType = component;
			}
		}
		if (m_waterSurface != surfaceType)
		{
			m_waterSurface = surfaceType;
			m_onWaterSurfaceChanged?.Invoke(m_waterSurface);
		}
	}

	private float GetZDepthForCollider(Collider2D collider)
	{
		DepthMovement component = collider.GetComponent<DepthMovement>();
		if (component != null)
		{
			return component.GetZDepth(base.transform.position);
		}
		return collider.transform.position.z;
	}

	private void UpdateDepth()
	{
		if (m_fixedDepthMovement)
		{
			return;
		}
		bool flag = false;
		Vector3 position = base.transform.position;
		float num = position.z;
		if (m_zForceDepthOverride != 0f)
		{
			num = m_zForceDepthOverride;
		}
		else if ((bool)m_activeSwimmingWaterBound)
		{
			num = m_activeSwimmingWaterBound.GetZDepth();
		}
		else if (m_collisions.m_below && m_currentSurfaceCollider != null)
		{
			num = GetZDepthForCollider(m_currentSurfaceCollider);
		}
		else
		{
			Vector3 position2 = base.transform.position;
			position2.y += 1f;
			RaycastHit2D[] array = Physics2D.RaycastAll(position2, Vector2.down, 2f, CollisionMask);
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			float num2 = float.MaxValue;
			RaycastHit2D[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit2D raycastHit2D2 = array2[i];
				float num3 = Vector2.Distance(raycastHit2D2.point, base.transform.position);
				if (num3 < num2)
				{
					raycastHit2D = raycastHit2D2;
					num2 = num3;
				}
			}
			if ((bool)raycastHit2D.collider)
			{
				num = GetZDepthForCollider(raycastHit2D.collider);
			}
			else
			{
				flag = true;
			}
		}
		if (!flag)
		{
			num += DepthStateOffset + m_zOffset;
			if (m_useRandomZOffsetAdjustment)
			{
				num += m_randomZOffset;
			}
			position.z = Mathf.MoveTowards(position.z, num, Time.deltaTime * ZMaxMoveSpeed);
			base.transform.position = position;
		}
	}

	public float GetDistanceFromGround()
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, 10f, GameLayers.CharacterNavigationMask);
		if ((bool)raycastHit2D)
		{
			return raycastHit2D.distance;
		}
		return -1f;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		if (m_persistentPosition)
		{
			m_persistentDataObject = dataEntry;
			m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
			if (m_persistentData == null)
			{
				m_persistentData = new PersistentData();
				m_persistentDataObject.Data = m_persistentData;
			}
		}
	}

	public void PostAllReceivedDatastoreEntries()
	{
		if (m_persistentData == null || !m_persistentData.m_hasPosition)
		{
			return;
		}
		Vector3 position = m_persistentData.m_position;
		PersistentPositionBlocker[] array = UnityEngine.Object.FindObjectsByType<PersistentPositionBlocker>(FindObjectsSortMode.None);
		foreach (PersistentPositionBlocker persistentPositionBlocker in array)
		{
			if (persistentPositionBlocker.ContainsPosition(position))
			{
				Vector3 vector = persistentPositionBlocker.GetPreferredLocation();
				ContactFilter2D contactFilter = default(ContactFilter2D);
				contactFilter.SetLayerMask(CollisionMask);
				Vector2 origin = vector;
				origin.y += 0.5f;
				int hitCount = Physics2D.BoxCast(size: new Vector2(0.5f, 0.01f), origin: origin, angle: 0f, direction: Vector2.down, contactFilter: contactFilter, results: m_raycastHits, distance: 1f);
				RaycastHit2D bestHit = GetBestHit(hitCount);
				if ((bool)bestHit)
				{
					vector = bestHit.point;
				}
				position.x = vector.x;
				position.y = vector.y;
				string text = base.gameObject.name;
				Vector3 vector2 = position;
				Debug.Log("Character " + text + " tried to start too close to PersistentPositionBlocker, moving to new position " + vector2.ToString());
				break;
			}
		}
		base.transform.SetPositionAndRotation(position, m_persistentData.m_rotation);
		m_rigidbody.linearVelocity = m_persistentData.m_velocity;
	}

	private void UpdatePersistentData()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_position = base.transform.position;
			m_persistentData.m_rotation = base.transform.rotation;
			m_persistentData.m_velocity = m_rigidbody.linearVelocity;
			m_persistentData.m_hasPosition = true;
		}
	}

	public void StartMigration()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_hasPosition = false;
			base.enabled = false;
		}
	}

	public bool CanUseOptionalStairs()
	{
		if (m_overlappingStairs != null && m_overlappingStairs.Count > 0 && m_attachedStairs == null && m_stairsDirection != 0)
		{
			return true;
		}
		return false;
	}

	public void DetachFromStairs()
	{
		m_attachedStairs = null;
		m_overlappingStairs.Clear();
	}

	private void CheckForStairs(Vector2 input)
	{
		if (m_forcedMovementStairsBehaviour)
		{
			if (m_previousVelocity != Vector2.zero)
			{
				input.x = m_previousVelocity.x;
			}
			else
			{
				input.x = m_direction.GetForwardVector().x;
			}
		}
		bool flag = false;
		Bounds bounds = Collider.bounds;
		Vector3 max = bounds.max;
		max.y = bounds.min.y + 0.2f;
		bounds.max = max;
		StairsSet stairsSet = GlobalReferences.Instance.Sets.Generic.StairsSet;
		Stairs stairs = null;
		OverlappingStairs.Clear();
		StairsDirection = AvailableStairsDirection.None;
		foreach (Stairs item in stairsSet)
		{
			if (item.TopBounds.Intersects(bounds))
			{
				OverlappingStairs.Add(item);
			}
			else if (item.BottomBounds.Intersects(bounds))
			{
				OverlappingStairs.Add(item);
			}
			else if (item.StairsCollider != null && IsTouchingCollider(item.StairsCollider))
			{
				stairs = item;
				flag = true;
			}
		}
		if (!(stairs != null) && OverlappingStairs.Count <= 0)
		{
			return;
		}
		bool flag2 = false;
		if (flag)
		{
			flag2 = true;
		}
		else
		{
			foreach (Stairs overlappingStair in OverlappingStairs)
			{
				bool flag3 = false;
				if (overlappingStair.TopBounds.Intersects(bounds))
				{
					StairsDirection |= AvailableStairsDirection.TravelDown;
					flag3 = true;
				}
				else if (overlappingStair.BottomBounds.Intersects(bounds))
				{
					StairsDirection |= AvailableStairsDirection.TravelUp;
					flag3 = false;
				}
				if (!(input.magnitude > GameUtils.Constants.s_inputMoveDeadzoneMinValue) && !m_forcedMovementStairsBehaviour)
				{
					continue;
				}
				if (flag3)
				{
					Vector2 downDirection = overlappingStair.DownDirection;
					if (CanGoDownStairs(input, overlappingStair, downDirection))
					{
						flag2 = true;
						stairs = overlappingStair;
						break;
					}
				}
				else
				{
					Vector2 upDirection = overlappingStair.UpDirection;
					if (CanGoUpStairs(input, overlappingStair, upDirection))
					{
						flag2 = true;
						stairs = overlappingStair;
						break;
					}
				}
			}
		}
		if (flag2)
		{
			AttachToStairs(stairs);
		}
	}

	private static bool CanGoUpStairs(Vector2 input, Stairs overlappingStairs, Vector2 upDirection)
	{
		bool num = Mathf.Sign(input.x) == Mathf.Sign(upDirection.x) || Mathf.Abs(input.x) < GameUtils.Constants.s_inputMoveDeadzoneMinValue;
		bool flag = false;
		if (overlappingStairs.BottomStairsPassableMode == Stairs.PassableMode.AlwaysUseStairs)
		{
			flag = true;
		}
		else if (overlappingStairs.BottomStairsPassableMode == Stairs.PassableMode.UseOnNeutral && input.y > 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			flag = true;
		}
		else if (input.y > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			flag = true;
		}
		return num && flag;
	}

	private static bool CanGoDownStairs(Vector2 input, Stairs overlappingStairs, Vector2 downDirection)
	{
		bool num = Mathf.Sign(input.x) == Mathf.Sign(downDirection.x) || Mathf.Abs(input.x) < GameUtils.Constants.s_inputMoveDeadzoneMinValue;
		bool flag = input.y < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue || overlappingStairs.TopStairsPassableMode == Stairs.PassableMode.AlwaysUseStairs;
		if (overlappingStairs.TopStairsPassableMode == Stairs.PassableMode.AlwaysUseStairs)
		{
			flag = true;
		}
		else if (overlappingStairs.TopStairsPassableMode == Stairs.PassableMode.UseOnNeutral && input.y < GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			flag = true;
		}
		else if (input.y < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			flag = true;
		}
		return num && flag;
	}

	private void AttachToStairs(Stairs stairs)
	{
		if (!(m_attachedStairs == stairs))
		{
			Stairs attachedStairs = m_attachedStairs;
			m_attachedStairs = stairs;
			m_overlappingStairs.Clear();
			m_isUsingStairs = ((stairs != null) ? true : false);
			if (stairs != null)
			{
				m_stairsForceAttachTimer = 0.2f;
				CorrectToStairsHeight();
			}
			else if (attachedStairs != null && Vector2.Dot(m_previousVelocity.normalized, attachedStairs.UpDirection) >= 0.5f && Vector2.Distance(base.transform.position, attachedStairs.TopStairsWorldPosition) < 0.25f)
			{
				Vector3 position = base.transform.position;
				position.x = attachedStairs.TopStairsWorldPosition.x;
				position.y = Mathf.Max(base.transform.position.y, attachedStairs.TopStairsWorldPosition.y);
				base.transform.position = position;
			}
		}
	}

	private void CorrectToStairsHeight()
	{
		if (AttachedStairs != null)
		{
			Vector3 snapPosition = AttachedStairs.GetSnapPosition(base.transform.position);
			Vector3 position = base.transform.position;
			position.y = snapPosition.y;
			base.transform.position = position;
		}
	}

	public bool IsJumpEnabled()
	{
		if (m_isUsingStairs)
		{
			return false;
		}
		if (OverlappingStairs.Count > 0)
		{
			return false;
		}
		return true;
	}

	private void DrawCastDataGizmos(PhysicsCastData castData, Color color)
	{
		if (castData.m_active)
		{
			Color color2 = color;
			color2.a = 0.6f;
			Gizmos.color = color2;
			Vector3 vector = castData.m_position;
			Vector3 size = castData.m_size;
			size.z = 0.1f;
			Gizmos.DrawCube(vector, size);
			Vector3 vector2 = castData.m_direction * castData.m_distance;
			Vector3 center = vector + vector2;
			Gizmos.color = color2 * 0.5f;
			Gizmos.DrawWireCube(center, size);
			Gizmos.color = Color.white;
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere((Vector2)base.transform.position + m_predictedPositionOffset, 0.05f);
		}
		if (m_showMovementDebug)
		{
			DrawCastDataGizmos(m_verticalCastData, Color.blue);
			DrawCastDataGizmos(m_horizontalBodyCastData, Color.yellow);
			DrawCastDataGizmos(m_horizontalFootCastData, Color.yellow * 0.75f);
			DrawCastDataGizmos(m_horizontalCharacterCastData, Color.magenta);
			DrawCastDataGizmos(m_ascendSlopeCastData, Color.green);
			DrawCastDataGizmos(m_descendSlopeCastData, Color.red);
		}
		Gizmos.color = Color.white;
	}

	private void OnDrawGizmosSelected()
	{
		if (m_useMovementRestrictions)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(new Vector3(m_minMaxX.x, -5f, 0f), new Vector3(m_minMaxX.x, 5f, 0f));
			Gizmos.DrawLine(new Vector3(m_minMaxX.y, -5f, 0f), new Vector3(m_minMaxX.y, 5f, 0f));
			Gizmos.color = Color.white;
		}
	}
}
