using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputCompanion : BaseCharacterInput, CompanionInputActions.ICompanionActions
{
	[SerializeField]
	private float m_jumpWindow = 0.1f;

	[SerializeField]
	private GameInputManagerAnchor m_gameInputManager;

	private AIBrain m_aiBrain;

	private Vector2 m_movementInput;

	private bool m_requestedJump;

	private bool m_isSprinting;

	private bool m_isFiring;

	private Coroutine m_jumpCoroutine;

	public override Vector2 MovementInput
	{
		get
		{
			if (ShouldUseAIBrain() && (bool)m_aiBrain)
			{
				return m_aiBrain.MovementInput;
			}
			return m_movementInput;
		}
	}

	public override bool IsJumping
	{
		get
		{
			if (ShouldUseAIBrain() && (bool)m_aiBrain)
			{
				return m_aiBrain.IsJumping;
			}
			return m_requestedJump;
		}
	}

	public override bool IsSprinting
	{
		get
		{
			if (ShouldUseAIBrain() && (bool)m_aiBrain)
			{
				return m_aiBrain.IsSprinting;
			}
			return m_isSprinting;
		}
	}

	public override bool IsFiring
	{
		get
		{
			if (ShouldUseAIBrain() && (bool)m_aiBrain)
			{
				return m_aiBrain.IsFiring;
			}
			return m_isFiring;
		}
	}

	public bool ShouldUseAIBrain()
	{
		if (m_aiBrain != null)
		{
			return !m_gameInputManager.Item.IsCompanionPlayerControlled;
		}
		return false;
	}

	public bool IsPlayerControlled()
	{
		return !ShouldUseAIBrain();
	}

	private void Start()
	{
		m_aiBrain = GetComponent<AIBrain>();
	}

	private void OnEnable()
	{
		RegisterCompanionInput(GameInputManager.CompanionInputActions);
		m_movementInput = GameInputManager.CompanionInputActions.Companion.Move.ReadValue<Vector2>();
		m_isSprinting = GameInputManager.CompanionInputActions.Companion.Sprint.ReadValue<float>() > 0.5f;
	}

	private void OnDisable()
	{
		if (GameInputManager.CompanionInputActions != null)
		{
			UnregisterCompanionInput(GameInputManager.CompanionInputActions);
		}
	}

	private void RegisterCompanionInput(CompanionInputActions inputActions)
	{
		inputActions.Companion.SetCallbacks(this);
	}

	private void UnregisterCompanionInput(CompanionInputActions inputActions)
	{
		inputActions.Companion.SetCallbacks(null);
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		m_movementInput = context.action.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			if (m_jumpCoroutine != null)
			{
				StopCoroutine(m_jumpCoroutine);
				m_jumpCoroutine = null;
			}
			m_jumpCoroutine = StartCoroutine(JumpCoroutine());
		}
	}

	private IEnumerator JumpCoroutine()
	{
		m_requestedJump = true;
		yield return new WaitForSeconds(m_jumpWindow);
		m_requestedJump = false;
	}

	public void OnSprint(InputAction.CallbackContext context)
	{
		m_isSprinting = context.performed;
	}

	public void OnMeow(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnReloadAction?.Invoke();
		}
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		m_isFiring = context.performed;
		OnFireAction?.Invoke(context.performed);
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnInteractAction?.Invoke();
		}
	}
}
