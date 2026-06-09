using UnityEngine;

public class LadderCheck : MonoBehaviour
{
	private Ladder m_detectedLadder;

	private bool m_isChecking;

	[SerializeField]
	private Ladder.LadderUser m_userType;

	public Ladder DetectedLadder => m_detectedLadder;

	public bool CanGrabLadder => m_detectedLadder != null;

	public bool IsChecking
	{
		get
		{
			return m_isChecking;
		}
		set
		{
			m_isChecking = value;
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag(Tags.Ladder))
		{
			Ladder component = collision.gameObject.GetComponent<Ladder>();
			if (component != null && component.CanUseLadder(m_userType))
			{
				m_detectedLadder = collision.gameObject.GetComponent<Ladder>();
			}
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag(Tags.Ladder) && collision.gameObject.GetComponent<Ladder>() == m_detectedLadder)
		{
			m_detectedLadder = null;
		}
	}
}
