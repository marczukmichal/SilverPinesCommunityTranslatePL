using UnityEngine;

public class T17PlatformDependant : MonoBehaviour
{
	[Platform(typeof(PlatformUtils.Platforms))]
	public int m_platformsActiveOn;

	public bool ShouldBeActive()
	{
		return PlatformUtils.HasPlatformFlag(m_platformsActiveOn);
	}

	private void Awake()
	{
		if (!ShouldBeActive())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (!ShouldBeActive())
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
