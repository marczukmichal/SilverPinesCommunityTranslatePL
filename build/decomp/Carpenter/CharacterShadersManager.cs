using UnityEngine;

public class CharacterShadersManager : MonoBehaviour
{
	[SerializeField]
	private Renderer m_renderer;

	[SerializeField]
	private Transform m_baseTransform;

	[SerializeField]
	private CharacterWetnessEffect m_wetnessEffect;

	[SerializeField]
	private float m_characterHeight = 2f;

	private int m_charaterMinMaxPosID;

	private int m_outlineLightDirectionID;

	private int m_wetnessID;

	private SpriteRenderer[] m_renderers;

	private Light[] m_sceneLights;

	private void Start()
	{
		m_charaterMinMaxPosID = Shader.PropertyToID("_CharacterMinMaxPos");
		m_outlineLightDirectionID = Shader.PropertyToID("_OutlineLightDirection");
		m_wetnessID = Shader.PropertyToID("_Wetness");
		m_renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		m_sceneLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
	}

	private void Update()
	{
		Vector2 vector = m_baseTransform.position;
		vector.y += m_characterHeight;
		Vector2 vector2 = m_baseTransform.position;
		Vector2 vector3 = default(Vector2);
		vector3.x = vector2.y;
		vector3.y = vector.y;
		Vector2 vector4 = Vector2.up;
		if (FindClosestImportantLight(base.transform.position) != null)
		{
			vector4 = (vector2 - vector).normalized;
		}
		SpriteRenderer[] renderers = m_renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer == null)
			{
				break;
			}
			renderer.material.SetVector(m_charaterMinMaxPosID, vector3);
			renderer.material.SetVector(m_outlineLightDirectionID, vector4);
			if (m_wetnessEffect != null)
			{
				renderer.material.SetFloat(m_wetnessID, m_wetnessEffect.MaterialWetness);
			}
		}
	}

	private Light FindClosestImportantLight(Vector3 targetPosition)
	{
		Light result = null;
		float num = 0f;
		Light[] sceneLights = m_sceneLights;
		foreach (Light light in sceneLights)
		{
			if (!(light == null) && light.isActiveAndEnabled && !(light.GetComponentInParent<CharacterShadersManager>() == this))
			{
				float b = Vector3.Distance(light.transform.position, targetPosition);
				float num2 = light.intensity / Mathf.Max(1f, b);
				if (num2 > num)
				{
					num = num2;
					result = light;
				}
			}
		}
		return result;
	}
}
