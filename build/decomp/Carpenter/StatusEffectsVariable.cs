using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Status Effects Variable")]
public class StatusEffectsVariable : ScriptableObject
{
	private List<StatusEffectInstance> m_value;

	public List<StatusEffectInstance> Value => m_value;

	public void SetValue(List<StatusEffectInstance> value)
	{
		m_value = value;
	}

	public void Reset()
	{
		if (m_value != null)
		{
			foreach (StatusEffectInstance item in m_value)
			{
				if (item.IsDefinitionLoaded())
				{
					item.Amount = 0f;
					GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Raise(item);
				}
			}
		}
		m_value = new List<StatusEffectInstance>();
	}
}
