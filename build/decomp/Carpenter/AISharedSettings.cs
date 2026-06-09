using UnityEngine;

[CreateAssetMenu(fileName = "AISharedSettings", menuName = "Misc/AI Shared Settings")]
public class AISharedSettings : ScriptableObject
{
	[SerializeField]
	public AnimationCurve m_aiVisionDetectionCurve;
}
