using UnityEngine;

public class StairsPortalUser : MonoBehaviour
{
	private StairsPortal m_activeStairsPortal;

	public void SetActiveStairsPortal(StairsPortal stairsPortal)
	{
		m_activeStairsPortal = stairsPortal;
	}

	public void DoInstantMovement()
	{
		base.transform.position = m_activeStairsPortal.PairedStairsPortal.transform.position;
	}

	public void Clear()
	{
		m_activeStairsPortal = null;
	}
}
