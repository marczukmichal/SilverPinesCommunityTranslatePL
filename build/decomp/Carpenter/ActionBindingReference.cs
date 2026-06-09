using System;
using UnityEngine.InputSystem;

[Serializable]
public class ActionBindingReference
{
	public InputActionReference m_action;

	public string m_bindingId;

	public bool m_isLockedControls;
}
