using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ActiveUseSettings", menuName = "Misc/ActiveUseSettings")]
public class ActiveUseSettings : ScriptableObject
{
	public enum ArtifactSpeedBonusType
	{
		None,
		Healing
	}

	[SerializeField]
	private AssetReference m_actionViewAsset;

	[Tooltip("If this weapon can be active used (reload)")]
	[SerializeField]
	private ActiveUseType m_activeUseType;

	[Tooltip("The window for this use item during time in which the active prompt is correct")]
	[SerializeField]
	private Vector2 m_activeUseTimeWindow;

	[Header("Active Use Type - SpeedChange")]
	[Tooltip("Multiplier applied to use speed when failed active reload")]
	[SerializeField]
	private float m_activeUseSpeedOnFail = 0.75f;

	[Tooltip("Multiplier applied to use speed when successful active reload")]
	[SerializeField]
	private float m_activeUseSpeedOnSuccess = 2f;

	[SerializeField]
	private float m_cycleUseTime = 1f;

	[SerializeField]
	private int m_cycleCount = 3;

	[SerializeField]
	private ArtifactSpeedBonusType m_artifactSpeedBonus;

	public AssetReference ActionViewAsset => m_actionViewAsset;

	public ActiveUseType ActiveUseType => m_activeUseType;

	public Vector2 ActiveTimeWindow => m_activeUseTimeWindow;

	public float ActiveUseSpeedOnFail => m_activeUseSpeedOnFail;

	public float ActiveUseSpeedOnSuccess => m_activeUseSpeedOnSuccess;

	public float CycleUseTime => m_cycleUseTime;

	public int CycleCount => m_cycleCount;

	public ArtifactSpeedBonusType ArtifactSpeedBonus => m_artifactSpeedBonus;
}
