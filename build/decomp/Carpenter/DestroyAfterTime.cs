using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
	[SerializeField]
	private float m_time = 0.1f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(m_time);
		Object.Destroy(base.gameObject);
	}
}
