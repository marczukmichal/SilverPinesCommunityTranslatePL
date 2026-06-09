using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CollectablesInsertSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_name;

	[SerializeField]
	private RectTransform m_visualsParent;

	private GameObject m_spawnedInstance;

	private AsyncOperationHandle<GameObject> m_handle;

	private CollectableDefinition m_collectable;

	public UnityAction<CollectablesInsertSlot> OnClicked;

	public CollectableDefinition Collectable => m_collectable;

	public void SetCollectable(CollectableDefinition collectable, bool collected)
	{
		if (m_spawnedInstance != null)
		{
			Unload();
		}
		m_collectable = collectable;
		m_name.text = collectable.CollectableName;
		if (collected)
		{
			StartCoroutine(SpawnVisuals(collectable));
		}
	}

	private void OnDisable()
	{
		Unload();
	}

	private IEnumerator SpawnVisuals(CollectableDefinition collectableDefition)
	{
		m_handle = Addressables.LoadAssetAsync<GameObject>(collectableDefition.CollectableVisualsTemplate);
		yield return m_handle;
		m_spawnedInstance = Object.Instantiate(m_handle.Result, m_visualsParent);
		m_spawnedInstance.GetComponent<CollectableVisuals>().SetCollectable(collectableDefition);
	}

	private void Unload()
	{
		if (m_spawnedInstance != null)
		{
			Object.Destroy(m_spawnedInstance);
		}
		if (m_handle.IsValid())
		{
			Addressables.Release(m_handle);
		}
	}

	public void OnButtonClicked()
	{
		OnClicked?.Invoke(this);
	}
}
