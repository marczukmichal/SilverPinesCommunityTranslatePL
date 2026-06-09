using System;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class PersistentDataIdentifier : MonoBehaviour
{
	[SerializeField]
	private string m_guid;

	[SerializeField]
	private PersistentDataStore m_data;

	[SerializeField]
	private bool m_manualOverride;

	private bool m_receivedDataFromDatastore;

	public bool ManualOverride => m_manualOverride;

	public string GUID => m_guid;

	public void SetAsNewDynamicObject()
	{
		m_receivedDataFromDatastore = false;
		m_guid = Guid.NewGuid().ToString();
		UpdateFromDatastore();
	}

	public void SetAsExistingSpawnedDynamicObject(string guid)
	{
		m_receivedDataFromDatastore = false;
		m_guid = guid;
		UpdateFromDatastore();
	}

	public void ClearGUID()
	{
		m_guid = "";
	}

	private void Start()
	{
		UpdateFromDatastore();
	}

	public void UpdateFromDatastore()
	{
		if (!Application.isPlaying || m_receivedDataFromDatastore || string.IsNullOrEmpty(m_guid) || !m_data)
		{
			return;
		}
		IPersistentComponent[] components = GetComponents<IPersistentComponent>();
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		bool flag = false;
		GameObject[] array = rootGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].GetComponent<MinigameScene>())
			{
				flag = true;
				break;
			}
		}
		string text = "";
		if (flag)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameInteractableAnchor.Item;
			if (item != null)
			{
				PersistentDataIdentifier component = item.GetComponent<PersistentDataIdentifier>();
				if (component != null)
				{
					text = component.GUID;
				}
			}
		}
		IPersistentComponent[] array2 = components;
		foreach (IPersistentComponent persistentComponent in array2)
		{
			string text2 = m_guid + ":" + persistentComponent.GetType().Name;
			if (flag && !string.IsNullOrEmpty(text))
			{
				text2 = text + ":" + text2;
			}
			PersistentDataObject dataEntry = m_data.Data.GetDataEntry(text2);
			persistentComponent.ReceiveDataStoreEntry(dataEntry);
		}
		array2 = components;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].PostAllReceivedDatastoreEntries();
		}
		m_receivedDataFromDatastore = true;
	}
}
