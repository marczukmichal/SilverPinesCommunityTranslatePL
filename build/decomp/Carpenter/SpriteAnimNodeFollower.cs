using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class SpriteAnimNodeFollower : MonoBehaviour
{
	[SerializeField]
	private SpriteAnimNodes m_animNodes;

	[SerializeField]
	private SpriteAnimNodeType m_animNodeType;

	[SerializeField]
	private Vector3 m_offset;

	[SerializeField]
	private Space m_offsetSpace = Space.Self;

	[SerializeField]
	private bool m_fullAttach;

	[SerializeField]
	private bool m_useFixedUpdate;

	[Tooltip("If true then don't instantly snap in place, instead move every frame towards the target. Only supported if not doing full attach")]
	[SerializeField]
	private bool m_useMoveTowards;

	private bool m_ignoreNextUpdate;

	public UnityAction<Vector3> m_onNodeMoved;

	public SpriteAnimNodeType NodeType => m_animNodeType;

	public void DoResetFrame()
	{
		m_ignoreNextUpdate = true;
		base.transform.localPosition = Vector3.zero;
	}

	private void LateUpdate()
	{
		if (!m_useFixedUpdate)
		{
			UpdatePosition();
		}
	}

	private void FixedUpdate()
	{
		if (m_useFixedUpdate)
		{
			UpdatePosition();
		}
	}

	private void OnEnable()
	{
		UpdatePosition(instant: true);
	}

	private void UpdatePosition(bool instant = false)
	{
		if (m_ignoreNextUpdate)
		{
			m_ignoreNextUpdate = false;
			return;
		}
		int animNodeType = (int)m_animNodeType;
		Vector3 localPosition = base.transform.localPosition;
		_ = base.transform.localRotation;
		if (m_fullAttach)
		{
			m_animNodes.SetTransformFromNode(base.transform, animNodeType);
		}
		else if (instant || !m_useMoveTowards)
		{
			base.transform.position = m_animNodes.GetPosition(animNodeType);
		}
		else
		{
			Vector3 position = m_animNodes.GetPosition(animNodeType);
			Vector3 position2 = Vector3.MoveTowards(base.transform.position, position, Time.deltaTime * 25f);
			base.transform.position = position2;
		}
		base.transform.Translate(m_offset, m_offsetSpace);
		if (localPosition != base.transform.localPosition)
		{
			m_onNodeMoved?.Invoke(base.transform.localPosition - localPosition);
		}
	}
}
