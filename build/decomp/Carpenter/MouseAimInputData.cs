using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Misc/Mouse Aim Input Data")]
public class MouseAimInputData : ScriptableObject
{
	[NonSerialized]
	public Vector2 m_mouseAimOffset;

	private bool m_isAiming;

	public UnityAction<bool> OnStartAiming;

	public bool IsAiming
	{
		get
		{
			return m_isAiming;
		}
		set
		{
			if (m_isAiming != value)
			{
				m_isAiming = value;
				OnStartAiming?.Invoke(value);
			}
		}
	}
}
