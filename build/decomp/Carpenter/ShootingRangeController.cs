using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ShootingRangeController : MonoBehaviour, IPersistentComponent
{
	private enum HintState
	{
		None,
		ReloadStart,
		ReloadActive,
		Done
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_completed;
	}

	[SerializeField]
	private ShootingRangeTarget[] m_targets;

	[SerializeField]
	private ShootingRangeScoreboard m_scoreboard;

	[Header("Inventory")]
	[SerializeField]
	private ProjectileWeaponItemDefinition m_weaponDefinition;

	[SerializeField]
	private AmmunitionItemDefinition m_ammoDefinition;

	[Header("Input")]
	[SerializeField]
	private CharacterInputPlayer.ForceAimMode m_forceAimMode;

	[Header("Visuals")]
	[SerializeField]
	private GameObject m_inworldRifle;

	[SerializeField]
	private Transform m_weaponChainTransform;

	[Header("Gameplay")]
	[SerializeField]
	private Interactable m_interactable;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_scoreAudioEvent;

	[SerializeField]
	private AudioEvent m_startAudioEvent;

	[SerializeField]
	private AudioEvent m_winAudioEvent;

	[SerializeField]
	private AudioEvent m_loseAudioEvent;

	[SerializeField]
	private AudioTrigger m_musicAudio;

	[SerializeField]
	private AudioTrigger m_machineryAudio;

	[SerializeField]
	private GameObject m_enableObject;

	[Header("Token")]
	[SerializeField]
	private ItemPickup m_tokenPickup;

	[SerializeField]
	private GameObject m_tokenAnimationTemplate;

	[SerializeField]
	private UnityEvent m_onGameEnded;

	[Header("Hints")]
	[SerializeField]
	private HintInfo m_reloadStartHint;

	[SerializeField]
	private HintInfo m_reloadActiveHint;

	[SerializeField]
	private HintInfo m_shootHint;

	[Header("Scores")]
	[SerializeField]
	private UnityEvent m_onWinEvent;

	private int m_score;

	private float m_gameOverTimer;

	private bool m_gameActive;

	private bool m_completed;

	private HintState m_hintState;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public Transform WeaponChainTransform => m_weaponChainTransform;

	public bool GameActive => m_gameActive;

	private void Awake()
	{
		ShootingRangeTarget[] targets = m_targets;
		foreach (ShootingRangeTarget obj in targets)
		{
			obj.OnShot = (UnityAction<int>)Delegate.Combine(obj.OnShot, new UnityAction<int>(IncrementScore));
		}
	}

	private bool ShouldExit()
	{
		_ = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (m_gameOverTimer >= 1f)
		{
			return true;
		}
		return false;
	}

	private int GetPlayerAmmoCountInWeapon()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			if (component != null && component.Inventory.EquippedPrimaryItem is ProjectileWeaponItemInstance projectileWeaponItemInstance)
			{
				return projectileWeaponItemInstance.AmmoCount;
			}
		}
		return 0;
	}

	private bool IsOutOfAmmo()
	{
		int num = 0;
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			if (component != null)
			{
				if (component.Inventory.EquippedPrimaryItem is ProjectileWeaponItemInstance projectileWeaponItemInstance)
				{
					num += projectileWeaponItemInstance.AmmoCount;
				}
				num += component.Inventory.GetAvailableAmmoAmount(m_ammoDefinition);
			}
		}
		return num == 0;
	}

	public void ActivateShootingRange()
	{
		m_gameActive = true;
		m_hintState = HintState.None;
		m_score = 0;
		m_gameOverTimer = 0f;
		m_scoreboard.SetScore(m_score);
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterInventory component = item.GetComponent<CharacterInventory>();
			if (component != null)
			{
				PlayerMainInventory inventory = component.Inventory;
				component.EnableTempInventory();
				PlayerMainInventory tempInventory = component.GetTempInventory();
				ItemInstance itemOfType = inventory.GetItemOfType(m_ammoDefinition);
				if (itemOfType != null)
				{
					inventory.TransferItem(itemOfType, tempInventory, new Vector2Int(-1, -1));
				}
				tempInventory.AddItemToInventory(m_weaponDefinition, 1, new Vector2Int(-1, -1), rotated: false, autoEquip: true, out var _, autoAddShortcut: false, 0);
			}
			CharacterInputPlayer component2 = item.GetComponent<CharacterInputPlayer>();
			if (component2 != null)
			{
				component2.SetForceAimingInput(m_forceAimMode);
				component2.SetTurnDisabled(disabled: true);
			}
		}
		m_inworldRifle.SetActive(value: false);
		m_interactable.gameObject.SetActive(value: false);
		m_enableObject.SetActive(value: true);
		m_startAudioEvent?.Play(m_weaponChainTransform.position);
		m_musicAudio.TriggerAudio();
		m_machineryAudio.TriggerAudio();
		ShootingRangeTarget[] targets = m_targets;
		for (int amountRemaining = 0; amountRemaining < targets.Length; amountRemaining++)
		{
			targets[amountRemaining].RaiseTarget();
		}
	}

	private void IncrementScore(int amount)
	{
		StartCoroutine(DoScoreCoroutine(amount));
		m_score += amount;
	}

	private IEnumerator DoScoreCoroutine(int scoreIncrement)
	{
		int currentScore = m_score;
		m_scoreAudioEvent?.Play(m_weaponChainTransform.position);
		for (int i = 0; i < scoreIncrement; i++)
		{
			currentScore++;
			m_scoreboard.SetScore(currentScore);
			yield return new WaitForSeconds(0.25f);
		}
		if (m_score == m_targets.Length)
		{
			GameOver();
		}
	}

	private void Update()
	{
		if (m_gameActive)
		{
			if (IsOutOfAmmo())
			{
				m_gameOverTimer += Time.deltaTime;
			}
			else
			{
				m_gameOverTimer = 0f;
			}
			if (ShouldExit())
			{
				GameOver();
			}
			UpdateHints();
		}
	}

	private bool PlayerIsReloading()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterReloading component = item.GetComponent<CharacterReloading>();
			if (component != null)
			{
				return component.IsReloading;
			}
		}
		return false;
	}

	private void UpdateHints()
	{
		switch (m_hintState)
		{
		case HintState.None:
			m_hintState = HintState.ReloadStart;
			GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_reloadStartHint);
			break;
		case HintState.ReloadStart:
			if (PlayerIsReloading())
			{
				m_hintState = HintState.ReloadActive;
				GlobalReferences.Instance.EventChannels.Hints.CancelHintInfo.Raise(m_reloadStartHint);
				GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_reloadActiveHint);
			}
			break;
		case HintState.ReloadActive:
			if (GetPlayerAmmoCountInWeapon() >= 1 && !PlayerIsReloading())
			{
				m_hintState = HintState.Done;
				GlobalReferences.Instance.EventChannels.Hints.CancelHintInfo.Raise(m_reloadActiveHint);
				GlobalReferences.Instance.EventChannels.Hints.ShowHintInfo.Raise(m_shootHint);
			}
			break;
		}
	}

	private void GameOver()
	{
		m_gameActive = false;
		ShootingRangeTarget[] targets = m_targets;
		foreach (ShootingRangeTarget shootingRangeTarget in targets)
		{
			if (shootingRangeTarget.IsUp)
			{
				shootingRangeTarget.LowerTarget();
			}
		}
		if (m_score == m_targets.Length && !m_completed)
		{
			GameObject obj = UnityEngine.Object.Instantiate(m_tokenAnimationTemplate, m_tokenAnimationTemplate.transform.parent, worldPositionStays: false);
			obj.transform.SetPositionAndRotation(m_tokenAnimationTemplate.transform.position, m_tokenAnimationTemplate.transform.rotation);
			obj.gameObject.SetActive(value: true);
			m_tokenPickup.SetItemAmount(1);
			m_completed = true;
			if (m_persistentData != null)
			{
				m_persistentData.m_completed = true;
			}
			m_onWinEvent.Invoke();
			if (m_winAudioEvent != null)
			{
				m_winAudioEvent.Play(m_weaponChainTransform.transform.position);
			}
		}
		else if (m_loseAudioEvent != null)
		{
			m_loseAudioEvent.Play(m_weaponChainTransform.transform.position);
		}
		m_musicAudio.StopAudio();
		m_machineryAudio.StopAudio();
		StartCoroutine(Deactivate());
	}

	private IEnumerator Deactivate()
	{
		GameObject player = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		CharacterReloading reloading = player.GetComponent<CharacterReloading>();
		if (reloading.IsReloading)
		{
			yield return new WaitUntil(() => !reloading.IsReloading);
		}
		CharacterInventory component = player.GetComponent<CharacterInventory>();
		if (component != null)
		{
			component.DisableTempInventory();
		}
		CharacterInputPlayer inputPlayer = player.GetComponent<CharacterInputPlayer>();
		if (inputPlayer != null)
		{
			inputPlayer.SetForceAimingInput(CharacterInputPlayer.ForceAimMode.None);
			inputPlayer.SetTurnDisabled(disabled: false);
		}
		m_enableObject.SetActive(value: false);
		inputPlayer.SetInputDisabled(disabled: true);
		m_inworldRifle.SetActive(value: true);
		yield return new WaitForSeconds(0.5f);
		inputPlayer.SetInputDisabled(disabled: false);
		m_interactable.gameObject.SetActive(value: true);
		m_onGameEnded.Invoke();
	}

	public bool RequiresPersistentData()
	{
		return true;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData != null)
		{
			if (m_persistentData.m_completed)
			{
				m_completed = true;
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}
}
