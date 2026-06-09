using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class RetunToMainMenuDialog : MenuDialogPopup
{
	[SerializeField]
	private TextMeshProUGUI m_timeSinceSavePopup;

	[SerializeField]
	private LocalizedString m_lessThanAMinuteString;

	[SerializeField]
	private LocalizedString m_lessThanTwoMinutesString;

	[SerializeField]
	private LocalizedString m_minutesString;

	[SerializeField]
	private LocalizedString m_pleaseWaitString;

	[SerializeField]
	private GameObject[] m_disableOnExitPrompts;

	[SerializeField]
	private TextMeshProUGUI m_mainText;

	private bool m_isExiting;

	public bool IsExiting => m_isExiting;

	public override void Show()
	{
		base.Show();
		m_isExiting = false;
		float timeSinceLastSave = SaveDataManager.GetTimeSinceLastSave();
		if (timeSinceLastSave < 60f)
		{
			m_timeSinceSavePopup.text = m_lessThanAMinuteString.GetLocalizedString();
			return;
		}
		if (timeSinceLastSave < 120f)
		{
			m_timeSinceSavePopup.text = m_lessThanTwoMinutesString.GetLocalizedString();
			return;
		}
		float num = timeSinceLastSave / 60f;
		m_timeSinceSavePopup.text = m_minutesString.GetLocalizedString(num.ToString("0"));
	}

	public override void OnConfirm()
	{
		if (!m_isExiting)
		{
			m_isExiting = true;
			StartCoroutine(DoExit());
		}
	}

	public override void Confirm()
	{
		OnConfirm();
	}

	private IEnumerator DoExit()
	{
		m_mainText.text = m_pleaseWaitString.GetLocalizedString();
		GameObject[] disableOnExitPrompts = m_disableOnExitPrompts;
		for (int i = 0; i < disableOnExitPrompts.Length; i++)
		{
			disableOnExitPrompts[i].SetActive(value: false);
		}
		yield return new WaitForSecondsRealtime(0.1f);
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}
}
