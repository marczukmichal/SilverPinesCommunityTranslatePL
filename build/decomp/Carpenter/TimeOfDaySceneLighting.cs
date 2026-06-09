using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[ExecuteAlways]
public class TimeOfDaySceneLighting : MonoBehaviour
{
	[SerializeField]
	private TimeOfDay.TimePeriod m_timePeriod;

	[SerializeField]
	private Light m_sunLightSource;

	[SerializeField]
	private Material m_skyboxMaterial;

	[ColorUsage(false, true)]
	[FormerlySerializedAs("m_newAmbientLightColor")]
	[SerializeField]
	private Color m_ambientLightColor;

	[Header("Fog Gradient")]
	[SerializeField]
	private Gradient m_fogGradientDistance = new Gradient
	{
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(1f, 1f)
		},
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 1f)
		}
	};

	[SerializeField]
	private float m_fogMinDistance;

	[SerializeField]
	private float m_fogMaxDistance = 1000f;

	[SerializeField]
	private Gradient m_fogGradientHeight = new Gradient
	{
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(0f, 1f)
		},
		colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 1f)
		}
	};

	[SerializeField]
	private float m_fogMinHeight;

	[SerializeField]
	private float m_fogMaxHeight = 1000f;

	[Header("Anchors")]
	[SerializeField]
	private GameObjectAnchor m_playerAnchor;

	[Header("Post Processing")]
	[SerializeField]
	private Volume m_postProcessingVolume;

	[Header("Object Visibility")]
	[FormerlySerializedAs("m_fieldOfViewVisibilityCurve")]
	[SerializeField]
	private ObjectVisibilityLevelSettings m_objectVisibilitySettings;

	private static TimeOfDaySceneLighting m_activeSceneLighting;

	public TimeOfDay.TimePeriod TimePeriod => m_timePeriod;

	public Gradient FogGradientDistance => m_fogGradientDistance;

	public float FogMinDistance => m_fogMinDistance;

	public float FogMaxDistance => m_fogMaxDistance;

	public Gradient FogGradientHeight => m_fogGradientHeight;

	public float FogMinHeight => m_fogMinHeight;

	public float FogMaxHeight => m_fogMaxHeight;

	public Volume PostProcessingVolume => m_postProcessingVolume;

	public ObjectVisibilityLevelSettings ObjectVisibilitySettings => m_objectVisibilitySettings;

	public static TimeOfDaySceneLighting ActiveSceneLighting => m_activeSceneLighting;

	private void OnEnable()
	{
		m_activeSceneLighting = this;
		if (Application.isPlaying)
		{
			GlobalReferences.Instance.EventChannels.Generic.TimeOfDaySceneLightingChanged.Raise();
		}
	}

	private void OnDisable()
	{
		if (m_activeSceneLighting == this)
		{
			m_activeSceneLighting = null;
		}
	}

	private void Update()
	{
		RenderSettings.ambientLight = m_ambientLightColor;
		if (m_skyboxMaterial != null)
		{
			RenderSettings.skybox = m_skyboxMaterial;
		}
		RenderSettings.sun = m_sunLightSource;
		Camera main = Camera.main;
		GameObject gameObject = ((m_playerAnchor != null) ? m_playerAnchor.Item : null);
		float value = 0f;
		if (main != null && gameObject != null)
		{
			value = main.transform.position.z - gameObject.transform.position.z;
		}
		Shader.SetGlobalFloat("_FogCameraOffset", value);
	}
}
