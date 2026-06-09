using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

[Serializable]
public class PersistentMapStateData
{
	public List<MapIconStateData> m_mapIconStateData;

	public List<string> m_contributionIDs;

	public List<MapAreaVistedDataPersistent> m_visitedData;

	public List<MapConnectionDataPersistent> m_connectionData;

	public List<AssetReference> m_mapRevealPickups;
}
