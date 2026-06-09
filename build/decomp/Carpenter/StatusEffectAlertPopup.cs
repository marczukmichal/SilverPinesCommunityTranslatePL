using UnityEngine;

public class StatusEffectAlertPopup : MonoBehaviour
{
	[SerializeField]
	private StatusEffectAlertBarPopup[] m_bars;

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Register(StatusEffectUpdated);
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Register(DataReloaded);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.StatusEffects.PlayerStatusEffectUpdated.Unregister(StatusEffectUpdated);
		GlobalReferences.Instance.EventChannels.Datastore.PersistentDataRefresh.Unregister(DataReloaded);
	}

	private void Start()
	{
		StatusEffectAlertBarPopup[] bars = m_bars;
		for (int i = 0; i < bars.Length; i++)
		{
			bars[i].SetShown(shown: false);
		}
	}

	private void DataReloaded()
	{
		StatusEffectAlertBarPopup[] bars = m_bars;
		for (int i = 0; i < bars.Length; i++)
		{
			bars[i].SetShown(shown: false);
		}
	}

	private void StatusEffectUpdated(StatusEffectInstance effectInstance)
	{
		if (!effectInstance.Definition.ShowAlertPopup)
		{
			return;
		}
		StatusEffectAlertBarPopup statusEffectAlertBarPopup = null;
		StatusEffectAlertBarPopup[] bars = m_bars;
		foreach (StatusEffectAlertBarPopup statusEffectAlertBarPopup2 in bars)
		{
			if (statusEffectAlertBarPopup2.StatusEffectDefinition == effectInstance.Definition)
			{
				statusEffectAlertBarPopup = statusEffectAlertBarPopup2;
				break;
			}
		}
		if (statusEffectAlertBarPopup == null)
		{
			bars = m_bars;
			foreach (StatusEffectAlertBarPopup statusEffectAlertBarPopup3 in bars)
			{
				if (!statusEffectAlertBarPopup3.gameObject.activeSelf)
				{
					statusEffectAlertBarPopup = statusEffectAlertBarPopup3;
					statusEffectAlertBarPopup3.SetStatusEffectDefinition(effectInstance.Definition);
					break;
				}
			}
		}
		if (statusEffectAlertBarPopup != null)
		{
			statusEffectAlertBarPopup.SetShown(effectInstance.IsActive || effectInstance.Amount > 0f);
			statusEffectAlertBarPopup.SetValues(effectInstance.PercentageFilled, effectInstance.IsActive);
		}
		else
		{
			Debug.LogError("Tried to show too many status effects at once! Could not find a free StatusEffectAlertBarPopup!");
		}
	}
}
