using TMPro;
using UnityEngine;

public class ShootingRangeScoreboard : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro m_text;

	private void Start()
	{
		SetScore(0);
	}

	public void SetScore(int score)
	{
		m_text.SetText(score.ToString("00"));
	}
}
