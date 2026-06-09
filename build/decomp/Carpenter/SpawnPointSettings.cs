using UnityEngine;

public class SpawnPointSettings : MonoBehaviour
{
	[SerializeField]
	private CharacterDirection.Facing m_startFacingDirection;

	public CharacterDirection.Facing StartFacingDirection => m_startFacingDirection;
}
