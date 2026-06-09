using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Interactor")]
public class GetInteractablePosition : FsmStateAction
{
	private CharacterInteractor m_interactor;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Vector3 variable to set.")]
	public FsmVector3 m_vector3Variable;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_interactor = base.Owner.GetComponent<CharacterInteractor>();
		}
	}

	public override void OnEnter()
	{
		if (m_interactor.ActiveInteractable != null)
		{
			m_vector3Variable.Value = m_interactor.ActiveInteractable.transform.position;
		}
		Finish();
	}
}
