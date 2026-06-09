using Cinemachine;
using UnityEngine;

public class CameraViewPointTrigger : MonoBehaviour
{
	[SerializeField]
	private float m_activateRadius = 5f;

	[SerializeField]
	private float m_activateTime = 0.5f;

	[SerializeField]
	private float m_deactivateTime = 0.1f;

	[SerializeField]
	private bool m_requiresMatchingLookDirection = true;

	[SerializeField]
	private CinemachineVirtualCamera m_virtualCamera;

	private bool m_isActive;

	private float m_timer;

	private void Awake()
	{
		m_virtualCamera.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		m_isActive = false;
	}

	private void Update()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (!(item != null))
		{
			return;
		}
		float num = Vector2.Distance(item.transform.position, base.transform.position);
		if (!m_isActive)
		{
			bool num2 = num < m_activateRadius;
			bool flag = false;
			bool flag2 = ((!m_requiresMatchingLookDirection) ? true : false);
			if (num2)
			{
				if (item.GetComponent<CharacterMovement>().PreviousVelocity.magnitude < 0.1f)
				{
					flag = true;
				}
				if (m_requiresMatchingLookDirection && item.GetComponent<CharacterDirection>().IsFacingPoint(base.transform.position))
				{
					flag2 = true;
				}
			}
			if (num2 && flag && flag2)
			{
				m_timer += Time.deltaTime;
				if (m_timer > m_activateTime)
				{
					Activate();
				}
			}
			else
			{
				m_timer = 0f;
			}
			return;
		}
		bool num3 = num < m_activateRadius;
		bool flag3 = false;
		bool flag4 = ((!m_requiresMatchingLookDirection) ? true : false);
		if (item.GetComponent<CharacterMovement>().PreviousVelocity.magnitude < 0.1f)
		{
			flag3 = true;
		}
		if (m_requiresMatchingLookDirection && item.GetComponent<CharacterDirection>().IsFacingPoint(base.transform.position))
		{
			flag4 = true;
		}
		if ((!num3 && !flag3) || !flag4)
		{
			m_timer += Time.deltaTime;
			if (m_timer > m_deactivateTime)
			{
				Deactivate();
			}
		}
		else
		{
			m_timer = 0f;
		}
	}

	private void Activate()
	{
		if (!m_isActive)
		{
			m_isActive = true;
			m_virtualCamera.gameObject.SetActive(value: true);
			Vector3 position = m_virtualCamera.transform.position;
			position.z = Camera.main.transform.position.z;
			m_virtualCamera.transform.position = position;
			m_timer = 0f;
		}
	}

	private void Deactivate()
	{
		if (m_isActive)
		{
			m_isActive = false;
			m_virtualCamera.gameObject.SetActive(value: false);
			m_timer = 0f;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, m_activateRadius);
		Gizmos.color = Color.white;
	}
}
