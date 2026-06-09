using System;

[Serializable]
public class PersistentDataInt : PersistentDataBase<int>
{
	public PersistentDataInt(string guid)
		: base(guid)
	{
	}
}
