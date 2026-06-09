using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SaveDuringGameMenu : MonoBehaviour, IInputBackHandler
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private SaveSelectPanel m_saveSelectPanel;

	[SerializeField]
	private float m_saveFadeInTime = 1f;

	[SerializeField]
	private CanvasGroup m_saveCanvasGroup;

	private int m_selectedSaveIndex = -1;

	private bool m_waitingForSave = true;

	public void SetSelectedSaveIndex(int index)
	{
		m_selectedSaveIndex = index;
	}

	private void Awake()
	{
		m_container.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.ShowSaveGamePanel.Register(Show);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Register(OnSaveCompletedSuccesfully);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.ShowSaveGamePanel.Unregister(Show);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Unregister(OnSaveCompletedSuccesfully);
		GameInputManager.RemoveBackInputHandler(this);
	}

	private void OnSaveCompletedSuccesfully()
	{
		m_waitingForSave = false;
	}

	public void Show()
	{
		m_container.SetActive(value: true);
		m_saveSelectPanel.Show();
		GameInputManager.PushBackInputHandler(this);
		GlobalReferences.Instance.GameMenuState.SetInMenu(GameMenuState.GameMenu.GameSaving);
		StartCoroutine(DoSaveGameFlow());
	}

	private IEnumerator DoSaveGameFlow()
	{
		m_selectedSaveIndex = -1;
		m_saveSelectPanel.gameObject.SetActive(value: true);
		m_saveSelectPanel.Show();
		m_saveCanvasGroup.alpha = 0f;
		m_saveCanvasGroup.DOFade(1f, m_saveFadeInTime).SetUpdate(isIndependentUpdate: true);
		m_waitingForSave = true;
		yield return new WaitUntil(() => m_selectedSaveIndex != -1);
		if (m_selectedSaveIndex != -1)
		{
			GlobalReferences.Instance.EventChannels.SaveLoad.GameSaveInSlot.Raise(m_selectedSaveIndex);
			yield return new WaitUntil(() => !m_waitingForSave);
			yield return new WaitForSecondsRealtime(0.1f);
			m_saveSelectPanel.RefreshButton(m_selectedSaveIndex);
			m_saveSelectPanel.SetSaveDoneMode();
			yield return new WaitForSecondsRealtime(2f);
			Hide();
		}
	}

	public void Hide()
	{
		StopAllCoroutines();
		m_container.SetActive(value: false);
		m_saveSelectPanel.Hide();
		GameInputManager.RemoveBackInputHandler(this);
		GlobalReferences.Instance.GameMenuState.ClearInMenu(GameMenuState.GameMenu.GameSaving);
	}

	public bool OnInputBack()
	{
		if (m_saveSelectPanel.IsConfirmationDialogueActive())
		{
			m_saveSelectPanel.CancelAction();
			return false;
		}
		Hide();
		return false;
	}
}
