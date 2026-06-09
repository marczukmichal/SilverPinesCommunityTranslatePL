using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class InteractablePhoneCommand : BaseInteractableCommand
{
	[SerializeField]
	private MinigameMetadata m_phoneMinigameMetadata;

	[SerializeField]
	private PhoneBook m_phoneBook;

	[SerializeField]
	private bool m_isPayphone;

	[SerializeField]
	private GameMenuState m_gameMenuState;

	[SerializeField]
	private PhoneFlags m_phoneFlags;

	private MinigameScene m_activeMinigameScene;

	private PhoneMinigame m_phoneMinigame;

	private SceneInstance m_loadedMinigameSceneInstance;

	private BaseInteractable m_parentInteractable;

	private BaseInteractor m_interactor;

	private PhoneInteraction m_phoneInteraction;

	private bool m_shouldExit;

	private PhoneEvent m_selectedPhoneEvent;

	private PhoneNumber m_selectedPhoneNumber;

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		m_interactor = interactor;
		m_parentInteractable = interactable;
		m_shouldExit = false;
		AsyncOperationHandle<SceneInstance> loadHandle = Addressables.LoadSceneAsync(m_phoneMinigameMetadata.AssetReference, LoadSceneMode.Additive);
		yield return new WaitUntil(() => loadHandle.IsDone);
		m_loadedMinigameSceneInstance = loadHandle.Result;
		GameObject[] rootGameObjects = m_loadedMinigameSceneInstance.Scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if ((bool)rootGameObjects[i].GetComponent<MinigameScene>())
			{
				m_activeMinigameScene = rootGameObjects[i].GetComponent<MinigameScene>();
				m_phoneMinigame = rootGameObjects[i].GetComponentInChildren<PhoneMinigame>();
			}
		}
		MinigameScene activeMinigameScene = m_activeMinigameScene;
		activeMinigameScene.m_onMinigameClose = (UnityAction)Delegate.Combine(activeMinigameScene.m_onMinigameClose, new UnityAction(CancelMinigame));
		PhoneMinigame phoneMinigame = m_phoneMinigame;
		phoneMinigame.m_onNumberDialed = (UnityAction)Delegate.Combine(phoneMinigame.m_onNumberDialed, new UnityAction(OnNumberDialed));
		PhoneMinigame phoneMinigame2 = m_phoneMinigame;
		phoneMinigame2.m_onPhoneNumberSelected = (UnityAction<string>)Delegate.Combine(phoneMinigame2.m_onPhoneNumberSelected, new UnityAction<string>(OnNumberSelected));
		m_phoneMinigame.Setup(m_isPayphone, interactable.GetComponent<PhoneMinigameData>());
		yield return new WaitUntil(() => m_shouldExit);
	}

	private void UnloadMinigame()
	{
		MinigameScene activeMinigameScene = m_activeMinigameScene;
		activeMinigameScene.m_onMinigameClose = (UnityAction)Delegate.Remove(activeMinigameScene.m_onMinigameClose, new UnityAction(CancelMinigame));
		PhoneMinigame phoneMinigame = m_phoneMinigame;
		phoneMinigame.m_onNumberDialed = (UnityAction)Delegate.Remove(phoneMinigame.m_onNumberDialed, new UnityAction(OnNumberDialed));
		PhoneMinigame phoneMinigame2 = m_phoneMinigame;
		phoneMinigame2.m_onPhoneNumberSelected = (UnityAction<string>)Delegate.Remove(phoneMinigame2.m_onPhoneNumberSelected, new UnityAction<string>(OnNumberSelected));
		Addressables.UnloadSceneAsync(m_loadedMinigameSceneInstance);
	}

	private void OnNumberSelected(string number)
	{
		PhoneNumber numberDataFromNumber = m_phoneBook.GetNumberDataFromNumber(number);
		PhoneEvent phoneEvent = null;
		if (numberDataFromNumber != null)
		{
			phoneEvent = m_phoneBook.GetBestPhoneEvent(numberDataFromNumber, m_phoneFlags);
			if (numberDataFromNumber.m_shouldSaveGame && phoneEvent != null)
			{
				m_phoneBook.SetPhoneSaveEvent(phoneEvent);
			}
		}
		m_selectedPhoneEvent = phoneEvent;
		m_selectedPhoneNumber = numberDataFromNumber;
	}

	private void OnNumberDialed()
	{
		PhoneEvent selectedPhoneEvent = m_selectedPhoneEvent;
		if ((bool)m_phoneMinigame && selectedPhoneEvent == null)
		{
			m_phoneMinigame.ReturnCoin();
		}
		m_phoneInteraction = new PhoneInteraction(m_selectedPhoneNumber, selectedPhoneEvent, m_phoneBook);
		UnloadMinigame();
		m_parentInteractable.StartCoroutine(DoCall());
	}

	private IEnumerator DoCall()
	{
		yield return m_phoneInteraction.CallNumber(m_interactor.transform);
		m_phoneInteraction.Cleanup();
		m_phoneInteraction = null;
		m_shouldExit = true;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (m_phoneInteraction != null)
		{
			m_phoneInteraction.Cleanup();
			m_phoneInteraction = null;
		}
	}

	private void CancelMinigame()
	{
		if ((bool)m_phoneMinigame && m_phoneMinigame.CanReturnCoin())
		{
			m_phoneMinigame.ReturnCoin();
		}
		UnloadMinigame();
		m_shouldExit = true;
	}
}
