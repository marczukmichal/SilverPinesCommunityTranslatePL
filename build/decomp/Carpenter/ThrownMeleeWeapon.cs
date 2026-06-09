using System.Collections;
using UnityEngine;

public class ThrownMeleeWeapon : MonoBehaviour
{
	private MeleeWeaponItemInstance m_weaponItemInstance;

	public MeleeWeaponItemInstance WeaponItemInstance => m_weaponItemInstance;

	public void SetItemInstance(MeleeWeaponItemInstance itemInstance)
	{
		m_weaponItemInstance = itemInstance;
	}

	public bool ApplyDurabilityLossForHit()
	{
		bool num = m_weaponItemInstance.RemoveDurability(WeaponItemInstance.WeaponDefinition.DurabilityLossOnThrownHit);
		if (num && m_weaponItemInstance.WeaponDefinition.WeaponDestroyedEffectPrefab != null)
		{
			DynamicallySpawnedObject.Spawn(m_weaponItemInstance.WeaponDefinition.WeaponDestroyedEffectPrefab, persistent: false, base.transform.position, Quaternion.identity);
			StartCoroutine(DestroyAfterFrame());
		}
		return num;
	}

	private IEnumerator DestroyAfterFrame()
	{
		yield return new WaitForEndOfFrame();
		Object.Destroy(base.gameObject);
	}
}
