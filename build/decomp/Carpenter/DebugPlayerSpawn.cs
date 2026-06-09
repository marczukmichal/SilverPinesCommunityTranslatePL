using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Debug/Player Spawn")]
public class DebugPlayerSpawn : ScriptableObject
{
	public bool m_enabled;

	public Vector3 m_position;

	public bool m_doQuickLoad;
}
