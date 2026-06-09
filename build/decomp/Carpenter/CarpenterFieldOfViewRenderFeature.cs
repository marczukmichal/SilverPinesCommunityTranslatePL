using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CarpenterFieldOfViewRenderFeature : ScriptableRendererFeature
{
	private class CarpenterFieldOfViewPass : ScriptableRenderPass
	{
		private class MeshPassData
		{
			internal List<Material> m_visibilityMaterials;
		}

		private class BlitPassData
		{
			internal TextureHandle m_textureToRead;

			internal TextureHandle m_sourceTexture;

			internal Material m_fovMaterial;
		}

		private Material m_fovMaterial;

		private List<Material> m_visibilityMeshMaterials;

		private static readonly int FOV_MATERIAL_COUNT = 12;

		public void Setup(Material fovMaterial, Material visibilityMeshMaterial)
		{
			m_fovMaterial = fovMaterial;
			if (visibilityMeshMaterial != null && m_visibilityMeshMaterials == null)
			{
				m_visibilityMeshMaterials = new List<Material>();
				for (int i = 0; i < FOV_MATERIAL_COUNT; i++)
				{
					m_visibilityMeshMaterials.Add(new Material(visibilityMeshMaterial));
				}
			}
			base.requiresIntermediateTexture = true;
		}

		public void Cleanup()
		{
			if (m_visibilityMeshMaterials == null)
			{
				return;
			}
			foreach (Material visibilityMeshMaterial in m_visibilityMeshMaterials)
			{
				Object.Destroy(visibilityMeshMaterial);
			}
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
		{
			if (m_fovMaterial == null || m_visibilityMeshMaterials == null || GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet.Items.Count == 0)
			{
				return;
			}
			UniversalResourceData universalResourceData = frameContext.Get<UniversalResourceData>();
			TextureHandle activeColorTexture = universalResourceData.activeColorTexture;
			TextureDesc desc = renderGraph.GetTextureDesc(activeColorTexture);
			desc.name = "FOVDestination";
			desc.clearBuffer = false;
			TextureHandle textureHandle = renderGraph.CreateTexture(in desc);
			TextureDesc desc2 = renderGraph.GetTextureDesc(activeColorTexture);
			desc2.name = "FOVMeshTexture";
			desc2.clearBuffer = false;
			desc2.clearColor = Color.white;
			TextureHandle textureHandle2 = renderGraph.CreateTexture(in desc2);
			MeshPassData passData;
			using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<MeshPassData>("FOV Draw Visibility Mesh", out passData, "D:\\work\\4484e127b6f5572a\\Upload\\Assets\\Scripts\\Rendering\\CarpenterFieldOfViewRenderFeature.cs", 103))
			{
				passData.m_visibilityMaterials = m_visibilityMeshMaterials;
				rasterRenderGraphBuilder.SetRenderAttachment(textureHandle2, 0);
				rasterRenderGraphBuilder.AllowPassCulling(value: false);
				rasterRenderGraphBuilder.SetRenderFunc(delegate(MeshPassData data, RasterGraphContext context)
				{
					DrawMeshPass(data, context);
				});
			}
			BlitPassData passData2;
			using (IRasterRenderGraphBuilder rasterRenderGraphBuilder2 = renderGraph.AddRasterRenderPass<BlitPassData>("FOV Full Screen Pass", out passData2, "D:\\work\\4484e127b6f5572a\\Upload\\Assets\\Scripts\\Rendering\\CarpenterFieldOfViewRenderFeature.cs", 112))
			{
				TextureHandle activeColorTexture2 = universalResourceData.activeColorTexture;
				rasterRenderGraphBuilder2.UseTexture(in textureHandle2);
				rasterRenderGraphBuilder2.UseTexture(in activeColorTexture2);
				passData2.m_textureToRead = textureHandle2;
				passData2.m_fovMaterial = m_fovMaterial;
				passData2.m_sourceTexture = activeColorTexture2;
				rasterRenderGraphBuilder2.SetRenderAttachment(textureHandle, 0);
				rasterRenderGraphBuilder2.AllowPassCulling(value: false);
				rasterRenderGraphBuilder2.SetRenderFunc(delegate(BlitPassData data, RasterGraphContext context)
				{
					ExecuteBlitPass(data, context);
				});
			}
			universalResourceData.cameraColor = textureHandle;
		}

		private static void DrawMeshPass(MeshPassData data, RasterGraphContext context)
		{
			Matrix4x4.identity.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
			int num = 0;
			Camera main = Camera.main;
			foreach (FieldOfViewArea item in GlobalReferences.Instance.Sets.FieldOfView.FieldOfViewAreaSet)
			{
				if (item.ShouldRender(main))
				{
					if (num >= FOV_MATERIAL_COUNT)
					{
						Debug.LogWarning("CarpenterFieldOfViewPass: too many active FieldOfViewAreas! Increase FOV_MATERIAL_COUNT");
						break;
					}
					data.m_visibilityMaterials[num].SetFloat("_Alpha", item.ObscuringAlpha);
					context.cmd.DrawMesh(item.Mesh, item.Matrix, data.m_visibilityMaterials[num], 0, 0);
					num++;
				}
			}
		}

		private static void ExecuteBlitPass(BlitPassData data, RasterGraphContext context)
		{
			data.m_fovMaterial.SetTexture("_SourceTexture", data.m_sourceTexture);
			Blitter.BlitTexture(context.cmd, data.m_textureToRead, new Vector4(1f, 1f, 0f, 0f), data.m_fovMaterial, 0);
		}
	}

	public Material m_fovScreenMaterial;

	public Material m_visibilityMeshMaterial;

	private CarpenterFieldOfViewPass m_fovPass;

	public override void Create()
	{
		m_fovPass = new CarpenterFieldOfViewPass();
		m_fovPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (Application.isPlaying && !renderingData.cameraData.isSceneViewCamera)
		{
			m_fovPass.Setup(m_fovScreenMaterial, m_visibilityMeshMaterial);
			renderer.EnqueuePass(m_fovPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (m_fovPass != null)
		{
			m_fovPass.Cleanup();
		}
	}
}
