using System;
using UnityEngine;

[Serializable]
public class ArtifactEffectInstance
{
	[SerializeField]
	private ArtifactEffectDefinition m_effectDefinition;

	[SerializeField]
	private float m_float;

	[SerializeField]
	private int m_integer;

	public ArtifactEffectDefinition EffectDefinition => m_effectDefinition;

	public float FloatValue => m_float;

	public int IntegerValue => m_integer;

	public string EffectDescription => m_effectDefinition.DisplayString.GetLocalizedString(GetEffectAmountString());

	private string GetEffectAmountString()
	{
		return m_effectDefinition.ValueType switch
		{
			ArtifactEffectDefinition.EffectValueType.Float => m_float * 100f + "%", 
			ArtifactEffectDefinition.EffectValueType.Integer => m_integer.ToString(), 
			_ => "", 
		};
	}
}
