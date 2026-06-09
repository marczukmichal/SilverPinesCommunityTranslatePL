using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class HideAction : FsmStateAction
{
	public BoolGameEventChannel m_hidingEventChannel;

	public bool m_canMove;

	private CharacterInteractor m_interactor;

	private InteractHide m_hideInteract;

	private Transform m_transform;

	private Rigidbody2D m_rigidbody2D;

	private Detectable m_detectable;

	private Vector3 m_startPosition;

	private LegacyCharacterMovement m_characterMovement;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
			m_transform = base.Owner.GetComponent<Transform>();
			m_rigidbody2D = base.Owner.GetComponent<Rigidbody2D>();
			m_detectable = base.Owner.GetComponent<Detectable>();
			m_characterMovement = base.Owner.GetComponent<LegacyCharacterMovement>();
		}
	}

	public override void OnEnter()
	{
		m_hideInteract = null;
		BaseInteractable activeInteractable = m_interactor.ActiveInteractable;
		m_hideInteract = activeInteractable as InteractHide;
		m_hideInteract.EnterHide();
		m_startPosition = m_transform.position;
		if (!m_canMove)
		{
			m_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			m_rigidbody2D.linearVelocity = Vector2.zero;
		}
		else
		{
			m_characterMovement.LimitMovementToCollider(activeInteractable.GetComponent<Collider2D>());
		}
		m_detectable.IsHiding = true;
		m_hidingEventChannel.Raise(value: true);
	}

	public override void OnUpdate()
	{
		if (m_hideInteract != null)
		{
			if (m_canMove)
			{
				Vector3 position = m_transform.position;
				position.z = m_hideInteract.transform.position.z;
				m_transform.position = position;
			}
			else
			{
				m_transform.position = m_hideInteract.transform.position;
			}
		}
	}

	public override void OnExit()
	{
		m_hideInteract.ExitHide();
		if (m_canMove)
		{
			Vector3 position = m_transform.position;
			position.z = m_startPosition.z;
			m_transform.position = position;
		}
		else
		{
			Vector3 startPosition = m_startPosition;
			startPosition.x = m_transform.position.x;
			m_transform.position = startPosition;
		}
		if (!m_canMove)
		{
			m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
			m_rigidbody2D.linearVelocity = Vector2.zero;
		}
		else
		{
			m_characterMovement.LimitMovementToCollider(null);
		}
		m_detectable.IsHiding = false;
		m_hidingEventChannel.Raise(value: false);
	}
}
