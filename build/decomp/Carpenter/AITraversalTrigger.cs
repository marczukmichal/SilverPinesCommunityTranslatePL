using UnityEngine;

public class AITraversalTrigger : MonoBehaviour
{
	public enum AITraversalType
	{
		Jump
	}

	[SerializeField]
	private AITraversalType m_traversalType;

	public AITraversalType TraversalType => m_traversalType;
}
