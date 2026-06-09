using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class GameCompletionSummary : MonoBehaviour
{
	[Header("UI Canvas Groups")]
	[SerializeField]
	private CanvasGroup m_mainCanvasGroup;

	[SerializeField]
	private CanvasGroup m_statsCanvasGroup;

	[SerializeField]
	private MenuInputPrompts m_menuInputPrompts;

	[SerializeField]
	private MenuInputPrompts.AvaialbleInput[] m_inputs;

	[Header("Main Values")]
	[SerializeField]
	private TextMeshProUGUI m_difficultyValue;

	[SerializeField]
	private TextMeshProUGUI m_endingNameValue;

	[SerializeField]
	private TextMeshProUGUI m_playtimeValue;

	[SerializeField]
	private TextMeshProUGUI m_savesCountValue;

	[SerializeField]
	private TextMeshProUGUI m_completionPercentValue;

	[Header("Stats")]
	[SerializeField]
	private ArtifactsGameSettings m_artifactGameSettings;

	[SerializeField]
	private RectTransform m_statsParent;

	[SerializeField]
	private GameCompletionStatEntry m_statsEntryPrefab;

	[SerializeField]
	private Color m_badStatColor;

	[SerializeField]
	private GameCompletionPercentSettings m_gameCompletionPercentSettings;

	private static GameEnding m_gameEndingState;

	private bool m_isExiting;

	public static void SetGameEnding(GameEnding ending)
	{
		m_gameEndingState = ending;
	}

	public void Show()
	{
		Initialise();
		PopulateResults();
		StartCoroutine(ShowAnimation());
	}

	private void Initialise()
	{
		base.gameObject.SetActive(value: true);
		m_mainCanvasGroup.alpha = 0f;
		m_statsCanvasGroup.alpha = 0f;
	}

	private GameCompletionStatEntry CreateStatEntry(string localizedStringKey, string valueText)
	{
		GameCompletionStatEntry gameCompletionStatEntry = UnityEngine.Object.Instantiate(m_statsEntryPrefab, m_statsParent);
		gameCompletionStatEntry.Set(new LocalizedString("UIGame", localizedStringKey), valueText);
		return gameCompletionStatEntry;
	}

	private void AddStatsSpacer(float distance)
	{
		RectTransform rectTransform = new GameObject("Spacer").AddComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(100f, distance);
		rectTransform.parent = m_statsParent;
	}

	private string GetEndingName()
	{
		return m_gameEndingState switch
		{
			GameEnding.EndingA => new LocalizedString("UIGame", "CompletionSummary_EndingA_Name").GetLocalizedString(), 
			GameEnding.EndingB => new LocalizedString("UIGame", "CompletionSummary_EndingB_Name").GetLocalizedString(), 
			GameEnding.EndingC => new LocalizedString("UIGame", "CompletionSummary_EndingC_Name").GetLocalizedString(), 
			_ => "?", 
		};
	}

	private void PopulateResults()
	{
		PersistentData data = GlobalReferences.Instance.DataStore.Data;
		PlayerMainInventory mainInventory = GlobalReferences.Instance.MainInventory;
		DifficultySettings difficultyModeConfiguration = GlobalReferences.Instance.GameDifficultySettings.GetDifficultyModeConfiguration(data.CurrentDifficulty);
		m_difficultyValue.text = difficultyModeConfiguration.DifficultyNameString.GetLocalizedString();
		m_endingNameValue.text = GetEndingName();
		m_playtimeValue.text = new TimeSpan(0, 0, 0, (int)data.PlayTime).ToString();
		m_savesCountValue.text = data.SaveCount.ToString();
		m_completionPercentValue.text = GameCompletionPercentSettings.GetCompletionPercentString(m_gameCompletionPercentSettings.CalculatePercentage(logResults: true));
		CreateStatEntry("Stats_TotalKills", data.m_stats.m_kills.ToString());
		CreateStatEntry("Stats_KillsWithGuns", data.m_stats.m_killsWithGuns.ToString());
		CreateStatEntry("Stats_KillsWithMelee", data.m_stats.m_killsWithMelee.ToString());
		AddStatsSpacer(8f);
		CreateStatEntry("Stats_DamageTaken", data.m_stats.m_damageTaken.ToString()).SetValueColor(m_badStatColor);
		AddStatsSpacer(8f);
		CreateStatEntry("Stats_ArtifactsFound", mainInventory.ArtifactItems.Count + " / " + m_artifactGameSettings.Artifacts.Length);
		CreateStatEntry("Stats_HealingItemsUsed", data.m_stats.m_healingItemsUsed.ToString());
		CreateStatEntry("Stats_FoodAndDrinkUsed", data.m_stats.m_foodAndDrinkItemsUsed.ToString());
		CreateStatEntry("Stats_TotalMoneySpent", data.m_stats.m_totalMoneySpent.ToString());
		CreateStatEntry("Stats_DistanceTravelled", (data.m_stats.m_distanceTravelled / 1000f).ToString("0.00") + " km");
	}

	private IEnumerator ShowAnimation()
	{
		yield return m_mainCanvasGroup.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		yield return new WaitForSecondsRealtime(2f);
		yield return m_statsCanvasGroup.DOFade(1f, 2f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		yield return new WaitForSecondsRealtime(2f);
		m_menuInputPrompts.gameObject.SetActive(value: true);
		m_menuInputPrompts.SetInputs(m_inputs);
		GameInputManager.GameInputActions.Game.Proceed.performed += OnProceedPerformed;
	}

	private void OnProceedPerformed(InputAction.CallbackContext contine)
	{
		if (!m_isExiting)
		{
			m_isExiting = true;
			StartCoroutine(ExitFlow());
		}
	}

	private IEnumerator ExitFlow()
	{
		m_menuInputPrompts.gameObject.SetActive(value: false);
		yield return m_mainCanvasGroup.DOFade(0f, 1f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		GlobalReferences.Instance.GameState.ClearStateFlag(GameState.GameStateFlag.GameActive);
		SceneManager.LoadScene("Assets/Scenes/Utility/Startup.unity");
	}

	private void OnDestroy()
	{
		m_gameEndingState = GameEnding.Undefined;
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= OnProceedPerformed;
		}
	}
}
