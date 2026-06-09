using UnityEngine;

public class CharacterTraversalUtils : MonoBehaviour
{
	public struct LadderData
	{
		public Ladder m_attachedLadder;

		public Ladder m_previousLadder;

		private bool m_enteredAtTop;

		public bool EnteredAtTop
		{
			get
			{
				return m_enteredAtTop;
			}
			set
			{
				m_enteredAtTop = value;
			}
		}
	}

	public LadderData m_ladderData;

	private ClimbableRope m_attachedRope;

	private ClimbableRope m_previousRope;

	public Ladder GetLadder()
	{
		if (m_ladderData.m_attachedLadder != null)
		{
			return m_ladderData.m_attachedLadder;
		}
		return m_ladderData.m_previousLadder;
	}

	public void AttachToLadder(Ladder ladder)
	{
		m_ladderData.m_attachedLadder = ladder;
		if (ladder != null)
		{
			m_ladderData.m_previousLadder = ladder;
		}
	}

	public ClimbableRope GetRope()
	{
		if (m_attachedRope != null)
		{
			return m_attachedRope;
		}
		return m_previousRope;
	}

	public void AttachToRope(ClimbableRope rope)
	{
		m_attachedRope = rope;
		if (rope != null)
		{
			m_previousRope = rope;
		}
	}
}
