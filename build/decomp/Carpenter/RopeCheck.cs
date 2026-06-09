using System.Collections;
using UnityEngine;

public class RopeCheck : MonoBehaviour
{
	private ClimbableRope m_detectedRope;

	private ClimbableRope m_blockedRopeGrab;

	private bool m_isChecking;

	public ClimbableRope DetectedRope => m_detectedRope;

	public bool CanGrabRope
	{
		get
		{
			if (m_detectedRope != null)
			{
				return m_detectedRope != m_blockedRopeGrab;
			}
			return false;
		}
	}

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

	public void BlockRegrab(ClimbableRope rope)
	{
		if (!base.gameObject.activeSelf)
		{
			m_blockedRopeGrab = null;
			return;
		}
		m_blockedRopeGrab = rope;
		StartCoroutine(BlockRegrabCoroutine(rope));
	}

	private IEnumerator BlockRegrabCoroutine(ClimbableRope rope)
	{
		m_blockedRopeGrab = rope;
		yield return new WaitForSeconds(0.5f);
		m_blockedRopeGrab = null;
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag(Tags.Rope))
		{
			m_detectedRope = collision.gameObject.GetComponent<ClimbableRope>();
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag(Tags.Rope) && collision.gameObject.GetComponent<ClimbableRope>() == m_detectedRope)
		{
			m_detectedRope = null;
		}
	}
}
