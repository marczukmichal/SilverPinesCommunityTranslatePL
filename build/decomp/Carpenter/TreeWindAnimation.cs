using UnityEngine;

public class TreeWindAnimation : MonoBehaviour
{
	[SerializeField]
	private float m_highSpeedAnimThreshold = 0.6f;

	private void Start()
	{
		float normalizedTime = Random.Range(0f, 1f);
		Animator component = GetComponent<Animator>();
		string stateName = "SwayMedium";
		if (GlobalReferences.Instance.Weather.CurrentWindAmount > m_highSpeedAnimThreshold)
		{
			stateName = "SwayHigh";
		}
		component.Play(stateName, 0, normalizedTime);
	}
}
