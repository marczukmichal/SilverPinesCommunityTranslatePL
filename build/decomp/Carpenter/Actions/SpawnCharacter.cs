using System;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;

namespace Actions;

[ActionCategory(ActionCategory.Character)]
public class SpawnCharacter : FsmStateAction
{
	public AssetReferenceSO m_asset;

	public bool m_forceAttackTargetOnPlayer;

	public Transform m_spawnPosition;

	public bool m_persistent = true;

	public override void Awake()
	{
		_ = base.Owner == null;
	}

	public override void OnEnter()
	{
		DynamicallySpawnObjectEventData eventData = new DynamicallySpawnObjectEventData(m_asset.AssetReference, m_persistent, m_spawnPosition.position);
		ref UnityAction<GameObject> onSpawnedAction = ref eventData.m_onSpawnedAction;
		onSpawnedAction = (UnityAction<GameObject>)Delegate.Combine(onSpawnedAction, new UnityAction<GameObject>(OnSpawned));
		DynamicallySpawnedObject.Spawn(eventData);
	}

	private void OnSpawned(GameObject gameObject)
	{
		Finish();
	}
}
