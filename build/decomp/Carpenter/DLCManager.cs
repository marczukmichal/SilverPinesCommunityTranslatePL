using Team17;
using UnityEngine;

public class DLCManager : MonoBehaviour
{
	private IPlatformDLC _platformDLC;

	public static DLCManager Instance => GlobalReferences.Instance.Anchors.DLC.DLCManagerAnchor.Item;

	private void Awake()
	{
		if (!Services.TryGet<IPlatformDLC>(out var service))
		{
			Debug.LogWarning("DLCManager.Awake - unable to find registered IPlatformDLC service, using NullPlatformDLC");
			_platformDLC = new NullPlatformDLC();
			Services.Register<IPlatformDLC>(_platformDLC);
			return;
		}
		Debug.Log(string.Format("{0}.{1} - using registered {2} service of type {3}", "DLCManager", "Awake", "IPlatformDLC", service.GetType()));
		_platformDLC = service;
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.DLC.DLCManagerAnchor.Set(this);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.Anchors.DLC.DLCManagerAnchor.Set(null);
	}

	public bool IsProductOwned(DLCProduct product)
	{
		return _platformDLC.HasEntitlementForProduct(product);
	}

	[DebugCommand("has_dlc1", "Checks whether DLC1 is owned by the current user", "", null, false)]
	private void Debug_HasDLC1()
	{
		DLCProduct dLC = GlobalReferences.Instance.DLC.DLC1;
		if (dLC != null)
		{
			bool flag = IsProductOwned(dLC);
			Debug.Log(string.Format("{0}.{1} returned {2}", "DLCManager", "IsProductOwned", flag));
		}
	}
}
