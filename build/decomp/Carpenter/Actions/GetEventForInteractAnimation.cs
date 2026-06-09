using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class GetEventForInteractAnimation : FsmStateAction
{
	[RequiredField]
	public FsmString m_stringVariable;

	private CharacterInteractor m_interactor;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnEnter()
	{
		string fSMEventForInteractAnimation = m_interactor.GetFSMEventForInteractAnimation();
		m_stringVariable.Value = "Animation/" + fSMEventForInteractAnimation;
		Finish();
	}
}
