using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class LegacyCharacterMovement : MonoBehaviour, IPersistentComponent
{
	[Serializable]
	public struct LadderSettings
	{
		public float m_characterHeightForLadderTop;

		public float m_characterHeightForLadderBottom;
	}

	public enum CharacterKinematicMode
	{
		Normal,
		NeverKinematic,
		AlwaysKinematic
	}

	public struct LadderRopeState
	{
		public bool m_isAtTop;

		public bool m_isAtBottom;

		public bool m_isNearBottom;
	}

	public enum MovementOrientation
	{
		None,
		NormalFloor,
		InvertedCeiling
	}

	[Serializable]
	private class PersistentData
	{
		public Vector3 m_position;

		public Quaternion m_rotation;

		public Vector2 m_velocity;
	}

	[Header("Movement")]
	[SerializeField]
	private CharacterMovementSettings m_defaultMovementSettings;

	[SerializeField]
	private UnityEvent m_onJumpEvent;

	[Tooltip("How long until a character is considered falling after we detect they are no longer on the ground")]
	[SerializeField]
	private float m_coyoteTime = 0.4f;

	[Header("Collision Check")]
	[SerializeField]
	private Transform m_groundCheckTransform;

	[SerializeField]
	private Vector2 m_groundCheckBoxSize;

	[Header("Collider Reference")]
	[FormerlySerializedAs("m_capsuleCollider")]
	[FormerlySerializedAs("m_collider")]
	[SerializeField]
	private Collider2D m_movementCollider;

	[Header("Sets")]
	[SerializeField]
	private StairsSet m_stairsSet;

	[SerializeField]
	private DropPlatformSet m_dropPlatformSet;

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

	[Header("Rope")]
	[SerializeField]
	private Vector2 m_ropeClimbOffset;

	[Header("Ramps")]
	[SerializeField]
	private float m_maxRampAngle = 40f;

	[Header("Anim Root")]
	[SerializeField]
	private SpriteAnimNodeFollower m_animRootNode;

	[Header("Predicted Position")]
	[SerializeField]
	private float m_predictedPositionTrackSpeed = 1f;

	[Header("Ladder")]
	[SerializeField]
	private LadderSettings m_ladderSettings;

	[Header("Movement Mask")]
	[SerializeField]
	private bool m_useCatEnvironmentMask;

	[Header("Sliding")]
	[SerializeField]
	private bool m_canSlide;

	[Header("Align to Floor")]
	[SerializeField]
	private bool m_alignToFloor;

	[SerializeField]
	private float m_alignToFloorLimitAngle = 30f;

	[SerializeField]
	private float m_alignToFloorSpeed = 10f;

	private float m_currentFloorAlignmentAngle;

	private Rigidbody2D m_rigidbody;

	private CharacterStance m_characterStance;

	private CharacterDirection m_characterDirection;

	private BaseCharacterInput m_input;

	private CharacterMovementSettings m_movementSettingsOverride;

	private CharacterKinematicMode m_kinematicMode;

	private bool m_isGrounded;

	private float m_offGroundTimer;

	private float m_forceFallingTimer;

	private float m_zStateOffset;

	private Ladder m_attachedLadder;

	private Ladder m_previousLadder;

	private ClimbableRope m_attachedRope;

	private ClimbableRope m_previousRope;

	private List<DropPlatform> m_ignoredPlatforms;

	private List<Collider2D> m_touchedColliders;

	private List<Stairs> m_touchedStairs;

	private JunctionPath m_activeJunctionPath;

	private float m_junctionPathTouchTimer;

	private Collider2D[] m_cachedColliders;

	private Collider2D m_movementLimitCollider;

	private SurfaceType m_currentSurfaceType;

	private Collider2D m_currentSurfaceCollider;

	private bool m_disableStickiness;

	private Coroutine m_stickinessCoroutine;

	private MovingPlatform m_activeMovingPlatform;

	private bool m_isStandingOnDropPlatform;

	private bool m_isDroppingThroughDropPlatforms;

	private bool m_isStandingOnMovingPlatform;

	private SlidePlatform m_currentSlidePlatform;

	private bool m_isInMovementState;

	public UnityAction<Collision2D> m_onCollision;

	private bool m_fixedDepthMovement;

	private SurfaceType m_waterSurface;

	public UnityAction<SurfaceType> m_onWaterSurfaceChanged;

	private Vector2 m_predictedPositionOffset;

	private static readonly float s_snapToGroundVelocity = 2f;

	private LadderRopeState m_currentLaderRopeState;

	private Vector2 m_movementInput;

	private Vector2 m_movementOverride;

	private MovementOrientation m_movementOrientation;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private Vector2 m_floorNormal;

	public int EnvironmentLayerMask => ((m_useCatEnvironmentMask ? GameLayers.CatEnvironmentMask : GameLayers.EnvironmentMask) | (int)m_rigidbody.includeLayers) & ~(int)m_rigidbody.excludeLayers;

	public CharacterKinematicMode KinematicMode
	{
		get
		{
			return m_kinematicMode;
		}
		set
		{
			m_kinematicMode = value;
		}
	}

	public bool IsGrounded
	{
		get
		{
			if (!m_isGrounded)
			{
				return m_offGroundTimer < m_coyoteTime;
			}
			return true;
		}
	}

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

	public bool IsForceFall => m_forceFallingTimer > 0f;

	public Ladder AttachedLadder => m_attachedLadder;

	public SurfaceType CurrentSurfaceType
	{
		get
		{
			if (!(m_waterSurface != null))
			{
				return m_currentSurfaceType;
			}
			return m_waterSurface;
		}
	}

	public Collider2D CurrentSurfaceCollider => m_currentSurfaceCollider;

	public bool IsSliding
	{
		get
		{
			if (m_canSlide && m_currentSlidePlatform != null)
			{
				return !m_disableStickiness;
			}
			return false;
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

	public SurfaceType CurrentWaterSurface => m_waterSurface;

	public Vector2 PredictedPositionOffsetSmoothed => m_predictedPositionOffset;

	public bool IsInMovementState
	{
		get
		{
			return m_isInMovementState;
		}
		set
		{
			m_isInMovementState = value;
		}
	}

	public LadderRopeState CurrentLadderRopeState => m_currentLaderRopeState;

	public Vector2 MovementOverride
	{
		get
		{
			return m_movementOverride;
		}
		set
		{
			m_movementOverride = value;
		}
	}

	public Vector2 MovementInput
	{
		get
		{
			if (m_movementOverride != Vector2.zero)
			{
				return m_movementOverride;
			}
			return m_movementInput;
		}
		set
		{
			if (value != m_movementInput)
			{
				m_movementInput = value;
			}
		}
	}

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

	public MovementOrientation MovementOrientationMode
	{
		get
		{
			return m_movementOrientation;
		}
		set
		{
			if (m_movementOrientation == value)
			{
				return;
			}
			m_movementOrientation = value;
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			switch (m_movementOrientation)
			{
			case MovementOrientation.NormalFloor:
				if (componentInChildren != null)
				{
					componentInChildren.flipY = false;
				}
				break;
			case MovementOrientation.InvertedCeiling:
				if (componentInChildren != null)
				{
					componentInChildren.flipY = true;
				}
				break;
			}
			SetDefaultGravityScale();
		}
	}

	public Vector2 RelativeVelocity
	{
		get
		{
			Vector2 linearVelocity = m_rigidbody.linearVelocity;
			if (m_activeMovingPlatform != null)
			{
				linearVelocity -= m_activeMovingPlatform.Velocity;
			}
			return linearVelocity;
		}
	}

	public Ladder GetLadder()
	{
		if (m_attachedLadder != null)
		{
			return m_attachedLadder;
		}
		return m_previousLadder;
	}

	public ClimbableRope GetRope()
	{
		if (m_attachedRope != null)
		{
			return m_attachedRope;
		}
		return m_previousRope;
	}

	public void SetMovementSettingsOverride(CharacterMovementSettings settings)
	{
		m_movementSettingsOverride = settings;
	}

	public void SetDefaultGravityScale()
	{
		float gravityScale = 1f;
		if (m_movementOrientation == MovementOrientation.InvertedCeiling)
		{
			gravityScale = -1f;
		}
		m_rigidbody.gravityScale = gravityScale;
	}

	public void DisableGravity()
	{
		m_rigidbody.gravityScale = 0f;
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	public void PostAllReceivedDatastoreEntries()
	{
		if (m_persistentData != null)
		{
			bool flag = false;
			CharacterHealth component = GetComponent<CharacterHealth>();
			if (component == null || !component.IsDead)
			{
				flag = true;
			}
			if (!flag)
			{
				base.transform.SetPositionAndRotation(m_persistentData.m_position, m_persistentData.m_rotation);
				m_rigidbody.linearVelocity = m_persistentData.m_velocity;
			}
		}
	}

	private void UpdatePersistenData()
	{
		if (m_persistentData != null)
		{
			m_persistentData.m_position = base.transform.position;
			m_persistentData.m_rotation = base.transform.rotation;
			m_persistentData.m_velocity = m_rigidbody.linearVelocity;
		}
	}

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody2D>();
		m_characterStance = GetComponent<CharacterStance>();
		m_characterDirection = GetComponent<CharacterDirection>();
		m_input = base.gameObject.GetCharacterInputComponent();
		m_touchedStairs = new List<Stairs>();
		m_ignoredPlatforms = new List<DropPlatform>();
		m_touchedColliders = new List<Collider2D>();
		m_cachedColliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
		MovementOrientationMode = MovementOrientation.NormalFloor;
		m_predictedPositionOffset = Vector2.zero;
		m_randomZOffset = UnityEngine.Random.Range(0f, 0.1f);
	}

	private void Update()
	{
		m_movementOverride = Vector2.zero;
		m_isGrounded = false;
		m_isStandingOnDropPlatform = false;
		m_isStandingOnMovingPlatform = false;
		m_currentSlidePlatform = null;
		m_currentSurfaceType = null;
		m_currentSurfaceCollider = null;
		Vector2 linearVelocity = m_rigidbody.linearVelocity;
		m_predictedPositionOffset = Vector2.MoveTowards(m_predictedPositionOffset, linearVelocity, Time.deltaTime * m_predictedPositionTrackSpeed);
		List<Collider2D> list = new List<Collider2D>();
		Collider2D[] array = Physics2D.OverlapBoxAll(m_groundCheckTransform.position, m_groundCheckBoxSize, 0f, EnvironmentLayerMask);
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].GetComponent<DropPlatform>())
			{
				m_isStandingOnDropPlatform = true;
			}
			if ((bool)array[i].GetComponent<MovingPlatform>())
			{
				m_isStandingOnMovingPlatform = true;
			}
			SlidePlatform component = array[i].GetComponent<SlidePlatform>();
			if ((object)component != null)
			{
				m_currentSlidePlatform = component;
			}
			SurfaceType component2 = array[i].GetComponent<SurfaceType>();
			if ((object)component2 != null)
			{
				m_currentSurfaceCollider = array[i];
				m_currentSurfaceType = component2;
			}
			if (array[i].gameObject != base.gameObject)
			{
				m_isGrounded = true;
			}
		}
		Collider2D collider = GetCollider();
		Collider2D[] array2 = Physics2D.OverlapBoxAll(collider.bounds.center, collider.bounds.size, 0f, EnvironmentLayerMask);
		for (int j = 0; j < array2.Length; j++)
		{
			if ((bool)array2[j].GetComponent<DropPlatform>())
			{
				list.Add(array2[j]);
			}
		}
		SurfaceType surfaceType = null;
		Collider2D[] array3 = Physics2D.OverlapBoxAll(collider.bounds.center, collider.bounds.size, 0f, 1 << GameLayers.WaterLayer);
		for (int k = 0; k < array3.Length; k++)
		{
			SurfaceType component3 = array3[k].GetComponent<SurfaceType>();
			if ((bool)array3[k].GetComponent<SurfaceType>())
			{
				surfaceType = component3;
			}
		}
		if (m_waterSurface != surfaceType)
		{
			m_waterSurface = surfaceType;
			m_onWaterSurfaceChanged?.Invoke(m_waterSurface);
		}
		if (m_isGrounded)
		{
			m_offGroundTimer = 0f;
		}
		else
		{
			m_offGroundTimer += Time.deltaTime;
		}
		if (m_forceFallingTimer > 0f)
		{
			m_forceFallingTimer -= Time.deltaTime;
			if (m_forceFallingTimer <= 0f && m_isGrounded)
			{
				m_onCollision?.Invoke(null);
			}
		}
		UpdateDropPlatforms(list);
		UpdatePersistenData();
		UpdateJunctionPath();
	}

	private void UpdateDropPlatforms(List<Collider2D> collidedPlatforms)
	{
		m_isDroppingThroughDropPlatforms = m_input.MovementInput.y < -0.75f;
		if (!(m_dropPlatformSet != null))
		{
			return;
		}
		if (m_isDroppingThroughDropPlatforms)
		{
			foreach (DropPlatform item in m_dropPlatformSet.Items)
			{
				Collider2D collider = item.Collider;
				if (!m_ignoredPlatforms.Contains(item))
				{
					SetIgnoreCollider(collider, ignore: true);
					m_ignoredPlatforms.Add(item);
				}
			}
		}
		for (int i = 0; i < m_ignoredPlatforms.Count; i++)
		{
			if (m_isDroppingThroughDropPlatforms)
			{
				continue;
			}
			if (m_ignoredPlatforms[i] == null)
			{
				m_ignoredPlatforms.RemoveAt(i--);
				continue;
			}
			bool flag = false;
			for (int j = 0; j < collidedPlatforms.Count; j++)
			{
				if (collidedPlatforms[j] == m_ignoredPlatforms[i].Collider)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				SetIgnoreCollider(m_ignoredPlatforms[i].Collider, ignore: false);
				m_ignoredPlatforms.RemoveAt(i--);
			}
		}
	}

	private void SlideMovement()
	{
		m_rigidbody.linearVelocity = m_currentSlidePlatform.GetSlideDirection() * ActiveMovementSettings.MoveSpeed;
	}

	private void FixedUpdate()
	{
		if ((bool)m_attachedLadder)
		{
			LadderMovement();
			return;
		}
		if ((bool)m_attachedRope)
		{
			RopeMovement();
			return;
		}
		if (IsSliding)
		{
			SlideMovement();
			return;
		}
		RegularMovement();
		if (!m_fixedDepthMovement)
		{
			Vector3 position = base.transform.position;
			position.y += 1f;
			RaycastHit2D[] array = Physics2D.RaycastAll(position, Vector2.down, 2f, EnvironmentLayerMask);
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			float num = float.MaxValue;
			RaycastHit2D[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit2D raycastHit2D2 = array2[i];
				float num2 = Vector2.Distance(raycastHit2D2.point, base.transform.position);
				if (num2 < num)
				{
					raycastHit2D = raycastHit2D2;
					num = num2;
				}
			}
			if ((bool)raycastHit2D.collider)
			{
				raycastHit2D.collider.GetComponent<Renderer>();
				DepthMovement component = raycastHit2D.collider.GetComponent<DepthMovement>();
				float num3 = ((!(component != null)) ? raycastHit2D.collider.transform.position.z : component.GetZDepth(base.transform.position));
				num3 += DepthStateOffset + m_zOffset;
				if (m_useRandomZOffsetAdjustment)
				{
					num3 += m_randomZOffset;
				}
				Vector3 position2 = base.transform.position;
				position2.z = Mathf.MoveTowards(position2.z, num3, Time.deltaTime * m_zMaxMoveSpeed);
				base.transform.position = position2;
			}
		}
		if (m_alignToFloor)
		{
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(m_groundCheckTransform.position, Vector2.down, m_groundCheckBoxSize.y + 0.5f, EnvironmentLayerMask);
			Vector2 to = ((raycastHit2D3.collider != null) ? raycastHit2D3.normal : Vector2.up);
			float num4 = Vector2.Angle(Vector2.up, to);
			m_currentFloorAlignmentAngle = Mathf.MoveTowardsAngle(target: (!(num4 >= m_alignToFloorLimitAngle)) ? num4 : 0f, current: m_currentFloorAlignmentAngle, maxDelta: Time.deltaTime * m_alignToFloorSpeed);
			base.transform.rotation = Quaternion.Euler(0f, 0f, m_currentFloorAlignmentAngle);
		}
	}

	public LadderRopeState GetLadderState(Ladder ladder)
	{
		LadderRopeState result = default(LadderRopeState);
		Vector3 bottomBound = ladder.BottomBound;
		Vector3 topBound = ladder.TopBound;
		result.m_isAtBottom = bottomBound.y >= base.transform.position.y + m_ladderSettings.m_characterHeightForLadderBottom;
		result.m_isNearBottom = bottomBound.y + 1f >= base.transform.position.y + m_ladderSettings.m_characterHeightForLadderBottom;
		result.m_isAtTop = topBound.y <= base.transform.position.y + m_ladderSettings.m_characterHeightForLadderTop;
		return result;
	}

	public LadderRopeState GetRopeState(ClimbableRope rope)
	{
		LadderRopeState result = default(LadderRopeState);
		Vector3 bottomBound = rope.BottomBound;
		Vector3 topBound = rope.TopBound;
		Bounds standingBounds = m_characterStance.GetStandingBounds();
		result.m_isAtBottom = bottomBound.y >= standingBounds.min.y;
		result.m_isAtTop = topBound.y <= standingBounds.max.y;
		return result;
	}

	private void LadderMovement()
	{
		Vector2 movementInput = MovementInput;
		movementInput.x = 0f;
		movementInput.y = Mathf.Round(movementInput.y);
		m_currentLaderRopeState = GetLadderState(m_attachedLadder);
		if (m_currentLaderRopeState.m_isAtBottom)
		{
			movementInput.y = Mathf.Max(0f, movementInput.y);
		}
		if (m_currentLaderRopeState.m_isAtTop)
		{
			movementInput.y = Mathf.Min(0f, movementInput.y);
		}
		m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
		m_rigidbody.linearVelocity = ActiveMovementSettings.MoveSpeed * movementInput;
		float z = m_attachedLadder.transform.position.z;
		z += DepthStateOffset;
		Vector3 position = base.transform.position;
		position.z = Mathf.MoveTowards(position.z, z, Time.deltaTime * m_zMaxMoveSpeed);
		base.transform.position = position;
	}

	private void RopeMovement()
	{
		Vector2 movementInput = MovementInput;
		movementInput.x = 0f;
		m_currentLaderRopeState = GetRopeState(m_attachedRope);
		if (m_currentLaderRopeState.m_isAtBottom)
		{
			movementInput.y = Mathf.Max(0f, movementInput.y);
		}
		if (m_currentLaderRopeState.m_isAtTop)
		{
			movementInput.y = Mathf.Min(0f, movementInput.y);
		}
		Vector2 linearVelocity = ActiveMovementSettings.MoveSpeed * movementInput;
		if (movementInput.y < 0f)
		{
			linearVelocity.y *= 3f;
		}
		m_rigidbody.linearVelocity = linearVelocity;
		Vector2 ropeClimbOffset = m_ropeClimbOffset;
		if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Left)
		{
			ropeClimbOffset.x *= -1f;
		}
		Vector2 position = m_rigidbody.position;
		position += ropeClimbOffset;
		position.x = m_attachedRope.GetXForPosition(position);
		position -= ropeClimbOffset;
		m_rigidbody.position = position;
	}

	private void RegularMovement()
	{
		Vector2 origin = m_groundCheckTransform.transform.position;
		origin.y += m_groundCheckBoxSize.y;
		RaycastHit2D[] array = Physics2D.BoxCastAll(origin, m_groundCheckBoxSize, 0f, Vector2.down, m_groundCheckBoxSize.y + 0.5f, EnvironmentLayerMask);
		m_floorNormal = Vector2.up;
		RaycastHit2D[] array2 = array;
		foreach (RaycastHit2D raycastHit2D in array2)
		{
			if (true)
			{
				m_floorNormal = raycastHit2D.normal;
				break;
			}
		}
		Vector2 vector = MovementInput;
		if (vector.x > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			vector.x = 1f;
		}
		else if (vector.x < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
		{
			vector.x = -1f;
		}
		else
		{
			vector.x = 0f;
		}
		vector.y = 0f;
		if (m_isGrounded && !m_disableStickiness && vector.magnitude > 0.01f)
		{
			Vector2 to = Vector2.Perpendicular(m_floorNormal);
			if (to.y < 0f)
			{
				to *= -1f;
			}
			if (Vector2.Angle(vector, to) < m_maxRampAngle)
			{
				vector.y = to.y;
			}
		}
		if (vector.magnitude > 0f)
		{
			Collider2D collider = GetCollider();
			float num = (m_isGrounded ? 0.5f : 0.95f);
			RaycastHit2D[] array3 = Physics2D.BoxCastAll(collider.bounds.center, collider.bounds.size * num, 0f, vector.normalized, 0.1f, EnvironmentLayerMask);
			bool flag = false;
			array2 = array3;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit2D raycastHit2D2 = array2[i];
				if (!raycastHit2D2.collider.GetComponent<Stairs>() && !raycastHit2D2.collider.GetComponent<DropPlatform>() && !raycastHit2D2.collider.GetComponent<PlatformEffector2D>() && raycastHit2D2.collider.gameObject.layer != LayerMask.NameToLayer("Pushable"))
				{
					flag = Mathf.Abs(Vector2.Dot(raycastHit2D2.normal, Vector2.up)) < 0.5f;
				}
			}
			if (flag)
			{
				vector = Vector2.zero;
			}
			if (m_movementLimitCollider != null)
			{
				Vector2 point = collider.bounds.center;
				point += vector.normalized * collider.bounds.size;
				if (!m_movementLimitCollider.OverlapPoint(point))
				{
					vector = Vector2.zero;
				}
			}
		}
		if (m_kinematicMode == CharacterKinematicMode.AlwaysKinematic)
		{
			m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
		}
		else if (m_kinematicMode == CharacterKinematicMode.Normal && vector.magnitude <= 0.01f && m_rigidbody.linearVelocity.magnitude <= 0.1f && IsGrounded && (!m_isStandingOnDropPlatform || !m_isDroppingThroughDropPlatforms) && m_activeMovingPlatform == null && !m_isStandingOnMovingPlatform)
		{
			m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
			m_rigidbody.linearVelocity = Vector2.zero;
		}
		else
		{
			m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
		}
		if (m_rigidbody.bodyType != RigidbodyType2D.Kinematic)
		{
			bool flag2 = false;
			if (Physics2D.Raycast(m_groundCheckTransform.transform.position, Vector2.down, 0.5f, EnvironmentLayerMask).collider != null)
			{
				flag2 = true;
			}
			Vector2 linearVelocity = m_rigidbody.linearVelocity;
			if (m_isGrounded)
			{
				linearVelocity = vector * ActiveMovementSettings.MoveSpeed;
				linearVelocity.y = m_rigidbody.linearVelocity.y;
			}
			else if (!m_disableStickiness && flag2 && m_isInMovementState)
			{
				linearVelocity.y = Mathf.Min(linearVelocity.y, 0f - s_snapToGroundVelocity);
			}
			if (m_isInMovementState)
			{
				m_rigidbody.linearVelocity = linearVelocity;
			}
		}
	}

	public void Jump(Vector2 jumpVelocity)
	{
		m_onJumpEvent.Invoke();
		m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
		m_kinematicMode = CharacterKinematicMode.NeverKinematic;
		Vector2 linearVelocity = jumpVelocity;
		if (m_characterDirection.CurrentDirection == CharacterDirection.Facing.Left)
		{
			linearVelocity.x *= -1f;
		}
		m_rigidbody.linearVelocity = linearVelocity;
		m_disableStickiness = true;
		if (m_stickinessCoroutine != null)
		{
			StopCoroutine(m_stickinessCoroutine);
		}
		m_stickinessCoroutine = StartCoroutine(StickinessReeanble());
	}

	private IEnumerator StickinessReeanble()
	{
		yield return new WaitForSeconds(0.5f);
		bool shouldReenable = false;
		while (!shouldReenable)
		{
			yield return new WaitUntil(() => m_isGrounded);
			yield return new WaitForSeconds(0.1f);
			if (m_isGrounded)
			{
				shouldReenable = true;
			}
		}
		m_disableStickiness = false;
		m_stickinessCoroutine = null;
	}

	public void AttachToLadder(Ladder ladder)
	{
		m_attachedLadder = ladder;
		if (ladder != null)
		{
			m_previousLadder = ladder;
		}
	}

	public void AttachToRope(ClimbableRope rope)
	{
		m_attachedRope = rope;
		if (rope != null)
		{
			m_previousRope = rope;
		}
	}

	public Collider2D GetCollider()
	{
		if (m_characterStance != null)
		{
			return m_characterStance.GetActiveCollider();
		}
		return m_movementCollider;
	}

	private void SetIgnoreCollider(Collider2D collider, bool ignore)
	{
		if (m_characterStance != null)
		{
			m_characterStance.SetIgnoreCollider(collider, ignore);
		}
		else
		{
			Physics2D.IgnoreCollision(m_movementCollider, collider, ignore);
		}
	}

	public void RegisterJunctionPath(JunctionPath path)
	{
		m_activeJunctionPath = path;
	}

	public void DeregisterJunctionPath(JunctionPath path)
	{
		if (m_activeJunctionPath == path)
		{
			Collider2D[] cachedColliders = m_cachedColliders;
			for (int i = 0; i < cachedColliders.Length; i++)
			{
				Physics2D.IgnoreCollision(cachedColliders[i], m_activeJunctionPath.TargetCollider, ignore: false);
			}
			m_activeJunctionPath = null;
		}
	}

	private void UpdateJunctionPath()
	{
		if (!(m_activeJunctionPath != null))
		{
			return;
		}
		bool ignore = true;
		if (MovementInput.y > 0.1f)
		{
			ignore = false;
		}
		bool flag = false;
		foreach (Collider2D touchedCollider in m_touchedColliders)
		{
			if (touchedCollider == m_activeJunctionPath.TargetCollider)
			{
				m_junctionPathTouchTimer = 0.25f;
				flag = true;
			}
		}
		if (!flag && m_junctionPathTouchTimer > 0f && m_isGrounded)
		{
			m_junctionPathTouchTimer -= Time.deltaTime;
		}
		if (m_junctionPathTouchTimer > 0f)
		{
			ignore = false;
		}
		Collider2D[] cachedColliders = m_cachedColliders;
		for (int i = 0; i < cachedColliders.Length; i++)
		{
			Physics2D.IgnoreCollision(cachedColliders[i], m_activeJunctionPath.TargetCollider, ignore);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Collider2D collider2D = ((collision.collider == this) ? collision.otherCollider : collision.collider);
		m_touchedColliders.Add(collider2D);
		Stairs component = collider2D.GetComponent<Stairs>();
		if (component != null)
		{
			m_touchedStairs.Add(component);
		}
		m_onCollision?.Invoke(collision);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		Collider2D collider2D = ((collision.collider == this) ? collision.otherCollider : collision.collider);
		m_touchedColliders.Remove(collider2D);
		Stairs component = collider2D.GetComponent<Stairs>();
		if (component != null)
		{
			m_touchedStairs.Remove(component);
		}
	}

	public void ClearLadderState()
	{
		m_currentLaderRopeState.m_isAtTop = false;
		m_currentLaderRopeState.m_isAtBottom = false;
	}

	public void InstantFullSpeed(Vector2 input)
	{
		Vector2 linearVelocity = input.normalized * float.MaxValue;
		linearVelocity.x = Mathf.Clamp(linearVelocity.x, 0f - ActiveMovementSettings.MoveSpeed, ActiveMovementSettings.MoveSpeed);
		linearVelocity.y = m_rigidbody.linearVelocity.y;
		m_rigidbody.linearVelocity = linearVelocity;
	}

	public void ForceFall(float forceFallTime = 0.1f)
	{
		m_isGrounded = false;
		m_forceFallingTimer = forceFallTime;
	}

	public void LimitMovementToCollider(Collider2D collider)
	{
		m_movementLimitCollider = collider;
	}

	public void AttachToMovingPlatform(MovingPlatform movingPlatform)
	{
		m_activeMovingPlatform = movingPlatform;
	}

	public void DetachFromMovingPlatform(MovingPlatform movingPlatform)
	{
		if (m_activeMovingPlatform == movingPlatform)
		{
			m_activeMovingPlatform = null;
		}
	}

	public void ApplyMovementFromAnimRootNode()
	{
		Vector3 position = base.transform.position;
		position += m_animRootNode.transform.localPosition;
		PositionAndUndoAnimRootNode(position);
	}

	public void PositionAndUndoAnimRootNode(Vector3 position)
	{
		base.transform.position = position;
		m_rigidbody.position = position;
		m_animRootNode.DoResetFrame();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (m_groundCheckTransform != null)
		{
			Gizmos.DrawWireCube(m_groundCheckTransform.transform.position, m_groundCheckBoxSize);
		}
		if (Application.isPlaying)
		{
			GizmoExtensions.DrawArrow(base.transform.position, m_floorNormal, 1f, 0.1f, 0.1f);
			Gizmos.color = Color.blue;
			Vector2 vector = Vector2.Perpendicular(m_floorNormal);
			if (vector.y < 0f)
			{
				vector *= -1f;
			}
			Vector3 position = base.transform.position;
			position.x += m_floorNormal.x;
			position.y += m_floorNormal.y;
			GizmoExtensions.DrawArrow(position, vector, 1f, 0.1f, 0.1f);
			Collider2D collider = GetCollider();
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere((Vector2)base.transform.position + m_predictedPositionOffset, 0.05f);
		}
		Gizmos.color = Color.white;
	}
}
