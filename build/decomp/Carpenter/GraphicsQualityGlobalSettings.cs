using System;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct GraphicsQualityGlobalSettings
{
	public GraphicsQualityPreset HighQualityPreset;

	public GraphicsQualityPreset MediumQualityPreset;

	public GraphicsQualityPreset LowQualityPreset;

	public UniversalRenderPipelineAsset HighQualityShadowsPipelineAsset;

	public UniversalRenderPipelineAsset MediumQualityShadowsPipelineAsset;

	public UniversalRenderPipelineAsset LowQualityShadowsPipelineAsset;
}
