using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PhotoVisibilityModifier : MonoBehaviour
{
	public enum VisibilityType
	{
		Hide,
		Show,
		Ignore
	}

	[SerializeField]
	private VisibilityType m_visibility;

	private List<Renderer> m_modifiedRenderers = new List<Renderer>();

	private List<LensFlareComponentSRP> m_modifiedLensFlares = new List<LensFlareComponentSRP>();

	private List<Light> m_modifiedLights = new List<Light>();

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Register(PreparePhoto);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Photos.PrepareForPhoto.Unregister(PreparePhoto);
	}

	private bool ShouldEffectComponent(Component component)
	{
		PhotoVisibilityModifier component2 = component.GetComponent<PhotoVisibilityModifier>();
		if (component2 != null && component2 != this)
		{
			return false;
		}
		return true;
	}

	private void PreparePhoto(bool isPrepare)
	{
		if (m_visibility == VisibilityType.Ignore)
		{
			return;
		}
		bool flag = !isPrepare;
		if (m_visibility == VisibilityType.Show)
		{
			flag = !flag;
		}
		if (!flag)
		{
			m_modifiedRenderers.Clear();
			m_modifiedLensFlares.Clear();
			m_modifiedLights.Clear();
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (ShouldEffectComponent(renderer) && renderer.enabled)
				{
					renderer.enabled = false;
					m_modifiedRenderers.Add(renderer);
				}
			}
			LensFlareComponentSRP[] componentsInChildren2 = GetComponentsInChildren<LensFlareComponentSRP>();
			foreach (LensFlareComponentSRP lensFlareComponentSRP in componentsInChildren2)
			{
				if (ShouldEffectComponent(lensFlareComponentSRP) && lensFlareComponentSRP.enabled)
				{
					lensFlareComponentSRP.enabled = false;
					m_modifiedLensFlares.Add(lensFlareComponentSRP);
				}
			}
			Light[] componentsInChildren3 = GetComponentsInChildren<Light>();
			foreach (Light light in componentsInChildren3)
			{
				if (ShouldEffectComponent(light) && light.enabled)
				{
					light.enabled = false;
					m_modifiedLights.Add(light);
				}
			}
			return;
		}
		foreach (Renderer modifiedRenderer in m_modifiedRenderers)
		{
			modifiedRenderer.enabled = true;
		}
		foreach (LensFlareComponentSRP modifiedLensFlare in m_modifiedLensFlares)
		{
			modifiedLensFlare.enabled = true;
		}
		foreach (Light modifiedLight in m_modifiedLights)
		{
			modifiedLight.enabled = true;
		}
	}
}
