using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InventoryInteractionPanel : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private TextMeshProUGUI m_infoText;

	[SerializeField]
	private Transform m_interactAnimationParent;

	[Header("Fade")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_fadeTime;

	[Header("UI")]
	[SerializeField]
	private GameObject m_buttonPromptsPanel;

	[SerializeField]
	private CursorInteractHighlightController m_cursorHighlightController;

	private GameObject m_currentInteractPrefab;

	private bool m_isShowing;

	private bool m_waitForInventoryExit;

	public void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Register(OnShowInteract);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Combine(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnDisable()
	{
		GameInputManager.RemoveBackInputHandler(this);
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Unregister(OnShowInteract);
		GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(null);
		GameMenuState gameMenuState = GlobalReferences.Instance.GameMenuState;
		gameMenuState.OnGameMenuChanged = (UnityAction<GameMenuState.GameMenu>)Delegate.Remove(gameMenuState.OnGameMenuChanged, new UnityAction<GameMenuState.GameMenu>(OnGameMenuChanged));
	}

	private void OnShowInteract(bool show)
	{
		if (show)
		{
			GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.ApplyInteract);
			GameInputManager.PushBackInputHandler(this);
		}
		else
		{
			GameInputManager.RemoveBackInputHandler(this);
			GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.ApplyInteract);
			GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Set(null);
		}
		bool flag = show;
		IApplyItem item = GlobalReferences.Instance.Anchors.Inventory.ApplyItemInteractableAnchor.Item;
		if (item != null && flag)
		{
			flag = true;
			if (m_currentInteractPrefab != null)
			{
				UnityEngine.Object.Destroy(m_currentInteractPrefab);
			}
			GameObject applyItemInteractUIPrefab = item.GetApplyItemInteractUIPrefab();
			bool flag2 = false;
			if (applyItemInteractUIPrefab != null)
			{
				m_currentInteractPrefab = UnityEngine.Object.Instantiate(applyItemInteractUIPrefab, m_interactAnimationParent, worldPositionStays: false);
				InventoryApplyItemInteract component = m_currentInteractPrefab.GetComponent<InventoryApplyItemInteract>();
				component.Initialise();
				flag2 = component.AlwaysShowInventory;
			}
			else
			{
				flag2 = true;
			}
			if (flag2)
			{
				GlobalReferences.Instance.EventChannels.InGameMenu.ShowInventoryMenuTab.Raise();
				m_buttonPromptsPanel.gameObject.SetActive(value: false);
				m_waitForInventoryExit = true;
				m_cursorHighlightController.enabled = false;
			}
			else
			{
				m_buttonPromptsPanel.gameObject.SetActive(value: true);
				m_waitForInventoryExit = false;
				m_cursorHighlightController.enabled = true;
			}
		}
		else
		{
			m_cursorHighlightController.enabled = false;
		}
		if (flag)
		{
			FadeIn();
		}
		else
		{
			FadeOut();
		}
	}

	private void FadeIn()
	{
		if (!m_isShowing)
		{
			m_isShowing = true;
			DOTween.Kill(m_canvasGroup);
			m_container.SetActive(value: true);
			if (m_canvasGroup != null)
			{
				m_canvasGroup.alpha = 0f;
				m_canvasGroup.DOFade(1f, m_fadeTime).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	private void FadeOut()
	{
		if (!m_isShowing)
		{
			return;
		}
		m_isShowing = false;
		DOTween.Kill(m_canvasGroup);
		if (m_canvasGroup != null)
		{
			m_canvasGroup.DOFade(0f, m_fadeTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_container.SetActive(value: false);
			});
		}
	}

	public bool OnInputBack()
	{
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Raise(value: false);
		return false;
	}

	private void OnGameMenuChanged(GameMenuState.GameMenu gameMenu)
	{
		if (m_waitForInventoryExit && !gameMenu.HasFlag(GameMenuState.GameMenu.InGameMenu))
		{
			StartCoroutine(DelayedExit());
			m_waitForInventoryExit = false;
		}
	}

	private IEnumerator DelayedExit()
	{
		yield return new WaitForEndOfFrame();
		GlobalReferences.Instance.EventChannels.Inventory.ShowApplyItemInteract.Raise(value: false);
	}
}
