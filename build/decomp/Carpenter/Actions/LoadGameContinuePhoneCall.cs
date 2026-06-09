using System.Collections;
using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class LoadGameContinuePhoneCall : FsmStateAction
{
	public PhoneBook m_phoneBook;

	private PhoneInteraction m_phoneInteraction;

	public override void OnEnter()
	{
		base.OnEnter();
		StartCoroutine(PhoneInteractionCoroutine());
	}

	private IEnumerator PhoneInteractionCoroutine()
	{
		PhoneNumber saveNumber = m_phoneBook.GetSaveNumber();
		PhoneEvent savePhoneEvent = m_phoneBook.GetSavePhoneEvent(saveNumber);
		m_phoneInteraction = new PhoneInteraction(saveNumber, savePhoneEvent, m_phoneBook);
		yield return m_phoneInteraction.CallNumber(base.Owner.transform, ignoreSave: true);
		m_phoneInteraction.Cleanup();
		m_phoneInteraction = null;
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_phoneInteraction != null)
		{
			m_phoneInteraction.Cleanup();
			m_phoneInteraction = null;
		}
	}
}
