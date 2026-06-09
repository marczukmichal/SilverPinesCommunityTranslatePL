using UnityEngine;

public class GameplayFogBounds : MonoBehaviour
{
	public enum FogType
	{
		Fog,
		Void
	}

	[SerializeField]
	private FogType m_fogType;

	[SerializeField]
	private Vector3 m_boundsScalar = new Vector3(0.9f, 0.7f, 1f);

	public FogType Type => m_fogType;

	public Bounds Bounds
	{
		get
		{
			Vector3 position = base.transform.position;
			Vector3 a = base.transform.lossyScale * 0.5f;
			a = Vector3.Scale(a, m_boundsScalar);
			return new Bounds(position, a * 2f);
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Generic.GameplayFogSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Generic.GameplayFogSet.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Bounds bounds = Bounds;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
		Gizmos.color = Color.white;
	}
}
