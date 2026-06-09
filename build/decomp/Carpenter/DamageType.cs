using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Damage Types/New Damage Type")]
public class DamageType : ScriptableObject
{
	[Flags]
	public enum DamageTypeFlags
	{
		None = 0,
		AffectedBySlashingArtifact = 1
	}

	[Tooltip("Set if this damage type should be ignored unless something specifically handles it")]
	[SerializeField]
	private bool m_ignoredByDefault;

	[SerializeField]
	private DamageTypeFlags m_damageTypeFlags;

	public bool IgnoredByDefault => m_ignoredByDefault;

	public bool HasFlag(DamageTypeFlags flag)
	{
		return m_damageTypeFlags.HasFlag(flag);
	}
}
