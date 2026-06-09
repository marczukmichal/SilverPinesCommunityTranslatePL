using UnityEngine.AddressableAssets;

public class ActiveUseStateInfoData
{
	public bool m_isActive;

	public float m_progress;

	public float m_activeActionTime;

	public bool m_canActiveInteract;

	public float m_activeActionTimeMin;

	public float m_activeActiveTimeMax;

	public bool m_isLastReloadAction;

	public int m_currentAmmoAmount;

	public int m_maxAmmoCapacity;

	public AssetReference m_actionViewAssetReference;

	public ActiveUseState m_activeUseState;
}
