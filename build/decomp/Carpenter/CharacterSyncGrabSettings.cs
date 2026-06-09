using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Settings/Character Sync Grab")]
public class CharacterSyncGrabSettings : ScriptableObject
{
	[SerializeField]
	public Vector2 m_offset;

	[Header("Damage Settings")]
	[SerializeField]
	public bool m_triggerDamageOnGrab;

	[FormerlySerializedAs("m_newhitSettings")]
	[SerializeField]
	private HitSettings m_damageHitSettings;

	[Header("Timings")]
	[SerializeField]
	public float m_grabTime;

	[SerializeField]
	public float m_panicSuccessTimeReduction;

	[Tooltip("If the isntigator takes damage then this amount is removed from the active timer")]
	[SerializeField]
	public float m_takeDamageTimeReductionInstigator;

	public HitSettings HitSettings => m_damageHitSettings;
}
