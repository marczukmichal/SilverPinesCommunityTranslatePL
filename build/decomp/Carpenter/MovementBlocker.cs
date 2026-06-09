using UnityEngine;

public class MovementBlocker : MonoBehaviour
{
	[SerializeField]
	private CharacterMovement.MovementBlockingType m_blockingType;

	public CharacterMovement.MovementBlockingType BlockingType => m_blockingType;
}
