using System.Collections;
using UnityEngine;

public class DSOEffectDisableAfterTime : MonoBehaviour
{
	[SerializeField]
	private float m_time = 0.1f;

	private IEnumerator DisableAfterTime()
	{
		yield return new WaitForSeconds(m_time);
		base.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		StartCoroutine(DisableAfterTime());
	}
}
