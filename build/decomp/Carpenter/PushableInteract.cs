using UnityEngine;
using UnityEngine.Localization;

public class PushableInteract : BaseInteractable
{
	public enum AutoMoveAwayFromWallsMode
	{
		None,
		Right,
		Left
	}

	[Header("Pushable")]
	[SerializeField]
	private BasePushableObject m_pushableObject;

	[SerializeField]
	private float m_pushVelocity = 2f;

	[SerializeField]
	private Bounds m_pullBlockedBounds;

	[SerializeField]
	private Bounds m_pullFallBounds;

	[SerializeField]
	private Bounds m_interactBlockedBounds;

	[SerializeField]
	private Bounds m_interactFallBounds;

	[SerializeField]
	protected LocalizedString m_interactStringReference = new LocalizedString("InteractPrompts", "Move");

	[SerializeField]
	private AutoMoveAwayFromWallsMode m_moveAwayFromWallsMode;

	protected bool m_isUsed;

	private bool m_isPushingAway;

	public BasePushableObject PushableObject => m_pushableObject;

	public float PushVelocity => m_pushVelocity;

	public Bounds PullBlockedBounds => m_pullBlockedBounds;

	public Bounds PullFallBounds => m_pullFallBounds;

	public Bounds InteractBlockedBounds => m_interactBlockedBounds;

	public Bounds InteractFallBounds => m_interactFallBounds;

	public bool IsUsed
	{
		get
		{
			return m_isUsed;
		}
		set
		{
			m_isUsed = value;
		}
	}

	public override string InteractString => m_interactStringReference.GetLocalizedString();

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (m_pushableObject.CanBePushed())
		{
			return !IsInteractBlocked();
		}
		return false;
	}

	public override InteractType GetInteractType()
	{
		return InteractType.Pushable;
	}

	public override void Interact(BaseInteractor interactor)
	{
	}

	public bool IsPullBlocked()
	{
		Collider2D[] array = Physics2D.OverlapBoxAll(base.transform.TransformPoint(m_pullBlockedBounds.center), Vector3.Scale(m_pullBlockedBounds.size, base.transform.lossyScale), 0f, GameLayers.PushableNavigationMask);
		foreach (Collider2D collider in array)
		{
			if (!m_pushableObject.ShouldIgnoreCollider(collider))
			{
				return true;
			}
		}
		if (Physics2D.OverlapBox(base.transform.TransformPoint(m_pullFallBounds.center), Vector3.Scale(m_pullFallBounds.size, base.transform.lossyScale), 0f, GameLayers.PushableNavigationMask) == null)
		{
			return true;
		}
		return false;
	}

	private bool IsInteractBlocked()
	{
		Collider2D[] array = Physics2D.OverlapBoxAll(base.transform.TransformPoint(m_interactBlockedBounds.center), Vector3.Scale(m_interactBlockedBounds.size, base.transform.lossyScale), 0f, GameLayers.PushableNavigationMask);
		foreach (Collider2D collider in array)
		{
			if (!m_pushableObject.ShouldIgnoreCollider(collider))
			{
				return true;
			}
		}
		if (Physics2D.OverlapBox(base.transform.TransformPoint(m_interactFallBounds.center), Vector3.Scale(m_interactFallBounds.size, base.transform.lossyScale), 0f, GameLayers.PushableNavigationMask) == null)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (m_moveAwayFromWallsMode == AutoMoveAwayFromWallsMode.None || m_isUsed)
		{
			return;
		}
		if (IsPullBlocked())
		{
			float num = m_pushVelocity * 0.05f;
			if (m_moveAwayFromWallsMode == AutoMoveAwayFromWallsMode.Right)
			{
				num *= -1f;
			}
			m_pushableObject.SetMoveSpeed(num);
			m_isPushingAway = true;
		}
		else if (m_isPushingAway)
		{
			m_isPushingAway = false;
			m_pushableObject.DisableMovement();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.TransformPoint(m_pullBlockedBounds.center), Vector3.Scale(m_pullBlockedBounds.size, base.transform.lossyScale));
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(base.transform.TransformPoint(m_pullFallBounds.center), Vector3.Scale(m_pullFallBounds.size, base.transform.lossyScale));
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(base.transform.TransformPoint(m_interactBlockedBounds.center), Vector3.Scale(m_interactBlockedBounds.size, base.transform.lossyScale));
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(base.transform.TransformPoint(m_interactFallBounds.center), Vector3.Scale(m_interactFallBounds.size, base.transform.lossyScale));
		Gizmos.color = Color.white;
	}
}
