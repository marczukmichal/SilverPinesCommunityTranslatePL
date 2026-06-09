using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("Special")]
public class FieldOfViewOverride : FsmStateAction
{
	public ObjectVisibilityLevelSettings m_overrideVisibilityCurveSettings;

	public bool m_clearOnExit;

	protected CharacterHealth m_characterHealth;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_characterHealth = base.Owner.GetComponent<CharacterHealth>();
		}
	}

	public override void OnEnter()
	{
		GlobalReferences.Instance.Anchors.FieldOfView.FieldOfViewLevelSettingsAnchor.Set(m_overrideVisibilityCurveSettings);
		Finish();
	}

	public override void OnExit()
	{
		if (m_clearOnExit)
		{
			GlobalReferences.Instance.Anchors.FieldOfView.FieldOfViewLevelSettingsAnchor.Set(null);
		}
	}
}
