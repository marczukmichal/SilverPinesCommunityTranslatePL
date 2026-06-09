using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class InMenuBlurOverlay : MonoBehaviour
{
	[SerializeField]
	private Volume m_postVolume;

	private float m_fadeSpeed = 6f;

	private GameMenuState.GameMenu m_activeMenu;

	private void OnEnable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnDisable()
	{
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu menu)
	{
		m_activeMenu = menu;
	}

	private void Update()
	{
		bool flag = m_activeMenu.HasFlag(GameMenuState.GameMenu.InGameMenu) || m_activeMenu.HasFlag(GameMenuState.GameMenu.PauseHint) || m_activeMenu.HasFlag(GameMenuState.GameMenu.GameSaving) || m_activeMenu.HasFlag(GameMenuState.GameMenu.SystemMenu) || m_activeMenu.HasFlag(GameMenuState.GameMenu.GameNotesReader) || m_activeMenu.HasFlag(GameMenuState.GameMenu.ItemPickup) || m_activeMenu.HasFlag(GameMenuState.GameMenu.ExaminableWithImage) || m_activeMenu.HasFlag(GameMenuState.GameMenu.ApplyInteract);
		if (!flag && m_activeMenu.HasFlag(GameMenuState.GameMenu.Minigame))
		{
			MinigameMetadata item = GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameMetadataAnchor.Item;
			if (item != null)
			{
				flag = item.BlurBackground;
			}
		}
		float target = (flag ? 1f : 0f);
		m_postVolume.weight = Mathf.MoveTowards(m_postVolume.weight, target, Time.unscaledDeltaTime * m_fadeSpeed);
	}
}
