using UnityEngine;
using UnityEngine.AddressableAssets;

public class FireManager : MonoBehaviour
{
	[SerializeField]
	private AssetReferenceGameObject m_fireAsset;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Fire.SpawnFire.Register(OnSpawnFireEvent);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Fire.SpawnFire.Unregister(OnSpawnFireEvent);
	}

	private void OnSpawnFireEvent(SpawnFireEventData eventData)
	{
		float num = ((eventData.m_amount > 1) ? ((0f - eventData.m_spread) * 0.5f) : 0f);
		float num2 = ((eventData.m_amount > 1) ? (eventData.m_spread / (float)(eventData.m_amount - 1)) : 0f);
		for (int i = 0; i < eventData.m_amount; i++)
		{
			Vector2 origin = eventData.m_position;
			origin.y += 1f;
			origin.x += num;
			origin.x += Random.Range((0f - num2) * 0.5f, num2 * 0.5f);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.down, 4f, GameLayers.EnvironmentMask);
			if (raycastHit2D.collider != null)
			{
				Vector3 position = new Vector3(origin.x, raycastHit2D.point.y, 0f);
				DynamicallySpawnObjectEventData eventData2 = default(DynamicallySpawnObjectEventData);
				eventData2.m_position = position;
				eventData2.m_rotation = Quaternion.identity;
				eventData2.m_scale = Vector3.one;
				eventData2.m_assetReference = m_fireAsset;
				eventData2.m_onSpawnedAction = delegate(GameObject fireObject)
				{
					fireObject.GetComponent<PlayMakerFSM>().SetState("Init");
				};
				DynamicallySpawnedObject.Spawn(eventData2);
			}
			num += num2;
		}
	}
}
