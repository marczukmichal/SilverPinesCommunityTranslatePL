using HutongGames.PlayMaker;
using UnityEngine;

namespace Actions;

[ActionCategory("Utilities")]
public class RemapFloat : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmFloat m_inValue;

	public FsmFloat m_inMinValue;

	public FsmFloat m_inMaxValue;

	[UIHint(UIHint.Variable)]
	public FsmFloat m_outValue;

	public FsmFloat m_outMinValue;

	public FsmFloat m_outMaxValue;

	public bool m_clamp;

	public override void OnEnter()
	{
		float value = m_inValue.Value;
		if (m_clamp)
		{
			value = Mathf.Clamp(value, m_inMinValue.Value, m_inMaxValue.Value);
		}
		m_outValue.Value = value.Remap(m_inMinValue.Value, m_inMaxValue.Value, m_outMinValue.Value, m_outMaxValue.Value);
		Finish();
	}
}
