using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CableStationBellInstance : MonoBehaviour
{
	[SerializeField]
	private bool m_hasBell;

	[SerializeField]
	private EventReference m_ringFmodEvent;

	[SerializeField]
	private EventReference m_noBellFmodEvent;

	[SerializeField]
	private string m_noteParameter;

	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private Button m_noBellButton;

	[SerializeField]
	private Button m_bellButton;

	public UnityAction<CableStationBellInstance> OnRing;

	private Vector3 m_startingScale;

	private void Start()
	{
		m_startingScale = base.transform.localScale;
		UpdateVisuals();
	}

	public void SetHasBell()
	{
		m_hasBell = true;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		m_visuals.SetActive(m_hasBell);
		Image component = m_noBellButton.GetComponent<Image>();
		bool raycastTarget = (m_noBellButton.enabled = !m_hasBell);
		component.raycastTarget = raycastTarget;
	}

	public void DisableInput()
	{
		m_bellButton.enabled = false;
	}

	public bool Ring()
	{
		if (m_hasBell)
		{
			EventInstance eventInstance = RuntimeManager.CreateInstance(m_ringFmodEvent);
			eventInstance.setParameterByNameWithLabel("BellsNoteController", m_noteParameter);
			eventInstance.start();
			DOTween.Kill(base.transform);
			base.transform.localScale = m_startingScale;
			base.transform.DOScale(base.transform.localScale * 1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
			OnRing(this);
		}
		else
		{
			RuntimeManager.CreateInstance(m_noBellFmodEvent).start();
		}
		return m_hasBell;
	}

	public void TryManualRing()
	{
		if (m_hasBell)
		{
			Ring();
		}
	}
}
