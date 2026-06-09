using UnityEngine;

public class RotaryPhoneMinigameButton : MonoBehaviour
{
	[SerializeField]
	private RotaryPhoneMinigame m_minigame;

	[SerializeField]
	private string m_codeInput;

	[SerializeField]
	private float m_rotationAmount;

	public string CodeInput => m_codeInput;

	public float RotationAmount => m_rotationAmount;

	public void Pressed()
	{
		m_minigame.DoInput(this);
	}
}
