using System;
using Team17;
using Team17.UI;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Game Menu State")]
public class GameMenuState : ScriptableObject
{
	[Flags]
	public enum GameMenu
	{
		None = 0,
		InGameMenu = 1,
		GameSaving = 2,
		Comic = 4,
		Minigame = 8,
		Examinable = 0x10,
		PauseHint = 0x20,
		SystemMenu = 0x40,
		Dialogue = 0x80,
		VideoCutscene = 0x100,
		ExaminableWithImage = 0x200,
		GameOver = 0x400,
		GameNotesReader = 0x800,
		ItemPickup = 0x1000,
		OptionsPage = 0x2000,
		ApplyInteract = 0x4000,
		DialogUI = 0x8000,
		MenuHint = 0x10000,
		FullCredits = 0x20000
	}

	private GameMenu m_activeMenu;

	private static readonly GameMenu s_pcMouseCursorMenus = GameMenu.InGameMenu | GameMenu.GameSaving | GameMenu.Minigame | GameMenu.SystemMenu | GameMenu.GameOver | GameMenu.GameNotesReader | GameMenu.ApplyInteract;

	public UnityAction<GameMenu> OnGameMenuChanged;

	public GameMenu ActiveMenuState => m_activeMenu;

	public void SetInMenu(GameMenu menu)
	{
		GameMenu activeMenu = m_activeMenu;
		m_activeMenu |= menu;
		if (m_activeMenu != activeMenu)
		{
			OnGameMenuChanged?.Invoke(m_activeMenu);
		}
	}

	public void ClearInMenu(GameMenu menu)
	{
		GameMenu activeMenu = m_activeMenu;
		m_activeMenu &= ~menu;
		if (m_activeMenu != activeMenu)
		{
			OnGameMenuChanged?.Invoke(m_activeMenu);
		}
	}

	public bool IsInMenu(GameMenu menu)
	{
		return m_activeMenu.HasFlag(menu);
	}

	public bool IsInMenuExclusively(GameMenu menu)
	{
		return m_activeMenu == menu;
	}

	public bool IsInAnyMenu()
	{
		if (m_activeMenu == GameMenu.None)
		{
			return Services.Get<Team17DialogService>().AnyActiveDialogs;
		}
		return true;
	}

	public bool MenuAllowsCursorOnPCMouseCursor()
	{
		return (m_activeMenu & s_pcMouseCursorMenus) != 0;
	}

	public bool MenuAllowsCursorOnGamepad()
	{
		if (m_activeMenu.HasFlag(GameMenu.GameNotesReader))
		{
			return false;
		}
		if (m_activeMenu.HasFlag(GameMenu.InGameMenu))
		{
			if (GlobalReferences.Instance.Anchors.Map.UIMap3DPanelAnchor.Item != null)
			{
				return true;
			}
			return false;
		}
		if (m_activeMenu.HasFlag(GameMenu.Minigame))
		{
			return true;
		}
		if (m_activeMenu.HasFlag(GameMenu.ApplyInteract))
		{
			return true;
		}
		return false;
	}

	private void OnEnable()
	{
		m_activeMenu = GameMenu.None;
	}
}
