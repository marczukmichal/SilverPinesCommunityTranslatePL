using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Progression Variable (Integer)")]
public class ProgressionVariableInt : IntVariable
{
	public override void OnValueChanged(int newValue)
	{
		base.OnValueChanged(newValue);
		if (GameDebugCommands.PROG_VAR_LOG_CHANGES)
		{
			Debug.Log("<color=yellow>ProgressionVariable " + base.name + " is now " + newValue + "</color>");
		}
	}

	public void IncrementProgressionVariable()
	{
		base.Value++;
	}
}
