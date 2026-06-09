using Steamworks;
using UnityEngine;

public class OpenURLHelper : MonoBehaviour
{
	[SerializeField]
	private string m_url;

	public void OpenURL()
	{
		Application.OpenURL(m_url);
	}

	public void OpenStorePage()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(2333000u), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
		else
		{
			OpenURL();
		}
	}
}
