using System;
using UnityEngine;

public class AnimatedInputPrompt : MonoBehaviour
{
	public enum AnimationPromptType
	{
		RotateGamepad,
		Horizontal
	}

	[SerializeField]
	private RectTransform m_buttonIcon;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_magnitude;

	[SerializeField]
	private AnimationCurve m_animationCurve;

	[SerializeField]
	private AnimationPromptType m_promptType;

	private void Update()
	{
		switch (m_promptType)
		{
		case AnimationPromptType.RotateGamepad:
			RotateGamepad();
			break;
		case AnimationPromptType.Horizontal:
			LeftRightMouseDrag();
			break;
		}
	}

	private void RotateGamepad()
	{
		if (m_buttonIcon != null)
		{
			float time = Time.time * m_speed;
			float f = m_animationCurve.Evaluate(time) * MathF.PI * 2f;
			m_buttonIcon.anchoredPosition = new Vector2(Mathf.Sin(f), Mathf.Cos(f)) * m_magnitude;
		}
	}

	private void LeftRightMouseDrag()
	{
		if (m_buttonIcon != null)
		{
			float num = Time.time * m_speed;
			m_animationCurve.Evaluate(num);
			m_buttonIcon.anchoredPosition = new Vector2(Mathf.Sin(num), 0f) * m_magnitude;
		}
	}
}
