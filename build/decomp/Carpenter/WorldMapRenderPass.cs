using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WorldMapRenderPass : ScriptableRenderPass
{
	private ProfilingSampler m_profilingSampler = new ProfilingSampler("WorldMap");

	private Material m_visibilityMeshMaterial;

	private Material m_backgroundMeshMateiral;

	private Material m_maskBlendMaterial;

	private Material m_blurMaterial;

	private Mesh m_mesh;

	private RTHandle m_cameraColorTarget;

	private RTHandle m_tempRT;

	private RTHandle m_mapMeshRT;

	private RTHandle m_backgroundRT;

	public WorldMapRenderPass(Material visibilityMeshMaterial, Material backgroundMeshMaterial, Material maskBlendMaterial, Material blurMaterial, Mesh mesh)
	{
		m_visibilityMeshMaterial = visibilityMeshMaterial;
		m_backgroundMeshMateiral = backgroundMeshMaterial;
		m_maskBlendMaterial = maskBlendMaterial;
		m_blurMaterial = blurMaterial;
		m_mesh = mesh;
		base.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
	}

	public void SetTarget(RTHandle colorHandle)
	{
		m_cameraColorTarget = colorHandle;
	}

	[Obsolete("Render Graph")]
	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
		descriptor.depthBufferBits = 0;
		descriptor.useMipMap = true;
		descriptor.autoGenerateMips = true;
		RenderingUtils.ReAllocateIfNeeded(ref m_tempRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_TemporaryColorTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_mapMeshRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_MapVisibleMesh");
		RenderingUtils.ReAllocateIfNeeded(ref m_backgroundRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_MapBackground");
		ConfigureTarget(m_cameraColorTarget);
	}

	[Obsolete("Render Graph")]
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_visibilityMeshMaterial == null || m_maskBlendMaterial == null || m_blurMaterial == null)
		{
			return;
		}
		Map3D item = GlobalReferences.Instance.Anchors.Map.ActiveMap3DView.Item;
		if (!(item == null) && item.UseFogOfWar)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, m_profilingSampler))
			{
				commandBuffer.SetRenderTarget(m_mapMeshRT);
				commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
				item.DrawFogOfWarVisibilityMeshes(commandBuffer, m_visibilityMeshMaterial);
				commandBuffer.SetRenderTarget(m_backgroundRT);
				commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
				item.DrawBackgroundTerrain(commandBuffer, m_backgroundMeshMateiral);
				commandBuffer.SetRenderTarget(m_cameraColorTarget);
				m_maskBlendMaterial.SetTexture("_BackgroundTexture", m_backgroundRT);
				m_maskBlendMaterial.SetTexture("_CurrentTexture", m_cameraColorTarget);
				m_maskBlendMaterial.SetTexture("_MaskTexture", m_mapMeshRT);
				m_maskBlendMaterial.SetFloat("_CameraSize", item.Camera.orthographicSize);
				m_maskBlendMaterial.SetVector("_CameraPosition", item.Camera.transform.position);
				Blitter.BlitCameraTexture(commandBuffer, m_cameraColorTarget, m_tempRT, m_maskBlendMaterial, 0);
				Blitter.BlitCameraTexture(commandBuffer, m_tempRT, m_cameraColorTarget, Vector2.one);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}
	}

	public void Dispose()
	{
		m_tempRT?.Release();
		m_mapMeshRT?.Release();
		m_backgroundRT?.Release();
	}
}
