using System;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class BackgroundSniper : ImmediateModeShapeDrawer
{
	[Serializable]
	private struct VOResponse
	{
		public float m_chanceToFire;

		public AudioVoicedEvent[] m_voiceLines;

		public bool TryTrigger(CharacterIdentifier identifier, Transform transform)
		{
			if (m_voiceLines.Length == 0)
			{
				return false;
			}
			if (UnityEngine.Random.Range(0f, 1f) < m_chanceToFire)
			{
				GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_voiceLines[UnityEngine.Random.Range(0, m_voiceLines.Length)], transform, isPlayer: false));
				return true;
			}
			return false;
		}
	}

	private enum LaserType
	{
		None,
		Enabled,
		Debug
	}

	private enum State
	{
		WaitingForPlayer,
		NoTargetIdle,
		TargetVisible,
		TargetBlocked,
		PostFireDelay,
		Reloading
	}

	[Header("Aim Settings")]
	[SerializeField]
	private float m_targetHeightOffset = 1f;

	[SerializeField]
	private bool m_invertCoverBehavior;

	[SerializeField]
	private float m_aimTime = 2f;

	[SerializeField]
	private float m_targetInteractingAimMultiplier = 2f;

	[SerializeField]
	private float m_invertedLaserDepth;

	[Header("Weapon Settings")]
	[SerializeField]
	private ProjectileSettings m_firedProjectile;

	[SerializeField]
	private float m_lostTargetDelay = 3f;

	[SerializeField]
	private int m_shotsPerMag = 8;

	[SerializeField]
	private float m_fireAtTargetDistance = 3f;

	[SerializeField]
	private float m_maxRefireRate = 1f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_fireAudioEvent;

	[SerializeField]
	private AudioEvent m_reloadingAudioEvent;

	[Header("Timings")]
	[SerializeField]
	private float m_afterShotDelay = 0.5f;

	[SerializeField]
	private float m_reloadTime = 3f;

	[Header("Lens Flare")]
	[SerializeField]
	private LensFlareComponentSRP m_lensFlare;

	[SerializeField]
	private AnimationCurve m_lensFlareIntensityCurve;

	[Header("Voiceover")]
	[SerializeField]
	private float m_minAudioRefireTime = 5f;

	[SerializeField]
	private VOResponse m_voiceoverReload;

	[SerializeField]
	private VOResponse m_voiceoverValidShot;

	[SerializeField]
	private VOResponse m_voiceoverMiss;

	[SerializeField]
	private VOResponse m_voiceoverTargetAcquired;

	[Header("Laser")]
	[SerializeField]
	private LaserType m_renderLaser;

	[SerializeField]
	private LineRenderer m_laserLineRenderer;

	[Header("Idle Settings")]
	[SerializeField]
	private float m_idleDistanceX = 10f;

	[SerializeField]
	private float m_idleDistanceY = 1f;

	[Header("Recoil")]
	[SerializeField]
	private float m_recoilAmount = 1f;

	[SerializeField]
	private float m_recoilRecoveryRate = 1f;

	private State m_state;

	private Vector2 m_aimPosition;

	private Vector3 m_laserEndPosition;

	private Vector3 m_lastKnownPosition;

	private Vector2 m_idleAimPosition;

	private float m_stateTimer;

	private float m_onTargetTimer;

	private float m_lastShotTime;

	private float m_lastAudioTriggerTime;

	private float m_fireBlockedTime;

	private int m_shotsAvailable;

	private CharacterIdentifier m_identifier;

	private float m_trackingTargetTimer;

	private Vector2 m_aimVelocity;

	private float m_activeRecoil;

	private void Awake()
	{
		m_identifier = GetComponent<CharacterIdentifier>();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		m_state = State.WaitingForPlayer;
		m_aimPosition = base.transform.position;
		m_shotsAvailable = m_shotsPerMag;
		m_trackingTargetTimer = 0f;
		m_aimVelocity = Vector2.zero;
		if (m_laserLineRenderer != null)
		{
			m_laserLineRenderer.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (m_state == State.WaitingForPlayer && item != null)
		{
			Vector2 vector = item.transform.position;
			vector.y += UnityEngine.Random.Range(2f, 3f);
			vector.x += UnityEngine.Random.Range(-2f, 2f);
			m_aimPosition = item.transform.position;
			SetState(State.NoTargetIdle);
		}
		if (item != null)
		{
			AimAtTarget(item);
		}
	}

	private void SetState(State state)
	{
		if (m_state == state)
		{
			return;
		}
		m_state = state;
		m_stateTimer = 0f;
		bool flag = true;
		switch (state)
		{
		case State.Reloading:
			TryTriggerVOResponse(m_voiceoverReload);
			m_reloadingAudioEvent.Play(base.transform.position);
			flag = false;
			break;
		case State.TargetBlocked:
		{
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				m_lastKnownPosition += item.GetComponent<CharacterDirection>().GetForwardVector();
			}
			m_fireBlockedTime = UnityEngine.Random.Range(-1f, 2f);
			break;
		}
		case State.TargetVisible:
			m_trackingTargetTimer = 0f;
			m_aimVelocity = Vector2.zero;
			break;
		case State.NoTargetIdle:
			m_idleAimPosition = m_lastKnownPosition;
			break;
		}
		if (m_laserLineRenderer != null)
		{
			m_laserLineRenderer.enabled = flag;
		}
	}

	private void AimAtTarget(GameObject player)
	{
		Vector3 position = player.transform.position;
		CharacterStance component = player.GetComponent<CharacterStance>();
		if (component != null)
		{
			position.y = component.GetActiveColliderBounds().max.y;
		}
		position.y += m_targetHeightOffset;
		BackgroundSniperCover backgroundSniperCover = null;
		bool flag = true;
		if (Physics.Linecast(base.transform.position, position, out var hitInfo))
		{
			backgroundSniperCover = hitInfo.collider.GetComponent<BackgroundSniperCover>();
			flag = false;
		}
		bool flag2 = false;
		if (m_invertCoverBehavior)
		{
			if (backgroundSniperCover != null)
			{
				flag2 = true;
			}
		}
		else
		{
			flag2 = flag;
		}
		Vector3 vector = m_lastKnownPosition;
		m_stateTimer += Time.deltaTime;
		float num = Vector2.Distance(position, m_aimPosition);
		float num2 = Vector2.Distance(vector, m_aimPosition);
		if (m_lensFlare != null)
		{
			m_lensFlare.scale = (m_lensFlare.intensity = m_lensFlareIntensityCurve.Evaluate(num));
		}
		bool flag3 = true;
		switch (m_state)
		{
		case State.NoTargetIdle:
			if (flag2)
			{
				TryTriggerVOResponse(m_voiceoverTargetAcquired);
				SetState(State.TargetVisible);
				break;
			}
			if (m_stateTimer >= m_lostTargetDelay)
			{
				m_stateTimer = 0f;
				m_idleAimPosition = m_lastKnownPosition;
				m_idleAimPosition.x += UnityEngine.Random.Range(0f - m_idleDistanceX, m_idleDistanceX);
				m_idleAimPosition.y += UnityEngine.Random.Range(0f - m_idleDistanceY, m_idleDistanceY);
			}
			vector = m_idleAimPosition;
			break;
		case State.TargetVisible:
		{
			if (!flag2)
			{
				SetState(State.TargetBlocked);
				break;
			}
			float num3 = 1f;
			CharacterInteractor component2 = player.GetComponent<CharacterInteractor>();
			if (component2 != null && component2.IsInteracting)
			{
				num3 = m_targetInteractingAimMultiplier;
			}
			m_trackingTargetTimer += Time.deltaTime * num3;
			m_lastKnownPosition = position;
			if (num < m_fireAtTargetDistance && m_trackingTargetTimer >= m_aimTime)
			{
				m_onTargetTimer += Time.deltaTime;
				if (m_onTargetTimer > 0.25f && TryFire())
				{
					TryTriggerVOResponse(m_voiceoverValidShot);
				}
			}
			else
			{
				m_onTargetTimer = 0f;
			}
			break;
		}
		case State.TargetBlocked:
			if (flag2)
			{
				TryTriggerVOResponse(m_voiceoverTargetAcquired);
				SetState(State.TargetVisible);
				break;
			}
			if (m_fireBlockedTime > 0f && num2 < 0.1f && m_stateTimer > m_fireBlockedTime && TryFire(spawnProjectile: false))
			{
				TryTriggerVOResponse(m_voiceoverMiss);
				if (backgroundSniperCover != null)
				{
					Vector2 vector2 = base.transform.position;
					Quaternion rotation = Quaternion.FromToRotation(toDirection: (m_aimPosition - vector2).normalized, fromDirection: Vector3.right);
					backgroundSniperCover.PerformImpactEffect(rotation, m_aimPosition);
				}
			}
			if (m_stateTimer >= m_lostTargetDelay)
			{
				SetState(State.NoTargetIdle);
			}
			break;
		case State.Reloading:
			flag3 = false;
			if (m_stateTimer >= m_reloadTime)
			{
				m_shotsAvailable = m_shotsPerMag;
				SetState((!flag2) ? State.NoTargetIdle : State.TargetVisible);
			}
			break;
		case State.PostFireDelay:
			flag3 = false;
			if (m_stateTimer >= m_afterShotDelay)
			{
				SetState((!flag2) ? State.NoTargetIdle : State.TargetVisible);
			}
			break;
		}
		bool flag4 = true;
		if (flag3)
		{
			if (m_state == State.NoTargetIdle)
			{
				m_aimPosition = Vector2.SmoothDamp(m_aimPosition, vector, ref m_aimVelocity, 1f);
			}
			else if (m_trackingTargetTimer >= m_aimTime)
			{
				m_aimPosition = vector;
			}
			else
			{
				float smoothTime = m_aimTime - m_trackingTargetTimer;
				m_aimPosition = Vector2.SmoothDamp(m_aimPosition, vector, ref m_aimVelocity, smoothTime);
			}
			bool flag5 = false;
			Vector3 laserEndPosition = Vector3.zero;
			if (Physics.Linecast(base.transform.position, m_aimPosition, out hitInfo))
			{
				flag5 = true;
				laserEndPosition = hitInfo.point;
			}
			if (m_invertCoverBehavior)
			{
				flag5 = !flag5;
			}
			if (flag5)
			{
				if (m_invertCoverBehavior)
				{
					if (flag5)
					{
						Vector3 normalized = (new Vector3(m_aimPosition.x, m_aimPosition.y, position.z) - base.transform.position).normalized;
						new Plane(Vector3.forward, new Vector3(0f, 0f, m_invertedLaserDepth)).Raycast(new Ray(base.transform.position, normalized), out var enter);
						m_laserEndPosition = base.transform.position + normalized * enter;
					}
				}
				else
				{
					m_laserEndPosition = laserEndPosition;
				}
				m_laserEndPosition.y += m_activeRecoil;
				flag4 = false;
			}
		}
		if (flag4)
		{
			Vector3 vector3 = new Vector3(m_aimPosition.x, m_aimPosition.y, position.z);
			vector3.y += m_activeRecoil;
			Collider2D collider2D = Physics2D.OverlapPoint(m_aimPosition, GameLayers.DamageablesMask);
			if (collider2D != null)
			{
				vector3.z = collider2D.transform.position.z;
				m_laserEndPosition = vector3;
				Vector3.Distance(new Vector3(m_aimPosition.x, m_aimPosition.y, collider2D.transform.position.z), base.transform.position);
			}
			else
			{
				Vector3 normalized2 = (vector3 - base.transform.position).normalized;
				m_laserEndPosition = vector3 + normalized2 * 100f;
			}
		}
		if (m_activeRecoil > 0f)
		{
			m_activeRecoil -= m_recoilRecoveryRate * Time.deltaTime;
			m_activeRecoil = Mathf.Max(0f, m_activeRecoil);
		}
		if (m_laserLineRenderer != null)
		{
			m_laserLineRenderer.positionCount = 2;
			m_laserLineRenderer.SetPosition(0, base.transform.position);
			m_laserLineRenderer.SetPosition(1, m_laserEndPosition);
		}
	}

	private void TryTriggerVOResponse(VOResponse response)
	{
		if (!(Time.time < m_lastAudioTriggerTime + m_minAudioRefireTime) && response.TryTrigger(m_identifier, base.transform))
		{
			m_lastAudioTriggerTime = Time.time;
		}
	}

	private bool TryFire(bool spawnProjectile = true)
	{
		if (Time.time < m_lastShotTime + m_maxRefireRate)
		{
			return false;
		}
		m_lastShotTime = Time.time;
		m_shotsAvailable--;
		m_fireAudioEvent.Play(base.transform.position);
		if (m_shotsAvailable == 0)
		{
			SetState(State.Reloading);
		}
		else
		{
			SetState(State.PostFireDelay);
		}
		if (spawnProjectile)
		{
			Vector2 vector = base.transform.position;
			Vector2 normalized = (m_aimPosition - vector).normalized;
			Quaternion rotation = Quaternion.FromToRotation(Vector3.right, normalized);
			ProjectileUtils.FireProjectile(m_aimPosition + normalized * -1f, rotation, m_firedProjectile, base.gameObject);
		}
		m_activeRecoil = m_recoilAmount;
		return true;
	}

	public override void DrawShapes(Camera cam)
	{
		if (m_renderLaser == LaserType.None)
		{
			return;
		}
		using (Draw.Command(cam))
		{
			Draw.LineGeometry = LineGeometry.Volumetric3D;
			Draw.ThicknessSpace = ThicknessSpace.Pixels;
			Color color = Color.red;
			if (m_renderLaser == LaserType.Debug)
			{
				switch (m_state)
				{
				case State.NoTargetIdle:
					color = Color.green;
					break;
				case State.TargetVisible:
					color = Color.red;
					break;
				case State.TargetBlocked:
					color = Color.yellow;
					break;
				case State.PostFireDelay:
					color = Color.gray;
					break;
				case State.Reloading:
					color = Color.cyan;
					break;
				}
			}
			else if (m_state == State.Reloading)
			{
				return;
			}
			color.a = 0.1f;
			Draw.Line(base.transform.position, m_laserEndPosition, 2f, color);
		}
	}
}
