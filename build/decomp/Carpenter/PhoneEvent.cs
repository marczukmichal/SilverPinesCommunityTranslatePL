using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
[CreateAssetMenu(menuName = "Audio/Phone Event")]
public class PhoneEvent : ScriptableObject
{
	[Serializable]
	public struct PhoneProgressionVariable
	{
		public ProgressionVariable m_variable;

		public bool m_requiredState;
	}

	[Serializable]
	public struct PhoneProgressionVariableInt
	{
		public ProgressionVariableInt m_variable;

		public int m_requiredValue;
	}

	public enum PhoneEventGameStateFlags
	{
		None,
		HealthCritical,
		Scripted
	}

	public LevelRegionSettings[] m_validLevelRegions;

	public PhoneProgressionVariable[] m_variables;

	public PhoneProgressionVariableInt[] m_intVariables;

	public int m_priority;

	[Tooltip("If true then if this event is allowed to fire it will be prioritised over all others, and in prevent any other event from playing, even if played multiple times.")]
	public bool m_isOverrideEvent;

	public Dialogue m_dialogue;

	public UnityEvent m_onCalledEvent;

	public bool m_onlyOnce;

	public PhoneFlags m_phoneFlags;

	public PhoneEventGameStateFlags m_gameStateFlags;

	public bool IsValid(LevelRegionSettings activePlayerRegion, PhoneFlags phoneFlags)
	{
		if (m_gameStateFlags.HasFlag(PhoneEventGameStateFlags.Scripted))
		{
			return false;
		}
		if (m_phoneFlags != 0 && m_phoneFlags != phoneFlags)
		{
			return false;
		}
		if (m_validLevelRegions != null && m_validLevelRegions.Length != 0)
		{
			bool flag = false;
			LevelRegionSettings[] validLevelRegions = m_validLevelRegions;
			for (int i = 0; i < validLevelRegions.Length; i++)
			{
				if (validLevelRegions[i] == activePlayerRegion)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		if (m_variables != null)
		{
			PhoneProgressionVariable[] variables = m_variables;
			for (int i = 0; i < variables.Length; i++)
			{
				PhoneProgressionVariable phoneProgressionVariable = variables[i];
				if (!(phoneProgressionVariable.m_variable == null) && phoneProgressionVariable.m_variable.Value != phoneProgressionVariable.m_requiredState)
				{
					return false;
				}
			}
		}
		if (m_intVariables != null)
		{
			PhoneProgressionVariableInt[] intVariables = m_intVariables;
			for (int i = 0; i < intVariables.Length; i++)
			{
				PhoneProgressionVariableInt phoneProgressionVariableInt = intVariables[i];
				if (!(phoneProgressionVariableInt.m_variable == null) && phoneProgressionVariableInt.m_variable.Value != phoneProgressionVariableInt.m_requiredValue)
				{
					return false;
				}
			}
		}
		if (m_gameStateFlags.HasFlag(PhoneEventGameStateFlags.HealthCritical))
		{
			bool flag2 = false;
			GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
			if (item != null)
			{
				CharacterHealth component = item.GetComponent<CharacterHealth>();
				if (component != null && component.HealthPercentage <= 0.3f)
				{
					flag2 = true;
				}
			}
			if (!flag2)
			{
				return false;
			}
		}
		return true;
	}
}
