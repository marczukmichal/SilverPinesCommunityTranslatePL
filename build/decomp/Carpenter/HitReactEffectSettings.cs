using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings/Hit React Effect Settings")]
public class HitReactEffectSettings : ScriptableObject
{
	[Serializable]
	public class HitReactEffect
	{
		[SerializeField]
		public AssetReference m_reactEffectAsset;

		[SerializeField]
		public float m_weight;

		[SerializeField]
		public float m_offsetDistance;
	}

	[Serializable]
	public class HitReactDecal
	{
		[SerializeField]
		public AssetReference m_prefabAssetReference;

		[SerializeField]
		public float m_weight;

		[SerializeField]
		public float m_depthOffset;
	}

	[Serializable]
	public class HitReactGibletPiece
	{
		[SerializeField]
		public AssetReference m_assetReference;

		[SerializeField]
		public float m_weight;
	}

	[Serializable]
	public class HitReactEffectGroup
	{
		[SerializeField]
		public HitReactEffect[] m_effects;

		[FormerlySerializedAs("m_decals")]
		[SerializeField]
		public HitReactDecal[] m_wallDecals;

		[SerializeField]
		public HitReactDecal[] m_floorDecals;

		[SerializeField]
		public int m_gibletPiecesToSpawn = 1;

		[SerializeField]
		public HitReactGibletPiece[] m_gibletPieces;

		[SerializeField]
		public float m_minTimeForRefire;

		[SerializeField]
		public ImpactType m_supportedImpactTypes;
	}

	[SerializeField]
	public HitReactEffectGroup[] m_effectGroups;
}
