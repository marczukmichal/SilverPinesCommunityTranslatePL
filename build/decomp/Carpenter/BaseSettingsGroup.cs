using System;

[Serializable]
public abstract class BaseSettingsGroup : ICloneable
{
	protected bool m_isDirty;

	public bool IsDirty => m_isDirty;

	protected bool SetValue<T>(ref T variable, T value)
	{
		if (!variable.Equals(value))
		{
			variable = value;
			m_isDirty = true;
			return true;
		}
		return false;
	}

	public abstract void ApplySettings();

	public object Clone()
	{
		return MemberwiseClone();
	}

	public abstract void SetDefaults();
}
