using Steamworks;
using UnityEngine;

public class WishlistButton : MonoBehaviour
{
	public void ButtonPressed()
	{
		if (!SteamManager.Initialized)
		{
			Application.OpenURL("https://store.steampowered.com/app/2333000/Silver_Pines/");
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(2333000u), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
	}
}
