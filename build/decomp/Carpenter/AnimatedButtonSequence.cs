using System;
using HutongGames.PlayMaker;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[ActionCategory(ActionCategory.Animation)]
public class AnimatedButtonSequence : FsmStateAction
{
	public AnimationClip m_animationClip;

	[UIHint(UIHint.Variable)]
	[SerializeField]
	public FsmObject m_animationVariable;

	[UIHint(UIHint.Variable)]
	public FsmGameObject m_activeTarget;

	public FsmFloat m_progressPerInput;

	public FsmEvent m_onContinueEvent;

	public FsmEvent m_onDoneEvent;

	public FsmEvent m_onCancelEvent;

	public FsmBool m_canCancel = true;

	[ObjectType(typeof(AudioEvent))]
	public FsmObject m_struggleAudioEvent;

	public float m_minTimeBeteweenStruggleAudio;

	[ObjectType(typeof(AudioEvent))]
	public FsmObject m_inputAudioEvent;

	private SpriteAnim m_spriteAnim;

	private AnimationEventsHelper m_animHelper;

	private CharacterInputPlayer m_inputPlayer;

	private CharacterInteractor m_interactor;

	private float m_currentProgress;

	private float m_autoInteractTimer;

	private float m_struggleAudioTime;

	private bool m_isComplete;

	private AudioEvent StruggleAudioEvent
	{
		get
		{
			if (m_struggleAudioEvent != null)
			{
				return m_struggleAudioEvent.Value as AudioEvent;
			}
			return null;
		}
	}

	private AudioEvent InputAudioEvent
	{
		get
		{
			if (m_inputAudioEvent != null)
			{
				return m_inputAudioEvent.Value as AudioEvent;
			}
			return null;
		}
	}

	protected virtual AnimationClip GetAnimationClip()
	{
		if (m_animationVariable != null && !m_animationVariable.IsNone && m_animationVariable.Value is AnimationClip)
		{
			return m_animationVariable.Value as AnimationClip;
		}
		return m_animationClip;
	}

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_spriteAnim = base.Owner.GetComponent<SpriteAnim>();
			m_animHelper = base.Owner.GetComponent<AnimationEventsHelper>();
			m_inputPlayer = base.Owner.GetComponent<CharacterInputPlayer>();
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	private BaseAnimatedButtonSequence GetSequenceListener()
	{
		BaseAnimatedButtonSequence baseAnimatedButtonSequence = null;
		if (m_activeTarget != null && m_activeTarget.Value != null)
		{
			baseAnimatedButtonSequence = m_activeTarget.Value.GetComponent<BaseAnimatedButtonSequence>();
		}
		if (baseAnimatedButtonSequence == null && m_interactor != null && m_interactor.ActiveInteractable != null)
		{
			baseAnimatedButtonSequence = m_interactor.ActiveInteractable.GetComponent<BaseAnimatedButtonSequence>();
		}
		return baseAnimatedButtonSequence;
	}

	public override void OnEnter()
	{
		m_isComplete = false;
		m_currentProgress = 0f;
		m_autoInteractTimer = 0f;
		if (m_animHelper != null)
		{
			m_animHelper.DisableMovementFromNode();
		}
		m_spriteAnim.Play(GetAnimationClip(), 0f);
		if (m_inputPlayer != null)
		{
			CharacterInputPlayer inputPlayer = m_inputPlayer;
			inputPlayer.OnInteractAction = (UnityAction)Delegate.Combine(inputPlayer.OnInteractAction, new UnityAction(OnInteractInput));
			if (m_canCancel.Value)
			{
				CharacterInputPlayer inputPlayer2 = m_inputPlayer;
				inputPlayer2.OnDodgeAction = (UnityAction)Delegate.Combine(inputPlayer2.OnDodgeAction, new UnityAction(Cancel));
			}
		}
		else
		{
			GameInputManager.GameInputActions.Player.Interact.performed += OnInteractInputRaw;
		}
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.ActiveAnimatedButtonSequenceChanged.Raise(this);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_canCancel.Value && m_inputPlayer != null && (m_inputPlayer.IsJumping || m_inputPlayer.IsFiring))
		{
			Cancel();
		}
		else if (GlobalReferences.Instance.UserPreferences.SkipButtonMashEvents)
		{
			m_autoInteractTimer += Time.deltaTime;
			if (m_autoInteractTimer > 0.1f)
			{
				OnInteractPerformed();
				m_autoInteractTimer = 0f;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (!m_isComplete)
		{
			BaseAnimatedButtonSequence sequenceListener = GetSequenceListener();
			if (sequenceListener != null)
			{
				sequenceListener.OnCancel();
			}
		}
		if (m_inputPlayer != null)
		{
			CharacterInputPlayer inputPlayer = m_inputPlayer;
			inputPlayer.OnInteractAction = (UnityAction)Delegate.Remove(inputPlayer.OnInteractAction, new UnityAction(OnInteractInput));
			if (m_canCancel.Value)
			{
				CharacterInputPlayer inputPlayer2 = m_inputPlayer;
				inputPlayer2.OnDodgeAction = (UnityAction)Delegate.Remove(inputPlayer2.OnDodgeAction, new UnityAction(Cancel));
			}
		}
		else
		{
			GameInputManager.GameInputActions.Player.Interact.performed -= OnInteractInputRaw;
		}
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.ActiveAnimatedButtonSequenceChanged.Raise(null);
	}

	private void OnInteractInputRaw(InputAction.CallbackContext context)
	{
		OnInteractInput();
	}

	private void OnInteractInput()
	{
		if (!GlobalReferences.Instance.UserPreferences.SkipButtonMashEvents)
		{
			OnInteractPerformed();
		}
	}

	private void OnInteractPerformed()
	{
		m_currentProgress += m_progressPerInput.Value;
		m_currentProgress = Mathf.Min(1f, m_currentProgress);
		m_spriteAnim.SetNormalizedTime(m_currentProgress);
		AudioEvent struggleAudioEvent = StruggleAudioEvent;
		if (struggleAudioEvent != null && Time.time - m_struggleAudioTime > m_minTimeBeteweenStruggleAudio)
		{
			m_struggleAudioTime = Time.time;
			struggleAudioEvent.Play(base.Owner.transform.position);
		}
		AudioEvent inputAudioEvent = InputAudioEvent;
		if (inputAudioEvent != null)
		{
			inputAudioEvent.Play(base.Owner.transform.position);
		}
		if (!(m_currentProgress >= 1f))
		{
			return;
		}
		m_isComplete = true;
		BaseAnimatedButtonSequence sequenceListener = GetSequenceListener();
		if (sequenceListener != null)
		{
			sequenceListener.OnProgress();
			if (sequenceListener.IsDone)
			{
				sequenceListener.OnComplete();
				base.Fsm.Event(m_onDoneEvent);
			}
			else
			{
				base.Fsm.Event(m_onContinueEvent);
			}
		}
		Finish();
		GlobalReferences.Instance.EventChannels.AnimatedButtonSequence.AnimatedButtonSequenceSuccess.Raise();
	}

	private void Cancel()
	{
		base.Fsm.Event(m_onCancelEvent);
	}
}
