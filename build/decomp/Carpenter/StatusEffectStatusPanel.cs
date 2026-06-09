using TMPro;
using UnityEngine;

public class StatusEffectStatusPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private StatusEffectsVariable m_statusEffectsVariable;

	[SerializeField]
	private GameObject m_container;

	public void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Register(StatusEffectUpdated);
		RefreshData();
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Unregister(StatusEffectUpdated);
	}

	private void StatusEffectUpdated(StatusEffectInstance arg0)
	{
		RefreshData();
	}

	private void RefreshData()
	{
		string text = "";
		bool flag = false;
		foreach (StatusEffectInstance item in m_statusEffectsVariable.Value)
		{
			if (item.Definition.ShowInHealthOverview && item.IsActive)
			{
				if (flag)
				{
					text += "\n";
				}
				text += item.Definition.ActiveName.GetLocalizedString();
				flag = true;
			}
		}
		m_text.text = text;
		m_container.SetActive(flag);
	}
}
