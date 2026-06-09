using UnityEngine;

[CreateAssetMenu(menuName = "Variables/String")]
public class StringVariable : BaseVariable<string>
{
	[Tooltip("Event to trigger if this value changes")]
	[SerializeField]
	private StringGameEventChannel _valueChangedEvent;

	public override void OnValueChanged(string newValue)
	{
		if ((bool)_valueChangedEvent)
		{
			_valueChangedEvent.Raise(newValue);
		}
	}
}
