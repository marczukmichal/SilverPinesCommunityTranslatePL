using UnityEngine;
using UnityEngine.Localization;

public class InteractableCircuitSwitch : BaseInteractable, IObjectVisibilityListener
{
	[Header("Circuit")]
	[SerializeField]
	private int m_circuitIndex;

	[SerializeField]
	private SimpleCircuit m_simpleCircuit;

	[Header("Strings")]
	[SerializeField]
	protected LocalizedString m_interactStringReference = new LocalizedString("InteractPrompts", "Use");

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_switchOnAudio;

	[SerializeField]
	private AudioEvent m_switchOffAudio;

	[Header("Voiceline")]
	[SerializeField]
	private AudioVoicedEvent m_noPowerVoiceLine;

	private bool m_isVisible = true;

	private bool m_playedPowerOutVoiceLine;

	public override string InteractString => m_interactStringReference.GetLocalizedString();

	protected override void Start()
	{
		base.Start();
		m_playedPowerOutVoiceLine = false;
		ObjectVisibility.AddToGameObjectIfMissing(base.gameObject);
	}

	public override bool CanInteract(BaseInteractor interactor)
	{
		if (m_isVisible)
		{
			return base.CanInteract(interactor);
		}
		return false;
	}

	public override void Interact(BaseInteractor interactor)
	{
		CircuitManager item = GlobalReferences.Instance.Anchors.Gameplay.CircuitManagerAnchor.Item;
		bool flag = false;
		if (m_simpleCircuit != null)
		{
			if (m_simpleCircuit.GetState())
			{
				if ((bool)m_switchOffAudio)
				{
					m_switchOffAudio.Play(base.transform.position);
				}
			}
			else
			{
				if ((bool)m_switchOnAudio)
				{
					m_switchOnAudio.Play(base.transform.position);
				}
				flag = true;
			}
			m_simpleCircuit.ToggleState(m_circuitIndex);
		}
		else if (item != null)
		{
			if (item.GetState(m_circuitIndex))
			{
				if ((bool)m_switchOffAudio)
				{
					m_switchOffAudio.Play(base.transform.position);
				}
			}
			else
			{
				if ((bool)m_switchOnAudio)
				{
					m_switchOnAudio.Play(base.transform.position);
				}
				flag = true;
			}
			item.ToggleState(m_circuitIndex);
		}
		else
		{
			Debug.LogError(base.gameObject.name + " failed to find Circuit Controller");
		}
		if (flag && item != null && !item.IsScenePowered() && m_noPowerVoiceLine != null && !m_playedPowerOutVoiceLine)
		{
			m_playedPowerOutVoiceLine = true;
			GlobalReferences.Instance.EventChannels.Audio.PlayAudioVoiced.Raise(new PlayAudioVoicedEventData(m_noPowerVoiceLine, null, isPlayer: true));
		}
	}

	public void SetObjectVisibility(bool visible)
	{
		m_isVisible = visible;
	}
}
