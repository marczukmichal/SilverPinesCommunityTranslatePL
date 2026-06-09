using System;
using UnityEngine.Localization;

[Serializable]
public class BindableInputSettings
{
	public LocalizedString m_label;

	public ActionBindingReference m_keyboardAction;

	public ActionBindingReference m_keyboardAltAction;

	public ActionBindingReference m_gamepadAction;

	public bool m_allowDuplicateBinding;

	public string GetLocalizedControlName()
	{
		return m_label.GetLocalizedString();
	}
}
