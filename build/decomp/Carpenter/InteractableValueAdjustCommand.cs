using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class InteractableValueAdjustCommand : BaseInteractableCommand
{
	[SerializeField]
	private ValueAdjustMinigameData m_valueAdjust;

	[SerializeField]
	private bool m_useInputScaling;

	[SerializeField]
	private GameObject m_enableWhileActive;

	[SerializeField]
	private AudioEvent m_startUseAudioEvent;

	[Header("Audio")]
	[SerializeField]
	private EventReference m_loopAudio;

	private bool m_isComplete;

	private bool m_shouldCancel;

	private float m_input;

	private float m_adjustTimer;

	private EventInstance m_fmodEventInstance;

	private float m_loopVolume;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_shouldCancel = false;
		m_isComplete = m_valueAdjust.IsComplete();
		ValueAdjustMinigameData valueAdjust = m_valueAdjust;
		valueAdjust.OnMinigameCompletedEvent = (UnityAction)Delegate.Combine(valueAdjust.OnMinigameCompletedEvent, new UnityAction(OnCompleteEvent));
		if (m_startUseAudioEvent != null)
		{
			m_startUseAudioEvent.Play(interactable.transform.position);
		}
		if (!m_loopAudio.IsNull)
		{
			m_fmodEventInstance = RuntimeManager.CreateInstance(m_loopAudio);
			m_fmodEventInstance.set3DAttributes(interactable.transform.position.To3DAttributes());
			m_loopVolume = 0f;
			m_fmodEventInstance.setVolume(m_loopVolume);
			m_fmodEventInstance.start();
		}
		GameInputManager.GameInputActions.Game.Close.performed += OnPerformedClose;
		if (m_enableWhileActive != null)
		{
			m_enableWhileActive.SetActive(value: true);
		}
		while (!m_isComplete && !m_shouldCancel)
		{
			m_input = GameInputManager.GameInputActions.Player.Move.ReadValue<Vector2>().x;
			float num = 1f;
			if (m_useInputScaling)
			{
				if (m_adjustTimer >= 6f)
				{
					num = 8f;
				}
				else if (m_adjustTimer >= 4f)
				{
					num = 4f;
				}
				else if (m_adjustTimer >= 2f)
				{
					num = 2f;
				}
			}
			if (m_input > GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_valueAdjust.IncreaseValue(num);
				m_adjustTimer += Time.deltaTime;
			}
			else if (m_input < 0f - GameUtils.Constants.s_inputMoveDeadzoneMinValue)
			{
				m_valueAdjust.DecreaseValue(num);
				m_adjustTimer += Time.deltaTime;
			}
			else
			{
				m_adjustTimer = 0f;
			}
			if (m_fmodEventInstance.isValid())
			{
				float target = ((Mathf.Abs(m_input) > 0.01f) ? 1f : 0f);
				m_loopVolume = Mathf.MoveTowards(m_loopVolume, target, Time.deltaTime * 5f);
				m_fmodEventInstance.setVolume(m_loopVolume);
				m_fmodEventInstance.setPitch(num);
			}
			yield return new WaitForEndOfFrame();
		}
		if (m_enableWhileActive != null)
		{
			m_enableWhileActive.SetActive(value: false);
		}
		if (!m_isComplete)
		{
			result.m_cancel = true;
		}
		StopAudio();
	}

	private void OnPerformedClose(InputAction.CallbackContext context)
	{
		m_shouldCancel = true;
	}

	private void OnCompleteEvent()
	{
		m_isComplete = true;
	}

	public override void Cancel()
	{
		base.Cancel();
		if (m_enableWhileActive != null)
		{
			m_enableWhileActive.SetActive(value: false);
		}
		StopAudio();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Close.performed -= OnPerformedClose;
		}
		if (m_enableWhileActive != null)
		{
			m_enableWhileActive.SetActive(value: false);
		}
		StopAudio();
	}

	private void StopAudio()
	{
		if (m_fmodEventInstance.isValid())
		{
			m_fmodEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			m_fmodEventInstance.release();
		}
	}
}
