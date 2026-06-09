using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class OptionsMenuTab : MonoBehaviour
{
	[Header("Prefabs")]
	[SerializeField]
	private GameObject m_sliderPrefab;

	[SerializeField]
	private GameObject m_dropdownPrefab;

	[SerializeField]
	private GameObject m_cyclePrefab;

	[SerializeField]
	private GameObject m_applyButtonPrefab;

	[SerializeField]
	private GameObject m_groupHeaderPrefab;

	[SerializeField]
	protected Transform m_optionsListContainer;

	[Header("References")]
	[SerializeField]
	private OptionsMenuPage m_optionsPage;

	protected GameObject m_selectGameObject;

	public GameObject SelectGameObject => m_selectGameObject;

	public void OnTabButtonPressed()
	{
		m_optionsPage.SetOpenTab(this);
	}

	protected virtual void Awake()
	{
		PopulateOptionsTab();
	}

	protected virtual void PopulateOptionsTab()
	{
	}

	protected List<LocalizedString> GetLocalisedNames(string[] names)
	{
		List<LocalizedString> list = new List<LocalizedString>();
		foreach (string text in names)
		{
			list.Add(new LocalizedString("SystemMenu", text));
		}
		return list;
	}

	protected LocalizedString GetLabelString(string name)
	{
		return new LocalizedString("SystemMenu", name);
	}

	private bool CheckForPlatform(PlatformUtils.Platforms[] platforms)
	{
		if (platforms != null)
		{
			return PlatformUtils.HasPlatformFlag(MaskUtils.ConvertToMask(platforms));
		}
		return true;
	}

	protected CycleButton AddToggleOption(string label, bool startingValue, UnityAction<bool> action, PlatformUtils.Platforms[] platforms = null)
	{
		if (!CheckForPlatform(platforms))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_cyclePrefab, m_optionsListContainer);
		CycleButton componentInChildren = gameObject.GetComponentInChildren<CycleButton>();
		componentInChildren.ClearOptions();
		List<LocalizedString> options = new List<LocalizedString>
		{
			new LocalizedString("UICommon", "Off"),
			new LocalizedString("UICommon", "On")
		};
		componentInChildren.AddOptions(options);
		componentInChildren.Value = (startingValue ? 1 : 0);
		componentInChildren.m_onValueChangedBool = (UnityAction<bool>)Delegate.Combine(componentInChildren.m_onValueChangedBool, action);
		AudioTrigger audioTrigger = gameObject.GetComponent<AudioTrigger>();
		if (audioTrigger != null)
		{
			componentInChildren.m_onValueChangedBool = (UnityAction<bool>)Delegate.Combine(componentInChildren.m_onValueChangedBool, (UnityAction<bool>)delegate
			{
				audioTrigger.TriggerAudio();
			});
		}
		componentInChildren.SetLabel(GetLabelString(label));
		RegisterSelectable(gameObject);
		return componentInChildren;
	}

	protected SliderExtended AddSliderOption(string label, float minValue, float maxValue, float startingValue, int stepCount, UnityAction<float> action, SliderValueType sliderValueType, PlatformUtils.Platforms[] platforms = null)
	{
		if (!CheckForPlatform(platforms))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_sliderPrefab, m_optionsListContainer);
		SliderExtended componentInChildren = gameObject.GetComponentInChildren<SliderExtended>();
		componentInChildren.m_onExtendedValueChanged = (UnityAction<float>)Delegate.Combine(componentInChildren.m_onExtendedValueChanged, action);
		componentInChildren.Setup(sliderValueType, minValue, maxValue, startingValue, stepCount);
		AudioTrigger audioTrigger = gameObject.GetComponent<AudioTrigger>();
		if (audioTrigger != null)
		{
			componentInChildren.m_onPlayAudioTrigger = (UnityAction)Delegate.Combine(componentInChildren.m_onPlayAudioTrigger, (UnityAction)delegate
			{
				audioTrigger.TriggerAudio();
			});
		}
		SetLocalisation(gameObject, GetLabelString(label));
		RegisterSelectable(gameObject);
		return componentInChildren;
	}

	protected TMP_Dropdown AddDropdownOption(string label, List<string> options, int startingIndex, UnityAction<int> action, PlatformUtils.Platforms[] platforms = null)
	{
		if (!CheckForPlatform(platforms))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_dropdownPrefab, m_optionsListContainer);
		TMP_Dropdown componentInChildren = gameObject.GetComponentInChildren<TMP_Dropdown>();
		componentInChildren.ClearOptions();
		componentInChildren.AddOptions(options);
		componentInChildren.value = startingIndex;
		componentInChildren.onValueChanged.AddListener(action);
		AudioTrigger audioTrigger = gameObject.GetComponent<AudioTrigger>();
		if (audioTrigger != null)
		{
			componentInChildren.onValueChanged.AddListener(delegate
			{
				audioTrigger.TriggerAudio();
			});
		}
		SetLocalisation(gameObject, GetLabelString(label));
		RegisterSelectable(gameObject);
		return componentInChildren;
	}

	private CycleButton AddCycleButton(string label, UnityAction<int> action, PlatformUtils.Platforms[] platforms = null)
	{
		if (!CheckForPlatform(platforms))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_cyclePrefab, m_optionsListContainer);
		CycleButton componentInChildren = gameObject.GetComponentInChildren<CycleButton>();
		componentInChildren.ClearOptions();
		componentInChildren.m_onValueChanged = (UnityAction<int>)Delegate.Combine(componentInChildren.m_onValueChanged, action);
		AudioTrigger audioTrigger = gameObject.GetComponent<AudioTrigger>();
		if (audioTrigger != null)
		{
			componentInChildren.m_onValueChanged = (UnityAction<int>)Delegate.Combine(componentInChildren.m_onValueChanged, (UnityAction<int>)delegate
			{
				audioTrigger.TriggerAudio();
			});
		}
		SetLocalisation(gameObject, GetLabelString(label));
		RegisterSelectable(gameObject);
		return componentInChildren;
	}

	protected CycleButton AddCycleButtonOption(string label, List<string> options, int startingIndex, UnityAction<int> action, PlatformUtils.Platforms[] platforms = null)
	{
		CycleButton cycleButton = AddCycleButton(label, action, platforms);
		if (cycleButton != null)
		{
			cycleButton.AddOptions(options);
			cycleButton.Value = startingIndex;
		}
		return cycleButton;
	}

	protected CycleButton AddCycleButtonOption(string label, List<LocalizedString> options, int startingIndex, UnityAction<int> action, PlatformUtils.Platforms[] platforms = null)
	{
		CycleButton cycleButton = AddCycleButton(label, action, platforms);
		if (cycleButton != null)
		{
			cycleButton.AddOptions(options);
			cycleButton.Value = startingIndex;
		}
		return cycleButton;
	}

	protected void AddGroupHeaderOption(string label, PlatformUtils.Platforms[] platforms = null)
	{
		if (CheckForPlatform(platforms))
		{
			GameObject newElement = UnityEngine.Object.Instantiate(m_groupHeaderPrefab, m_optionsListContainer);
			SetLocalisation(newElement, GetLabelString(label));
		}
	}

	protected void AddApplyButton()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_applyButtonPrefab, m_optionsListContainer);
		Button componentInChildren = gameObject.GetComponentInChildren<Button>();
		componentInChildren.onClick.AddListener(OnApplyChanges);
		RegisterSelectable(gameObject);
		AudioTrigger audioTrigger = gameObject.GetComponent<AudioTrigger>();
		if (audioTrigger != null)
		{
			componentInChildren.onClick.AddListener(delegate
			{
				audioTrigger.TriggerAudio();
			});
		}
	}

	private void SetLocalisation(GameObject newElement, LocalizedString localizedString)
	{
		newElement.GetComponentInChildren<LocalizeStringEvent>().StringReference = localizedString;
	}

	private void RegisterSelectable(GameObject newElement)
	{
		if (m_selectGameObject == null)
		{
			Selectable componentInChildren = newElement.GetComponentInChildren<Selectable>();
			if (componentInChildren != null)
			{
				m_selectGameObject = componentInChildren.gameObject;
			}
		}
	}

	public bool CanExitPage()
	{
		TMP_Dropdown[] componentsInChildren = GetComponentsInChildren<TMP_Dropdown>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].IsExpanded)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void OnDisable()
	{
		OnApplyChanges();
	}

	protected virtual void OnApplyChanges()
	{
	}
}
