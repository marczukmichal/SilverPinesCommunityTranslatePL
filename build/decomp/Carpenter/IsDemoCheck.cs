using UnityEngine;
using UnityEngine.Events;

public class IsDemoCheck : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_isDemo;

	[SerializeField]
	private UnityEvent m_isNotDemo;

	private void Start()
	{
		m_isDemo.Invoke();
	}
}
