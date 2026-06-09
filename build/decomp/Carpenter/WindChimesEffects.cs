using System.Collections;
using UnityEngine;

public class WindChimesEffects : MonoBehaviour
{
	[SerializeField]
	private GameObject[] m_chimes;

	public void SetDropped()
	{
		GameObject[] chimes = m_chimes;
		foreach (GameObject chime in chimes)
		{
			StartCoroutine(Drop(chime));
		}
	}

	private IEnumerator Drop(GameObject chime)
	{
		yield return new WaitForSeconds(Random.Range(0f, 0.5f));
		chime.GetComponent<PassiveRandomRotator>().enabled = false;
		chime.GetComponent<Rigidbody2D>().simulated = true;
		chime.GetComponent<Collider2D>().enabled = true;
	}
}
