using HutongGames.PlayMaker;

namespace Actions;

[ActionCategory("AI")]
public class SetAIVisionScalar : FsmStateAction
{
	public float m_scalar = 1f;

	public float m_distanceScalar = 1f;

	public bool m_ignoreDirection;

	private AIVision m_vision;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_vision = base.Owner.GetComponent<AIVision>();
		}
	}

	public override void OnEnter()
	{
		m_vision.ActiveDetectionScalar = m_scalar;
		m_vision.IgnoreLookDirection = m_ignoreDirection;
		m_vision.DetectionDistanceScalar = m_distanceScalar;
		Finish();
	}

	public override void OnExit()
	{
		m_vision.ActiveDetectionScalar = 1f;
		m_vision.IgnoreLookDirection = false;
		m_vision.DetectionDistanceScalar = 1f;
	}
}
