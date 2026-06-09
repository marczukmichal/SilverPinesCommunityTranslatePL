using System;

[Serializable]
public class PersistentDataBool : PersistentDataBase<bool>
{
	public PersistentDataBool(string guid)
		: base(guid)
	{
	}
}
