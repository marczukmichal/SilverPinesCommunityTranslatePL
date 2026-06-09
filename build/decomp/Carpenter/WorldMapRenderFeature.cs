using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WorldMapRenderFeature : ScriptableRendererFeature
{
	[SerializeField]
	private Material m_visionMeshMaterial;

	[SerializeField]
	private Material m_backgroundMeshMaterial;

	[SerializeField]
	private Shader m_maskBlendShader;

	[SerializeField]
	private Shader m_blurShader;

	[SerializeField]
	private Mesh m_mesh;

	private Material m_worldMapMaskMaterialInstance;

	private Material m_blurMaterialInstance;

	private WorldMapRenderPass m_worldMapRenderPass;

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!(m_maskBlendShader == null) && renderingData.cameraData.cameraType == CameraType.Game)
		{
			renderer.EnqueuePass(m_worldMapRenderPass);
		}
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (!(m_maskBlendShader == null) && renderingData.cameraData.cameraType == CameraType.Game)
		{
			m_worldMapRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
			m_worldMapRenderPass.SetTarget(renderer.cameraColorTargetHandle);
		}
	}

	public override void Create()
	{
		if (!(m_maskBlendShader == null))
		{
			m_worldMapMaskMaterialInstance = CoreUtils.CreateEngineMaterial(m_maskBlendShader);
			m_blurMaterialInstance = CoreUtils.CreateEngineMaterial(m_blurShader);
			m_worldMapRenderPass = new WorldMapRenderPass(m_visionMeshMaterial, m_backgroundMeshMaterial, m_worldMapMaskMaterialInstance, m_blurMaterialInstance, m_mesh);
		}
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_worldMapMaskMaterialInstance);
		CoreUtils.Destroy(m_blurMaterialInstance);
	}
}
