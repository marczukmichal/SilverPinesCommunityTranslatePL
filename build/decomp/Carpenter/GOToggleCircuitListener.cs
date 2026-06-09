using UnityEngine;

public class GOToggleCircuitListener : BaseCircuitListener
{
	[Header("Game Objects")]
	[SerializeField]
	private GameObject[] m_poweredGameObjects;

	[SerializeField]
	private GameObject[] m_unpoweredGameObjects;

	protected override void OnPowered()
	{
		GameObject[] poweredGameObjects = m_poweredGameObjects;
		foreach (GameObject gameObject in poweredGameObjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
		poweredGameObjects = m_unpoweredGameObjects;
		foreach (GameObject gameObject2 in poweredGameObjects)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: false);
			}
		}
	}

	protected override void OnUnpowered()
	{
		GameObject[] poweredGameObjects = m_poweredGameObjects;
		foreach (GameObject gameObject in poweredGameObjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
		poweredGameObjects = m_unpoweredGameObjects;
		foreach (GameObject gameObject2 in poweredGameObjects)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: true);
			}
		}
	}
}
