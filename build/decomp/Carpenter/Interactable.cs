using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
[AddComponentMenu("Gameplay/Interactable")]
public class Interactable : BaseInteractable, IPersistentComponent, IObjectVisibilityListener
{
	public enum ReusableMode
	{
		OnceOnly,
		Toggle,
		Reusable
	}

	[Serializable]
	private struct InteractEvents
	{
		[Tooltip("Triggered when an interact finishes, no matter whether it was completed or not")]
		public UnityEvent m_onInteractFinished;
	}

	[Serializable]
	private class PersistentData
	{
		public bool m_isDone;

		public bool m_interactEnabled;

		public bool m_gameObjectDisabled;

		public int m_maxReachedStep;
	}

	[SerializeField]
	private int m_priority;

	[Header("Interact Requirement")]
	[SerializeField]
	private ItemDefinition m_requiresItemForInteract;

	private bool m_interactEnabled = true;

	private bool m_interactionDone;

	private bool m_isVisible = true;

	[Header("Interact Commands")]
	[SerializeReference]
	private List<BaseInteractableCommand> m_interactableCommands = new List<BaseInteractableCommand>();

	[SerializeReference]
	private List<BaseInteractableCommand> m_resetInteractableCommands = new List<BaseInteractableCommand>();

	[Header("Re-Use Mode")]
	[SerializeField]
	private ReusableMode m_reusableMode;

	[SerializeField]
	private bool m_startToggled;

	[Header("Strings")]
	[SerializeField]
	protected LocalizedString m_interactStringReference = new LocalizedString("InteractPrompts", "Use");

	[SerializeField]
	protected LocalizedString m_toggleResetStringReference;

	private bool m_isDone;

	[Header("Misc")]
	[SerializeField]
	private InteractPromptIconType m_interactPromptIconType;

	[SerializeField]
	private InteractEvents m_interactMiscEvents;

	private BaseInteractableCommand m_activeCommand;

	private Coroutine m_activeCoroutine;

	private bool m_fsmEventDone;

	private PersistentDataObject m_persistentDataObject;

	private PersistentData m_persistentData;

	public override int InteractPriority => m_priority;

	public bool InteractionDone => m_interactionDone;

	public List<BaseInteractableCommand> InteractableCommands
	{
		get
		{
			return m_interactableCommands;
		}
		set
		{
			m_interactableCommands = value;
		}
	}

	public List<BaseInteractableCommand> ResetInteractableCommands
	{
		get
		{
			return m_resetInteractableCommands;
		}
		set
		{
			m_resetInteractableCommands = value;
		}
	}

	private string NormalInteractString
	{
		get
		{
			if (m_interactStringReference != null && !m_interactStringReference.IsEmpty)
			{
				ItemPickup component = GetComponent<ItemPickup>();
				LevelTransition component2 = GetComponent<LevelTransition>();
				List<object> list = new List<object>();
				if (component != null)
				{
					list.Add(component.ItemDefinition.ItemName);
					return m_interactStringReference.GetLocalizedString(list);
				}
				if (component2 != null && component2.TargetLevel != null)
				{
					LocalizedString localizedString;
					if (component2.TargetLevel.SpacialType == LevelMetadata.LevelSpacialType.Interior)
					{
						localizedString = new LocalizedString("InteractPrompts", "Enter_Basic");
					}
					else
					{
						LevelMetadata item = GlobalReferences.Instance.Anchors.Generic.ActiveLevelMetadata.Item;
						localizedString = ((!(item != null) || item.SpacialType != LevelMetadata.LevelSpacialType.Interior) ? new LocalizedString("InteractPrompts", "Travel_Basic") : new LocalizedString("InteractPrompts", "Exit_Basic"));
					}
					return localizedString.GetLocalizedString();
				}
				list.Add("");
				return m_interactStringReference.GetLocalizedString(list);
			}
			return "<Missing String>";
		}
	}

	private string ToggleString
	{
		get
		{
			if (m_toggleResetStringReference != null && !m_toggleResetStringReference.IsEmpty)
			{
				return m_toggleResetStringReference.GetLocalizedString();
			}
			if (m_interactStringReference != null && !m_interactStringReference.IsEmpty)
			{
				return m_interactStringReference.GetLocalizedString();
			}
			return "<Missing Toggle>";
		}
	}

	public override string InteractString
	{
		get
		{
			if (m_reusableMode != ReusableMode.Toggle || !m_isDone)
			{
				return NormalInteractString;
			}
			return ToggleString;
		}
	}

	public BaseInteractableCommand ActiveCommand => m_activeCommand;

