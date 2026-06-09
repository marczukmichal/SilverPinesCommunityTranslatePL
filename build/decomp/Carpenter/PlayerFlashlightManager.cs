using UnityEngine;

public class PlayerFlashlightManager : MonoBehaviour
{
	[SerializeField]
	private BoolVariable m_flashlightEnabledVariable;

	private PlayerFlashlight[] m_flashlights;

	public bool IsFlashlightActive
	{
		get
		{
			PlayerFlashlight[] flashlights = m_flashlights;
			for (int i = 0; i < flashlights.Length; i++)
			{
				if (flashlights[i].isActiveAndEnabled)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void Start()
	{
		m_flashlights = GetComponentsInChildren<PlayerFlashlight>(includeInactive: true);
	}

	private void Update()
	{
		if (m_flashlightEnabledVariable != null)
		{
			m_flashlightEnabledVariable.Value = IsFlashlightActive;
		}
	}
}
