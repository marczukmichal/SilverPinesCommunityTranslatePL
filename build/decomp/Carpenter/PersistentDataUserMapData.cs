using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PersistentDataUserMapData
{
	[SerializeField]
	public List<PhotoData> m_photos;

	[SerializeField]
	public List<MapUserMarkerData> m_markers;
}
