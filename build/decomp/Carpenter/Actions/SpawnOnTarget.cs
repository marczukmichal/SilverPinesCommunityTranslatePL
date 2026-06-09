using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Combat")]
public class SpawnOnTarget : FsmStateAction
{
	public GameObject m_spawnPrefab;

	public float m_positionOffset;

	public float m_verticalPlacementOffset;

	public float m_delay;

	private float m_timer;

	private bool m_spawned;

	private AISenses m_senses;

	public override void Awake()
	{
		base.Awake();
		if (base.Owner != null)
		{
			m_senses = base.Owner.GetComponent<AISenses>();
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_timer = 0f;
		m_spawned = false;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!m_spawned)
		{
			m_timer += Time.deltaTime;
			if (m_timer > m_delay && m_senses.CurrentTarget != null)
			{
				Spawn();
			}
		}
	}

	private void Spawn()
	{
		Vector3 aimPosition = m_senses.CurrentTarget.GetAimPosition(Detectable.TargetArea.Chest);
		float num = ((((Vector2)(aimPosition - base.Owner.transform.position)).x < 0f) ? (0f - m_positionOffset) : m_positionOffset);
		aimPosition.x += num;
		Vector2 vector = new Vector2(0.5f, 0.5f);
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(aimPosition, vector, 0f, Vector2.down, 2f, GameLayers.CharacterNavigationMask);
		bool flag = false;
		if ((bool)raycastHit2D.collider)
		{
			Vector2 point = raycastHit2D.point;
			point.y += vector.y * 0.5f;
			if (Physics2D.OverlapCapsule(point, vector * 0.9f, CapsuleDirection2D.Vertical, 0f, GameLayers.CharacterNavigationMask) == null)
			{
				aimPosition.x = raycastHit2D.point.x;
				aimPosition.y = raycastHit2D.point.y + m_verticalPlacementOffset;
				flag = true;
			}
		}
		if (flag)
		{
			DamageCollider[] componentsInChildren = Object.Instantiate(m_spawnPrefab, aimPosition, Quaternion.identity).GetComponentsInChildren<DamageCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Source = base.Owner;
			}
			m_spawned = true;
			Finish();
		}
	}
}
