using UnityEngine;

public class PursuerSpawnPosition : MonoBehaviour
{
	public static Vector3 GetSpawnPosition(PursuerSpawnTrigger trigger)
	{
		if (trigger.ForceSpawnPosition != null)
		{
			return trigger.ForceSpawnPosition.transform.position;
		}
		PursuerSpawnPosition[] array = Object.FindObjectsByType<PursuerSpawnPosition>(FindObjectsSortMode.None);
		if (array.Length != 0)
		{
			return array[Random.Range(0, array.Length)].transform.position;
		}
		return trigger.transform.position;
	}
}
