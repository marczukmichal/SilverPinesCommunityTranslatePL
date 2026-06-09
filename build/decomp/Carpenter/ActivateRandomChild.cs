using UnityEngine;

public class ActivateRandomChild : MonoBehaviour
{
	private GameObject m_activatedGameObject;

	private void Awake()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		DisableCurrentActiveChild();
		ActivateRandomChildEnable();
	}

	private void OnDisable()
	{
		DisableCurrentActiveChild();
	}

	private void DisableCurrentActiveChild()
	{
		if (m_activatedGameObject != null)
		{
			m_activatedGameObject.SetActive(value: false);
			m_activatedGameObject = null;
		}
	}

	private void ActivateRandomChildEnable()
	{
		int childCount = base.transform.childCount;
		if (childCount > 0)
		{
			Transform child = base.transform.GetChild(Random.Range(0, childCount));
			m_activatedGameObject = child.gameObject;
			m_activatedGameObject.SetActive(value: true);
		}
	}
}
