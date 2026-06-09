using UnityEngine;

public class RestrictPlayerActions : MonoBehaviour
{
	public enum Mode
	{
		None,
		WalkOnly
	}

	[SerializeField]
	private Mode m_mode;

	public Mode ActiveMode => m_mode;

	private void OnEnable()
	{
		GlobalReferences.Instance.Sets.Input.RestrictPlayerActionsSet.Add(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Sets.Input.RestrictPlayerActionsSet.Remove(this);
	}

	private void OnDestroy()
	{
		GlobalReferences.Instance.Sets.Input.RestrictPlayerActionsSet.Remove(this);
	}
}
