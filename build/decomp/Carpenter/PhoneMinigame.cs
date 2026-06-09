using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class PhoneMinigame : MonoBehaviour, IMinigameBackInputHandler
{
	[SerializeField]
	private TextMeshProUGUI m_codeDisplay;

	[SerializeField]
	private int m_maxLength = 10;

	[SerializeField]
	private UIMinigameHighlight m_coinHighlight;

	[SerializeField]
	private UIMinigameHighlight m_callHighlight;

	[Header("Options")]
	[SerializeField]
	private bool m_useHyphens;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_coinAddAudioEvent;

	[SerializeField]
	private AudioEvent m_coinReturnAudioEvent;

	[SerializeField]
	private AudioEvent m_dialingAudioEvent;

	[SerializeField]
	private AudioEvent m_phoneKeyToneAudioEvent;

	[SerializeField]
	private AudioEvent m_noCoinInsertedButtonPressedAudioEvent;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_onCoinAdded;

	[Header("LCD Display")]
	[SerializeField]
	private int m_lcdMaxCharacters = 7;

	[SerializeField]
	private float m_animTime = 0.3f;

	[SerializeField]
	private float m_flashTimerOnTimer = 1.25f;

	[SerializeField]
	private float m_flashTimerOffTimer = 0.75f;

	[Header("Hints")]
	[SerializeField]
	private BoolVariable m_donePhoneHints;

	[SerializeField]
	private HintInfo m_insertMoneyHint;

	[SerializeField]
	private HintInfo m_dialNumberHint;

	[Header("Save")]
	[SerializeField]
	private float m_saveFadeInTime = 1f;

	[SerializeField]
	private CanvasGroup m_phoneElementsCanvasGroup;

	[SerializeField]
	private CanvasGroup m_saveCanvasGroup;

	[SerializeField]
	private SaveSelectPanel m_saveSelectPanel;

	[Header("Phone Book")]
	[SerializeField]
	private PhoneBook m_phoneBook;

	[SerializeField]
	private Transform m_phoneBookListParent;

	[SerializeField]
	private PhoneNumberButton m_phoneNumButtonPrefab;

	[SerializeField]
	private RectTransform m_phoneBookContainer;

	[SerializeField]
	private CanvasGroup m_phoneBookCanvasGroup;

	[Header("Coin Return Slot")]
	[SerializeField]
	private GameObject m_coinCheckSlotButton;

	[SerializeField]
	private Image m_coinSlotImage;

	[SerializeField]
	private CoinReturnVisuals m_coinReturnVisuals;

	public UnityAction<string> m_onPhoneNumberSelected;

	public UnityAction m_onNumberDialed;

	private int m_lcdTextLoopIndex;

	private bool m_blockInput;

	private bool m_requiresCoin;

	private bool m_insertedCoin;

	private bool m_shouldFlashText;

	private float m_flashTimer;

	private PhoneMinigameData m_phoneData;

	private string m_stringToShow;

	private string m_currentNumber = "";

	private int m_selectedSaveIndex = -1;

	private bool m_waitingForSave = true;

	private bool m_doneSave;

	private bool ShouldFlashText
	{
		set
		{
			if (m_shouldFlashText != value)
			{
				m_shouldFlashText = value;
				m_codeDisplay.gameObject.SetActive(value: true);
				m_flashTimer = 0f;
			}
		}
	}

	public string StringToShow => m_stringToShow;

	public bool InsertedCoin => m_insertedCoin;

	public bool RequiresCoin
	{
		get
		{
			return m_requiresCoin;
		}
		set
		{
			if (value != m_requiresCoin)
			{
				m_requiresCoin = value;
				if (m_requiresCoin)
				{
					ShouldFlashText = true;
					m_stringToShow = "INSERT COIN";
					m_phoneBookContainer.pivot = new Vector2(-1f, 0.5f);
					m_phoneBookCanvasGroup.alpha = 0f;
				}
				else
				{
					UpdateDisplay();
					m_phoneBookContainer.DOPivotX(1f, 0.5f).SetUpdate(isIndependentUpdate: true);
					m_phoneBookCanvasGroup.DOFade(1f, 0.5f);
					m_coinHighlight.enabled = false;
				}
			}
		}
	}

	private string CurrentNumber
	{
		get
		{
			return m_currentNumber;
		}
		set
		{
			if (m_currentNumber != value)
			{
				m_currentNumber = value;
				UpdateDisplay();
			}
		}
	}

	private void Start()
	{
		m_flashTimer = 0f;
		m_selectedSaveIndex = -1;
		m_doneSave = false;
		if (m_callHighlight != null)
		{
			m_callHighlight.enabled = false;
		}
		PopulatePhoneBook();
		StartCoroutine(LCDDisplayUpdateLoop());
		if ((bool)m_donePhoneHints && !m_donePhoneHints.Value)
		{
			StartCoroutine(PhoneHintFlow());
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Register(OnSaveCompletedSuccesfully);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedFailed.Register(OnSaveFailed);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedSuccesfully.Unregister(OnSaveCompletedSuccesfully);
		GlobalReferences.Instance.EventChannels.SaveLoad.SaveCompletedFailed.Unregister(OnSaveFailed);
	}

	private void PopulatePhoneBook()
	{
		List<PhoneNumber> knownNumbers = m_phoneBook.GetKnownNumbers();
		foreach (PhoneNumber item in knownNumbers)
		{
			Object.Instantiate(m_phoneNumButtonPrefab, m_phoneBookListParent).Setup(item, this);
		}
		if (knownNumbers.Count > 0)
		{
			m_phoneBookContainer.gameObject.SetActive(value: true);
		}
		else
		{
			m_phoneBookContainer.gameObject.SetActive(value: false);
		}
	}

	public void Setup(bool isPayphone, PhoneMinigameData phoneMinigameData)
	{
		RequiresCoin = isPayphone;
		if (m_coinHighlight != null)
		{
			m_coinHighlight.enabled = RequiresCoin;
		}
		m_phoneData = phoneMinigameData;
		if (m_phoneData != null && m_phoneData.HasTakenCoins() && !CanGenerateMagicCoin())
		{
			m_coinCheckSlotButton.SetActive(value: false);
		}
	}

	private bool CanGenerateMagicCoin()
	{
		if (GlobalReferences.Instance.MainInventory.Money <= 1 && m_phoneData != null && !m_phoneData.HasRecentlyTakenCoin())
		{
			return true;
		}
		return false;
	}

	private void AddInput(string input)
	{
		if (!m_blockInput && !m_requiresCoin && CurrentNumber.Length < m_maxLength)
		{
			StopAllCoroutines();
			if (CurrentNumber.Length == 3 && m_useHyphens)
			{
				input = "-" + input;
			}
			CurrentNumber += input;
		}
	}

	public void PressedButton(int number)
	{
		if (m_requiresCoin)
		{
			m_noCoinInsertedButtonPressedAudioEvent.Play2D();
			return;
		}
		m_phoneKeyToneAudioEvent.Play2D();
		AddInput(number.ToString());
	}

	public void SetNumber(string number)
	{
		if (m_requiresCoin)
		{
			TryAddCoin();
			if (m_requiresCoin)
			{
				return;
			}
		}
		StopAllCoroutines();
		StartCoroutine(AutoDialCoroutine(number));
	}

	private IEnumerator AutoDialCoroutine(string number)
	{
		CurrentNumber = "";
		yield return new WaitForSecondsRealtime(0.04f);
		for (int i = 0; i < number.Length; i++)
		{
			if (i == 3 && m_useHyphens)
			{
				CurrentNumber += "-";
				yield return new WaitForSecondsRealtime(0.04f);
			}
			m_phoneKeyToneAudioEvent.Play2D();
			CurrentNumber += number[i];
			yield return new WaitForSecondsRealtime(Random.Range(0.2f, 0.5f));
		}
		Call();
	}

	public void Delete()
	{
		if (m_blockInput)
		{
			return;
		}
		m_noCoinInsertedButtonPressedAudioEvent.Play2D();
		if (!m_requiresCoin && CurrentNumber.Length > 0)
		{
			if (CurrentNumber.Length == 5 && m_useHyphens)
			{
				string currentNumber = CurrentNumber;
				CurrentNumber = currentNumber.Substring(0, currentNumber.Length - 2);
			}
			else
			{
				string currentNumber = CurrentNumber;
				CurrentNumber = currentNumber.Substring(0, currentNumber.Length - 1);
			}
		}
	}

	public void Call()
	{
		if (m_requiresCoin)
		{
			return;
		}
		m_noCoinInsertedButtonPressedAudioEvent.Play2D();
		if (!m_blockInput && CurrentNumber.Length >= 3)
		{
			m_blockInput = true;
			if (m_callHighlight != null)
			{
				m_callHighlight.enabled = false;
			}
			StartCoroutine(Dialing());
		}
	}

	private IEnumerator Dialing()
	{
		m_codeDisplay.text = "DIALING\n" + m_currentNumber;
		m_dialingAudioEvent.Play2D();
		yield return new WaitForSecondsRealtime(2f);
		string text = m_currentNumber.Replace("-", "");
		PhoneNumber numberDataFromNumber = m_phoneBook.GetNumberDataFromNumber(text);
		if (numberDataFromNumber != null && numberDataFromNumber.m_shouldSaveGame)
		{
			yield return DoSaveGameFlow(text);
			yield break;
		}
		m_onPhoneNumberSelected?.Invoke(text);
		m_onNumberDialed?.Invoke();
	}

	private IEnumerator DoSaveGameFlow(string number)
	{
		m_selectedSaveIndex = -1;
		m_saveSelectPanel.gameObject.SetActive(value: true);
		m_saveSelectPanel.Show();
		m_phoneElementsCanvasGroup.DOFade(0f, m_saveFadeInTime);
		m_saveCanvasGroup.alpha = 0f;
		m_saveCanvasGroup.DOFade(1f, m_saveFadeInTime).SetUpdate(isIndependentUpdate: true);
		m_waitingForSave = true;
		m_onPhoneNumberSelected?.Invoke(number);
		yield return new WaitUntil(() => m_selectedSaveIndex != -1);
		if (m_selectedSaveIndex != -1)
		{
			m_doneSave = true;
			GlobalReferences.Instance.EventChannels.SaveLoad.GameSaveInSlot.Raise(m_selectedSaveIndex);
			GlobalReferences.Instance.Variables.Mechanics.PV_PlayerHasSaved.SetValue(value: true);
			yield return new WaitUntil(() => !m_waitingForSave);
			yield return new WaitForSecondsRealtime(0.1f);
			m_saveSelectPanel.RefreshButton(m_selectedSaveIndex);
			m_saveSelectPanel.SetSaveDoneMode();
			yield return new WaitForSecondsRealtime(2f);
			m_onNumberDialed?.Invoke();
		}
	}

	private void OnSaveCompletedSuccesfully()
	{
		m_waitingForSave = false;
	}

	private void OnSaveFailed()
	{
		m_waitingForSave = false;
	}

	public bool TryBackInput()
	{
		if (m_saveSelectPanel.IsConfirmationDialogueActive())
		{
			m_saveSelectPanel.CancelAction();
			return true;
		}
		return false;
	}

	public void SetSelectedSaveIndex(int index)
	{
		m_selectedSaveIndex = index;
	}

	public void Clear()
	{
		if (!m_blockInput)
		{
			m_noCoinInsertedButtonPressedAudioEvent.Play2D();
			if (!m_requiresCoin)
			{
				CurrentNumber = "";
				StopAllCoroutines();
				StartCoroutine(LCDDisplayUpdateLoop());
			}
		}
	}

	private void UpdateDisplay()
	{
		if (m_currentNumber.Length == 0)
		{
			ShouldFlashText = true;
			m_stringToShow = "DIAL NUMBER";
		}
		else
		{
			ShouldFlashText = false;
			m_stringToShow = m_currentNumber + " ";
		}
		UpdateLCDText();
		if (m_callHighlight != null)
		{
			m_callHighlight.enabled = m_currentNumber.Length >= 3;
		}
	}

	public void TryAddCoin()
	{
		if (RequiresCoin && GlobalReferences.Instance.MainInventory.Money > 0)
		{
			m_onCoinAdded.Invoke();
			m_coinAddAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.RemoveMoney(1);
			GlobalReferences.Instance.DataStore.Data.m_stats.m_totalMoneySpent++;
			RequiresCoin = false;
			m_insertedCoin = true;
		}
	}

	public bool CanReturnCoin()
	{
		if (m_doneSave)
		{
			return false;
		}
		return true;
	}

	public void ReturnCoin()
	{
		if (m_insertedCoin)
		{
			m_coinReturnAudioEvent.Play2D();
			GlobalReferences.Instance.MainInventory.AddMoney(1);
			GlobalReferences.Instance.DataStore.Data.m_stats.m_totalMoneySpent--;
			m_insertedCoin = false;
		}
	}

	private void Update()
	{
		int num = GameUIUtility.CheckForNumberInputInteger();
		if (num != int.MaxValue)
		{
			PressedButton(num);
		}
		if (Keyboard.current != null)
		{
			Keyboard current = Keyboard.current;
			if (current.numpadEnterKey.wasPressedThisFrame || current.enterKey.wasReleasedThisFrame)
			{
				Call();
			}
			if (current.deleteKey.wasPressedThisFrame || current.backspaceKey.wasReleasedThisFrame || current.numpadPeriodKey.wasPressedThisFrame)
			{
				Delete();
			}
		}
		if (!m_shouldFlashText)
		{
			return;
		}
		m_flashTimer += Time.unscaledDeltaTime;
		if (m_codeDisplay.gameObject.activeSelf)
		{
			if (m_flashTimer >= m_flashTimerOnTimer)
			{
				m_codeDisplay.gameObject.SetActive(value: false);
				m_flashTimer = 0f;
			}
		}
		else if (m_flashTimer >= m_flashTimerOffTimer)
		{
			m_codeDisplay.gameObject.SetActive(value: true);
			m_flashTimer = 0f;
		}
	}

	private IEnumerator LCDDisplayUpdateLoop()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(m_animTime);
			bool flag = false;
			if (m_lcdTextLoopIndex >= StringToShow.Length)
			{
				m_lcdTextLoopIndex = 0;
				flag = true;
			}
			UpdateLCDText();
			if (!flag)
			{
				m_lcdTextLoopIndex++;
			}
			if (m_lcdTextLoopIndex >= StringToShow.Length)
			{
				m_lcdTextLoopIndex = 0;
			}
		}
	}

	private void UpdateLCDText()
	{
		if (StringToShow.Length < m_lcdMaxCharacters)
		{
			m_codeDisplay.text = StringToShow;
		}
		else
		{
			m_codeDisplay.text = GameUIUtility.WrapAroundSubstring(StringToShow, m_lcdTextLoopIndex, m_lcdMaxCharacters);
		}
	}

	private IEnumerator PhoneHintFlow()
	{
		yield return new WaitForSeconds(1f);
		GlobalReferences.Instance.EventChannels.Hints.ShowMinigameHintInfo.Raise(m_insertMoneyHint);
		yield return new WaitUntil(() => m_insertedCoin);
		GlobalReferences.Instance.EventChannels.Hints.CancelHintInfo.Raise(m_insertMoneyHint);
		yield return new WaitForSeconds(1f);
		GlobalReferences.Instance.EventChannels.Hints.ShowMinigameHintInfo.Raise(m_dialNumberHint);
	}

	public IEnumerator DoPhoneCoinSlotCommand(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		int coins = 0;
		if (m_phoneData != null && m_phoneData.CoinSlotCoins > 0)
		{
			coins = m_phoneData.CoinSlotCoins;
		}
		else if (CanGenerateMagicCoin())
		{
			coins = 1;
		}
		if (m_phoneData != null)
		{
			m_phoneData.RemoveCoins();
		}
		m_coinSlotImage.enabled = false;
		if (coins > 0)
		{
			GlobalReferences.Instance.EventChannels.Inventory.AddPlayerMoney.Raise(coins);
		}
		yield return m_coinReturnVisuals.AnimateCoins(coins);
		InteractableExaminableCommand command = ((coins <= 0) ? InteractableExaminableCommand.CreateInstance(new LocalizedString("Minigames", "Phone_ReturnCoins_Empty"), isChoice: false) : ((coins <= 1) ? InteractableExaminableCommand.CreateInstance(new LocalizedString("Minigames", "Phone_ReturnCoins_OneCoin"), isChoice: false) : InteractableExaminableCommand.CreateInstance(new LocalizedString("Minigames", "Phone_ReturnCoins_MultipleCoins"), isChoice: false)));
		if (command != null)
		{
			command.SetupInteraction(interactable, interactor);
			yield return command.DoInteraction(interactable, interactor, result);
			command.FinishInteraction(interactable, interactor);
		}
	}
}
