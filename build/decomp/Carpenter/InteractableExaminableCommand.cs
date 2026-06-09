using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

[Serializable]
public class InteractableExaminableCommand : BaseInteractableCommand, IPersistentComponent
{
	[Serializable]
	private class PersistentData
	{
		public bool m_examined;
	}

	[SerializeField]
	private List<ExaminableSection> m_sections;

	[SerializeField]
	private UnityEvent m_onExaminationDoneEvent;

	[SerializeField]
	private bool m_onlyOnce;

	[SerializeField]
	private bool m_choicePrompt;

	[SerializeField]
	private LocalizedString m_confirmTextStringReference = new LocalizedString("Examinable", "Examinable_Prompt_Yes");

	[SerializeField]
	private LocalizedString m_cancelTextStringRference = new LocalizedString("Examinable", "Examinable_Prompt_Cancel");

	[SerializeField]
	private BaseInteractable m_cancelInteractable;

	private bool m_isMinigame;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	private bool m_finishedExamine;

	private bool m_shouldContinue;

	public List<ExaminableSection> Sections => m_sections;

	public bool HasChoice => m_choicePrompt;

	public string ConfirmText => m_confirmTextStringReference.GetLocalizedString();

	public string CancelText => m_cancelTextStringRference.GetLocalizedString();

	public static InteractableExaminableCommand CreateInstance(LocalizedString text, bool isChoice)
	{
		List<ExaminableSection> list = new List<ExaminableSection>();
		list.Add(new ExaminableSection
		{
			m_textStringReference = text
		});
		return new InteractableExaminableCommand
		{
			m_sections = list,
			m_onlyOnce = false,
			m_choicePrompt = isChoice
		};
	}

	public string GetText(int index)
	{
		return LocalizationUtility.GetLocalizedString(m_sections[index].m_textStringReference, m_sections[index].m_text);
	}

	public Sprite GetSprite(int index)
	{
		return m_sections[index].m_image;
	}

	public bool HasMoreEntries(int index)
	{
		return index + 1 < m_sections.Count;
	}

	public void ExaminationDone()
	{
		m_onExaminationDoneEvent?.Invoke();
		if (m_persistentData != null)
		{
			m_persistentData.m_examined = true;
		}
		else if (m_onlyOnce)
		{
			Debug.LogError("Examinable is marked as only once but has no PersistenDataIdentifier component, so this state cannot be saved! Please fix!");
		}
	}

	public override IEnumerator DoInteraction(BaseInteractable interactable, BaseInteractor interactor, InteractionResult result)
	{
		bool flag = false;
		if (m_onlyOnce && m_persistentData != null && m_persistentData.m_examined)
		{
			flag = true;
		}
		if (!flag)
		{
			m_finishedExamine = false;
			m_shouldContinue = false;
			GlobalReferences.Instance.Anchors.Generic.ExaminableAnchor.Set(this);
			m_isMinigame = interactor is MinigameInteractor;
			if (m_isMinigame)
			{
				GlobalReferences.Instance.EventChannels.Examine.ShowMinigameExaminable.Raise();
				GlobalReferences.Instance.EventChannels.Examine.DoneMinigameExamining.Register(OnDoneExamining);
			}
			else
			{
				GlobalReferences.Instance.EventChannels.Examine.ShowExaminable.Raise();
				GlobalReferences.Instance.EventChannels.Examine.DoneExamining.Register(OnDoneExamining);
			}
			yield return new WaitUntil(IsFinishedExamine);
			ExaminationDone();
			OnExit();
			result.m_cancel = !m_shouldContinue;
			if (!m_shouldContinue && m_cancelInteractable != null)
			{
				result.m_followUpInteractable = m_cancelInteractable;
			}
		}
	}

	private void OnExit()
	{
		if (m_isMinigame)
		{
			GlobalReferences.Instance.EventChannels.Examine.CancelMinigameExamining.Raise();
			GlobalReferences.Instance.EventChannels.Examine.DoneMinigameExamining.Unregister(OnDoneExamining);
		}
		else
		{
			GlobalReferences.Instance.EventChannels.Examine.CancelExamining.Raise();
			GlobalReferences.Instance.EventChannels.Examine.DoneExamining.Unregister(OnDoneExamining);
		}
		GlobalReferences.Instance.Anchors.Generic.ExaminableAnchor.Set(null);
	}

	public override void Cancel()
	{
		base.Cancel();
		OnExit();
	}

	public override bool RequiresPersistentData()
	{
		return m_onlyOnce;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		if (m_persistentData == null)
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
		}
	}

	private bool IsFinishedExamine()
	{
		return m_finishedExamine;
	}

	private void OnDoneExamining(bool shouldContinue)
	{
		m_finishedExamine = true;
		m_shouldContinue = shouldContinue;
	}
}