	public bool FSMEventDone
	{
		get
		{
			return m_fsmEventDone;
		}
		set
		{
			m_fsmEventDone = value;
		}
	}

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (m_requiresItemForInteract != null && interactor != null)
		{
			bool flag = false;
			CharacterInventory component = interactor.GetComponent<CharacterInventory>();
			if (component != null)
			{
				flag = component.Inventory.HasSeenItemType(m_requiresItemForInteract);
			}
			if (!flag)
			{
				return false;
			}
		}
		if (m_interactEnabled && base.enabled)
		{
			return m_isVisible;
		}
		return false;
	}

	public override InteractPromptIconType GetInteractPromptIconType()
	{
		return m_interactPromptIconType;
	}

	public override InteractType GetInteractType()
	{
		return InteractType.Interactable;
	}

	public override void Interact(BaseInteractor interactor)
	{
		m_activeCoroutine = StartCoroutine(DoInteractFlow(interactor));
	}

	private IEnumerator DoInteractFlow(BaseInteractor interactor)
	{
		InteractionResult result = new InteractionResult();
		m_interactionDone = false;
		if (!m_isDone)
		{
			int index = 0;
			foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
			{
				BaseInteractableCommand command2 = (m_activeCommand = interactableCommand);
				command2.SetupInteraction(this, interactor);
				yield return command2.DoInteraction(this, interactor, result);
				command2.FinishInteraction(this, interactor);
				m_activeCommand = null;
				if (result.m_cancel)
				{
					break;
				}
				index++;
			}
			if (m_persistentData != null)
			{
				m_persistentData.m_maxReachedStep = index;
			}
			if (!result.m_cancel)
			{
				if (m_reusableMode != ReusableMode.Reusable)
				{
					m_isDone = true;
				}
				if (m_reusableMode == ReusableMode.OnceOnly)
				{
					m_interactEnabled = false;
					if (m_persistentData != null)
					{
						m_persistentData.m_interactEnabled = false;
					}
				}
			}
		}
		else
		{
			foreach (BaseInteractableCommand resetInteractableCommand in m_resetInteractableCommands)
			{
				BaseInteractableCommand command2 = (m_activeCommand = resetInteractableCommand);
				command2.SetupInteraction(this, interactor);
				yield return command2.DoInteraction(this, interactor, result);
				command2.FinishInteraction(this, interactor);
				m_activeCommand = null;
				if (result.m_cancel)
				{
					break;
				}
			}
			if (m_persistentData != null)
			{
				m_persistentData.m_maxReachedStep = 0;
			}
			if (!result.m_cancel)
			{
				m_isDone = false;
			}
		}
		ReusableMode reusableMode = m_reusableMode;
		if ((uint)(reusableMode - 1) <= 1u)
		{
			StartCoroutine(ResetInteract());
		}
		if (m_reusableMode != ReusableMode.Reusable && m_persistentData != null)
		{
			m_persistentData.m_isDone = m_isDone;
		}
		m_interactionDone = true;
		m_interactMiscEvents.m_onInteractFinished?.Invoke();
		if (m_isDone && m_reusableMode == ReusableMode.OnceOnly)
		{
			SetInteractDisabled();
		}
		if (result.m_disableGameObject)
		{
			base.gameObject.SetActive(value: false);
			if (m_persistentData != null)
			{
				m_persistentData.m_gameObjectDisabled = true;
			}
		}
		if ((bool)result.m_followUpInteractable)
		{
			Debug.Log("Do follow up with: " + result.m_followUpInteractable);
			if (interactor is CharacterInteractor characterInteractor)
			{
				characterInteractor.DoForceInteract(result.m_followUpInteractable);
			}
			else
			{
				Debug.LogError("Follow up interactable is only supported for CharacterInteractor currently");
			}
		}
	}

	private void SetInteractDisabled()
	{
		UIMinigameHighlight component = GetComponent<UIMinigameHighlight>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private IEnumerator ResetInteract()
	{
		m_interactEnabled = false;
		yield return new WaitForSeconds(0.5f);
		m_interactEnabled = true;
	}

	protected override void Start()
	{
		base.Start();
		ObjectVisibility.AddToGameObjectIfMissing(base.gameObject);
	}

	private void OnDestroy()
	{
		if (m_activeCommand != null)
		{
			m_activeCommand.Cancel();
			m_activeCommand = null;
		}
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			interactableCommand?.Cleanup();
		}
	}

	private void OnDisable()
	{
		if (m_activeCommand != null && !m_interactionDone)
		{
			FinishedInteract();
		}
	}

	public void SetAlreadyInteracted()
	{
		if (m_isDone)
		{
			return;
		}
		m_isDone = true;
		if (m_persistentData != null)
		{
			m_persistentData.m_isDone = m_isDone;
		}
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			interactableCommand.ApplyPersistentEffects(this);
		}
	}

	public override void FinishedInteract()
	{
		base.FinishedInteract();
		if (m_activeCommand != null)
		{
			m_activeCommand.Cancel();
			m_activeCommand = null;
		}
		if (m_activeCoroutine != null)
		{
			StopCoroutine(m_activeCoroutine);
		}
		m_interactionDone = true;
	}

	public void ResetInteractData()
	{
		m_isDone = false;
		if (m_persistentData != null)
		{
			m_persistentData.m_isDone = false;
			m_persistentData.m_gameObjectDisabled = false;
			m_persistentData.m_maxReachedStep = 0;
			m_persistentData.m_interactEnabled = true;
		}
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			interactableCommand.Reset();
		}
		foreach (BaseInteractableCommand resetInteractableCommand in m_resetInteractableCommands)
		{
			resetInteractableCommand.Reset();
		}
	}

	public void ForceSetInteractDone(bool done)
	{
		m_isDone = done;
		if (m_persistentData != null)
		{
			m_persistentData.m_isDone = m_isDone;
		}
	}

	public bool HasExaminable()
	{
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			if (interactableCommand is InteractableExaminableCommand)
			{
				return true;
			}
		}
		foreach (BaseInteractableCommand resetInteractableCommand in m_resetInteractableCommands)
		{
			if (resetInteractableCommand is InteractableExaminableCommand)
			{
				return true;
			}
		}
		return false;
	}

	public void SetInteractEnabled(bool interactable)
	{
		m_interactEnabled = interactable;
		if (m_persistentData != null)
		{
			m_persistentData.m_interactEnabled = m_interactEnabled;
		}
	}

	public bool RequiresPersistentData()
	{
		if (m_reusableMode != ReusableMode.Reusable)
		{
			return true;
		}
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			if (interactableCommand.RequiresPersistentData())
			{
				return true;
			}
		}
		return false;
	}

	public void ReceiveDataStoreEntry(PersistentDataObject dataEntry)
	{
		m_persistentDataObject = dataEntry;
		m_persistentData = ((m_persistentDataObject.Data != null) ? (m_persistentDataObject.Data as PersistentData) : null);
		PersistentDataIdentifier component = GetComponent<PersistentDataIdentifier>();
		int num = 0;
		GameObject[] rootGameObjects = base.gameObject.scene.GetRootGameObjects();
		bool flag = false;
		GameObject[] array = rootGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].GetComponent<MinigameScene>())
			{
				flag = true;
				break;
			}
		}
		string text = "";
		if (flag)
		{
			GameObject item = GlobalReferences.Instance.Anchors.Minigame.ActiveMinigameInteractableAnchor.Item;
			if (item != null)
			{
				PersistentDataIdentifier component2 = item.GetComponent<PersistentDataIdentifier>();
				if (component2 != null)
				{
					text = component2.GUID;
				}
			}
		}
		string text2 = ((!flag) ? (base.gameObject.scene.name + ":" + component.GUID) : (text + ":" + component.GUID));
		foreach (BaseInteractableCommand interactableCommand in m_interactableCommands)
		{
			if (interactableCommand is IPersistentComponent)
			{
				IPersistentComponent obj = interactableCommand as IPersistentComponent;
				PersistentDataObject dataEntry2 = GlobalReferences.Instance.DataStore.Data.GetDataEntry(text2 + ":InteractableCommand:" + num);
				num++;
				obj.ReceiveDataStoreEntry(dataEntry2);
			}
		}
		foreach (BaseInteractableCommand resetInteractableCommand in m_resetInteractableCommands)
		{
			if (resetInteractableCommand is IPersistentComponent)
			{
				IPersistentComponent obj2 = resetInteractableCommand as IPersistentComponent;
				PersistentDataObject dataEntry3 = GlobalReferences.Instance.DataStore.Data.GetDataEntry(text2 + ":InteractableCommandReset:" + num);
				num++;
				obj2.ReceiveDataStoreEntry(dataEntry3);
			}
		}
		if (m_persistentData != null)
		{
			m_interactEnabled = m_persistentData.m_interactEnabled;
			if (m_persistentData.m_isDone)
			{
				m_isDone = m_persistentData.m_isDone;
				foreach (BaseInteractableCommand interactableCommand2 in m_interactableCommands)
				{
					interactableCommand2.ApplyPersistentEffects(this);
				}
				SetInteractDisabled();
			}
			else
			{
				for (int j = 0; j < m_persistentData.m_maxReachedStep; j++)
				{
					if (m_interactableCommands.Count > j)
					{
						m_interactableCommands[j].ApplyPersistentEffects(this);
					}
				}
			}
			if (m_persistentData.m_gameObjectDisabled)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_persistentData = new PersistentData();
			m_persistentDataObject.Data = m_persistentData;
			m_persistentData.m_interactEnabled = m_interactEnabled;
			m_persistentData.m_isDone = m_isDone;
			if (m_reusableMode == ReusableMode.Toggle && m_startToggled)
			{
				SetAlreadyInteracted();
			}
		}
	}

	public void DoMinigameInteract()
	{
		MinigameInteractor componentInParent = GetComponentInParent<MinigameInteractor>();
		if (componentInParent != null)
		{
			componentInParent.RequestInteracat(this);
		}
		else
		{
			Debug.LogError("Tried to use DoMinigameInteract but there is no MinigameInteractor in the minigame scene.");
		}
	}

	public void SetObjectVisibility(bool visible)
	{
		m_isVisible = visible;
	}
}
