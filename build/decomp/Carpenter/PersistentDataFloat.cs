using System;

[Serializable]
public class PersistentDataFloat : PersistentDataBase<float>
{
	public PersistentDataFloat(string guid)
		: base(guid)
	{
	}
}
