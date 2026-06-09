using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Artifact Effect Definition", menuName = "Items/Artifact Effect Definition")]
public class ArtifactEffectDefinition : ScriptableObject
{
	public enum EffectValueType
	{
		None,
		Float,
		Integer
	}

	public enum EffectStackBehavior
	{
		TakeHighest,
		Additive
	}

	public enum EffectOutcomeType
	{
		Positive,
		Negative
	}

	[SerializeField]
	private LocalizedString m_displayString;

	[SerializeField]
	private EffectValueType m_valueType;

	[SerializeField]
	private EffectStackBehavior m_stackBehavior;

	[SerializeField]
	private EffectOutcomeType m_outcomeType;

	public LocalizedString DisplayString => m_displayString;

	public EffectValueType ValueType => m_valueType;

	public EffectStackBehavior StackBehavior => m_stackBehavior;

	public EffectOutcomeType OutcomeType => m_outcomeType;
}
