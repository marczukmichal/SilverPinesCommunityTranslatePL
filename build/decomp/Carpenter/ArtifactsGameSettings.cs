using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Settings/Artifact Game Settings")]
public class ArtifactsGameSettings : ScriptableObject
{
	[SerializeField]
	private ArtifactItemDefinition[] m_artifacts;

	public ArtifactItemDefinition[] Artifacts => m_artifacts;
}
