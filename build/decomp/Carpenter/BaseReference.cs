using System;
using UnityEngine;

[Serializable]
public class BaseReference<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>
{
	[SerializeField]
	private bool m_useConstant = true;

	[SerializeField]
	private T m_constantValue;

	[SerializeField]
	private BaseVariable<T> m_variable;

	public T Value
	{
		get
		{
			if (!m_useConstant && !(m_variable == null))
			{
				return m_variable.Value;
			}
			return m_constantValue;
		}
	}

	public BaseReference()
	{
	}

	public BaseReference(T value)
	{
		m_useConstant = true;
		m_constantValue = value;
	}

	public static implicit operator T(BaseReference<T> reference)
	{
		return reference.Value;
	}
}
