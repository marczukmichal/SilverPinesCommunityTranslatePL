using UnityEngine;
using UnityEngine.Events;

public class MeleeDamageTrigger : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_onTriggered;

	public void Trigger()
	{
		m_onTriggered.Invoke();
	}
}
