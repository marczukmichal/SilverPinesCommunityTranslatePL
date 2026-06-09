using UnityEngine;

[CreateAssetMenu(menuName = "Misc/Graphics Quality Preset")]
public class GraphicsQualityPreset : ScriptableObject
{
	[SerializeField]
	public GraphicsQualitySettings m_settings;
}
