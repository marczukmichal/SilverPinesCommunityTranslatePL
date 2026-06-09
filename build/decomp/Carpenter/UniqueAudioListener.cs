using UnityEngine;

public class UniqueAudioListener : MonoBehaviour
{
	private void OnEnable()
	{
		AudioListener[] array = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (AudioListener audioListener in array)
		{
			if (audioListener.gameObject != base.gameObject)
			{
				Object.Destroy(audioListener.gameObject);
			}
		}
	}
}
