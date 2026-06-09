using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Detectables")]
public class SetNoise : FsmStateAction
{
	[SerializeField]
	public float m_noiseDistance;

	[SerializeField]
	public float m_noiseIntensity;

	[SerializeField]
	public bool m_resetOnExit;

	private NoiseSource m_noiseSource;

	public override void Awake()
	{
		if (!(base.Owner == null))
		{
			m_noiseSource = base.Owner.GetComponent<NoiseSource>();
		}
	}

	public override void OnEnter()
	{
		m_noiseSource.SetBaseNoiseValues(m_noiseDistance, m_noiseIntensity);
		Finish();
	}

	public override void OnExit()
	{
		base.OnExit();
		if (m_resetOnExit)
		{
			m_noiseSource.ResetToDefault();
		}
	}
}
