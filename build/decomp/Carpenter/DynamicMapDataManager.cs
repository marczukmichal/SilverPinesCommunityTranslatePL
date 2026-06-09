using UnityEngine;

public class DynamicMapDataManager : MonoBehaviour
{
	[SerializeField]
	private MapDynamicData m_dynamicData;

	private void Update()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (!(item != null))
		{
			return;
		}
		LevelMetadata item2 = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
		if (item2 != null)
		{
			BaseMapAreaMetadata activePlayerMapAreaMetadata = item2.GetActivePlayerMapAreaMetadata();
			if (activePlayerMapAreaMetadata != null)
			{
				activePlayerMapAreaMetadata.UpdateDynamicData(m_dynamicData, item);
			}
		}
	}
}
