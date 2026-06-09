using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class CarpenterNoiseRenderFeature : ScriptableRendererFeature
{
	private class CarpenterNoisePass : ScriptableRenderPass
	{
		private const string m_passName = "CarpenterNoisePass";

		private Material m_blitMaterial;

		public void Setup(Material material)
		{
			m_blitMaterial = material;
			base.requiresIntermediateTexture = true;
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			CarpenterDangerVolumeComponent component = VolumeManager.instance.stack.GetComponent<CarpenterDangerVolumeComponent>();
			float value = 0f;
			if (component != null && component.IsActive())
			{
				value = component.m_intensity.value;
			}
			m_blitMaterial.SetFloat("_DangerIntensity", value);
			UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
			if (universalResourceData.isActiveTargetBackBuffer)
			{
				Debug.LogError("Trying to use CarpenterNoisePass on backbuffer, requires an intermediate ColorTexture. Skipping pass.");
				return;
			}
			TextureHandle activeColorTexture = universalResourceData.activeColorTexture;
			TextureDesc desc = renderGraph.GetTextureDesc(activeColorTexture);
			desc.name = "CameraColor-CarpenterNoisePass";
			desc.clearBuffer = false;
			TextureHandle textureHandle = renderGraph.CreateTexture(in desc);
			RenderGraphUtils.BlitMaterialParameters blitParameters = new RenderGraphUtils.BlitMaterialParameters(activeColorTexture, textureHandle, m_blitMaterial, 0);
			renderGraph.AddBlitPass(blitParameters, "CarpenterNoisePass", "D:\\work\\4484e127b6f5572a\\Upload\\Assets\\Scripts\\Rendering\\CarpenterNoiseRenderFeature.cs", 48);
			universalResourceData.cameraColor = textureHandle;
		}
	}

	public RenderPassEvent m_injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

	public Material m_material;

	private CarpenterNoisePass m_noisePass;

	private Material m_materialInstance;

	public override void Create()
	{
		m_noisePass = new CarpenterNoisePass();
		m_noisePass.renderPassEvent = m_injectionPoint;
		m_materialInstance = new Material(m_material);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!(m_material == null) && Application.isPlaying && !renderingData.cameraData.isSceneViewCamera && GlobalReferences.Instance.UserPreferences.DisplaySettings.FilmGrainNoise)
		{
			m_noisePass.Setup(m_materialInstance);
			renderer.EnqueuePass(m_noisePass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		Object.DestroyImmediate(m_materialInstance);
	}
}
