using UnityEngine;

[CreateAssetMenu(fileName = "DLC", menuName = "Misc/DLC")]
public class DLCProduct : ScriptableObject
{
	[SerializeField]
	public string SteamId;

	[SerializeField]
	public string EGSId;

	[SerializeField]
	public string PS5Id;

	[SerializeField]
	public string SwitchId;

	[SerializeField]
	public string Switch2Id;

	[SerializeField]
	public string GameCoreId;
}
