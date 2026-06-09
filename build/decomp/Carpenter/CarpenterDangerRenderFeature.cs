using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class CarpenterDangerRenderFeature : ScriptableRendererFeature
{
	private class CarpenterDangerPass : ScriptableRenderPass
	{
		private static class ShaderIDs
		{
			internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

			internal static readonly int WarpScale = Shader.PropertyToID("_WarpScale");

			internal static readonly int WarpPower = Shader.PropertyToID("_WarpPower");

			internal static readonly int BleedOffset = Shader.PropertyToID("_BleedOffset");

			internal static readonly int TimeAmplitude = Shader.PropertyToID("_TimeAmplitude");

			internal static readonly int TimeFrequency = Shader.PropertyToID("_TimeFrequency");
		}

		private const string m_passName = "CarpenterDangerPass";

		private Material m_blitMaterial;

		public void Setup(Material material)
		{
			m_blitMaterial = material;
			base.requiresIntermediateTexture = true;
		}

		private void UpdateMaterial(CarpenterDangerVolumeComponent effect)
		{
			m_blitMaterial.SetFloat(ShaderIDs.Intensity, effect.m_intensity.value);
			m_blitMaterial.SetFloat(ShaderIDs.WarpScale, effect.m_warpScale.value);
			m_blitMaterial.SetFloat(ShaderIDs.WarpPower, effect.m_warpPower.value);
			m_blitMaterial.SetVector(ShaderIDs.BleedOffset, effect.m_bleedOffset.value);
			m_blitMaterial.SetFloat(ShaderIDs.TimeAmplitude, effect.m_timeAmplitude.value);
			m_blitMaterial.SetFloat(ShaderIDs.TimeFrequency, effect.m_timeFrequency.value);
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			CarpenterDangerVolumeComponent component = VolumeManager.instance.stack.GetComponent<CarpenterDangerVolumeComponent>();
			if (component.IsActive())
			{
				UniversalResourceData universalResourceData = frameData.Get<UniversalResourceData>();
				if (universalResourceData.isActiveTargetBackBuffer)
				{
					Debug.LogError("Trying to use CarpenterDangerPass on backbuffer, requires an intermediate ColorTexture. Skipping pass.");
					return;
				}
				TextureHandle activeColorTexture = universalResourceData.activeColorTexture;
				TextureDesc desc = renderGraph.GetTextureDesc(activeColorTexture);
				desc.name = "CameraColor-CarpenterDangerPass";
				desc.clearBuffer = false;
				TextureHandle textureHandle = renderGraph.CreateTexture(in desc);
				UpdateMaterial(component);
				RenderGraphUtils.BlitMaterialParameters blitParameters = new RenderGraphUtils.BlitMaterialParameters(activeColorTexture, textureHandle, m_blitMaterial, 0);
				renderGraph.AddBlitPass(blitParameters, "CarpenterDangerPass", "D:\\work\\4484e127b6f5572a\\Upload\\Assets\\Scripts\\Rendering\\CarpenterDangerRenderFeature.cs", 66);
				universalResourceData.cameraColor = textureHandle;
			}
		}
	}

	public RenderPassEvent m_injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

	public Material m_material;

	private CarpenterDangerPass m_dangerPass;

	public override void Create()
	{
		m_dangerPass = new CarpenterDangerPass();
		m_dangerPass.renderPassEvent = m_injectionPoint;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!(m_material == null) && !renderingData.cameraData.isSceneViewCamera)
		{
			m_dangerPass.Setup(m_material);
			renderer.EnqueuePass(m_dangerPass);
		}
	}
}
