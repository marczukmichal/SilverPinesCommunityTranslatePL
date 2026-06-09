using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LevelTransitionEventData
{
	public LevelTransitionEventType m_type;

	public LevelMetadata m_levelMetadata;

	public LevelTransition m_levelTransitionComponent;

	public string m_targetSceneName;

	public string m_targetEntryPoint;

	public AssetReference m_targetSceneAssetReference;

	public Vector3 m_playerSpawnPosition;

	public LevelTransition.ArrivalDirection m_arrivalDirection;

	public LevelTransition.LeaveDirection m_leaveDirection;

	public AudioEvent m_leavingTransitionAudioEvent;

	public EventReference m_transitionAudioEvent;

	public LevelTransitionPlayerSetup m_playerSetup;

	public LevelTransitionFadeType m_fadeType;

	public Vector3 m_transitionFromPosition;

	public bool m_canEnemiesMigrate;

	public bool m_isSprinting;
}
