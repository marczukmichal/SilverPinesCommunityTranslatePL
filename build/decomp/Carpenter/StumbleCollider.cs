using UnityEngine;

public class StumbleCollider : MonoBehaviour
{
	[SerializeField]
	private Collider2D[] m_colliders;

	[SerializeField]
	private bool m_canTriggerFall = true;

	[SerializeField]
	private bool m_stumbleActive = true;

	private AIFSMAttackStrategy m_aiFSMAttackStrategy;

	private float m_clearForcedMoveInputTime;

	private bool m_overridingAIMovement;

	public bool CanTriggerFall => m_canTriggerFall;

	public bool StumbleActive
	{
		get
		{
			return m_stumbleActive;
		}
		set
		{
			m_stumbleActive = value;
		}
	}

	private void Start()
	{
		m_aiFSMAttackStrategy = GetComponentInParent<AIFSMAttackStrategy>();
	}

	private void Reset()
	{
		m_colliders = GetComponentsInChildren<Collider2D>();
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		CharacterStumble componentInParent = collision.gameObject.GetComponentInParent<CharacterStumble>();
		if (componentInParent != null)
		{
			componentInParent.ApplyStumbleOverlap(this);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		CharacterStumble componentInParent = collision.gameObject.GetComponentInParent<CharacterStumble>();
		if (componentInParent != null)
		{
			componentInParent.RemoveStumbleOverlap(this);
		}
	}

	private void Update()
	{
		Collider2D[] colliders = m_colliders;
		for (int i = 0; i < colliders.Length; i++)
		{
			colliders[i].enabled = m_stumbleActive;
		}
		if (m_overridingAIMovement && Time.time > m_clearForcedMoveInputTime)
		{
			m_overridingAIMovement = false;
			m_aiFSMAttackStrategy.ResetForceMoveInput();
		}
	}

	public void ApplySuccesfulStumbleSeperationBehaviour(Vector2 moveDirection, float duration)
	{
		if (m_aiFSMAttackStrategy != null)
		{
			m_aiFSMAttackStrategy.SetForceMoveInput(moveDirection);
			m_clearForcedMoveInputTime = Time.time + duration;
			m_overridingAIMovement = true;
		}
	}
}
