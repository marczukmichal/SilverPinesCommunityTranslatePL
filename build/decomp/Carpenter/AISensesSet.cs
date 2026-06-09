using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Sets/Active AI Senses")]
public class AISensesSet : RuntimeSet<AISenses>
{
	public float GetHighetstDetectionPercent()
	{
		float num = 0f;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AISenses aISenses = (AISenses)enumerator.Current;
				num = Mathf.Max(num, aISenses.DetectionPercent);
			}
			return num;
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	public AISenses.TargetState GetHighestTargetState()
	{
		AISenses.TargetState targetState = AISenses.TargetState.None;
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AISenses.TargetState currentTargetState = ((AISenses)enumerator.Current).CurrentTargetState;
				if (currentTargetState == AISenses.TargetState.Detected && targetState != AISenses.TargetState.Detected)
				{
					targetState = currentTargetState;
				}
				else if (currentTargetState == AISenses.TargetState.Suspicious && targetState == AISenses.TargetState.None)
				{
					targetState = currentTargetState;
				}
			}
			return targetState;
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
