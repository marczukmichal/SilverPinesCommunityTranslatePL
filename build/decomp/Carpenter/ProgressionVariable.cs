using UnityEngine;

[CreateAssetMenu(menuName = "Variables/Progression Variable")]
public class ProgressionVariable : BoolVariable
{
	public override void OnValueChanged(bool newValue)
	{
		base.OnValueChanged(newValue);
		if (GameDebugCommands.PROG_VAR_LOG_CHANGES)
		{
			Debug.Log("<color=yellow>ProgressionVariable " + base.name + " is now " + newValue + "</color>");
		}
	}
}
