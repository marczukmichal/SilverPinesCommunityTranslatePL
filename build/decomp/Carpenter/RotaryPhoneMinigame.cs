using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class RotaryPhoneMinigame : MonoBehaviour
{
	[SerializeField]
	private string m_code;

	[SerializeField]
	private RectTransform m_rotaryDial;

	[SerializeField]
	private AudioEvent m_dialTurnEvent;

	[SerializeField]
	private float m_dialTurnSpeed;

	[SerializeField]
	private float m_dialResetSpeed;

	[SerializeField]
	private UnityEvent m_onComplete;

	private string m_currentInput = "";

	private bool m_isAnimating;

	private void Start()
	{
		m_currentInput = "";
	}

	public void DoInput(RotaryPhoneMinigameButton button)
	{
		if (base.enabled && !m_isAnimating)
		{
			StartCoroutine(DoButtonPress(button));
		}
	}

	private IEnumerator DoButtonPress(RotaryPhoneMinigameButton button)
	{
		m_isAnimating = true;
		if (m_dialTurnEvent != null)
		{
			m_dialTurnEvent.Play2D();
		}
		yield return m_rotaryDial.DOLocalRotate(new Vector3(0f, 0f, button.RotationAmount), m_dialTurnSpeed, RotateMode.WorldAxisAdd).SetEase(Ease.InQuad).SetSpeedBased(isSpeedBased: true)
			.WaitForCompletion();
		m_currentInput += button.CodeInput;
		yield return new WaitForSeconds(0.2f);
		yield return m_rotaryDial.DOLocalRotate(new Vector3(0f, 0f, 0f - button.RotationAmount), m_dialResetSpeed, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetSpeedBased(isSpeedBased: true)
			.WaitForCompletion();
		m_rotaryDial.transform.localRotation = Quaternion.identity;
		if (m_currentInput == m_code)
		{
			CompleteMinigame();
		}
		else
		{
			m_isAnimating = false;
		}
	}

	private void CompleteMinigame()
	{
		m_onComplete.Invoke();
	}

	public void ResetInput()
	{
		m_currentInput = "";
	}
}
