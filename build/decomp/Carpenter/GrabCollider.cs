using UnityEngine;

public class GrabCollider : MonoBehaviour
{
	[SerializeField]
	private Bounds m_bounds;

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + m_bounds.center, m_bounds.size);
		Gizmos.color = Color.white;
	}

	public CharacterSyncGrab GetGrabbableCharacter(CharacterSyncGrabSettings settings)
	{
		Vector2 point = base.transform.position + m_bounds.center;
		Vector2 size = m_bounds.size;
		Collider2D[] array = Physics2D.OverlapBoxAll(point, size, 0f, GameLayers.DamageablesMask);
		for (int i = 0; i < array.Length; i++)
		{
			CharacterSyncGrab componentInParent = array[i].GetComponentInParent<CharacterSyncGrab>();
			if ((object)componentInParent != null && componentInParent.CanPerformGrab(settings, asInstigator: false))
			{
				return componentInParent;
			}
		}
		return null;
	}
}
