using UnityEngine;

public class ComputerFile : ScriptableObject
{
	[SerializeField]
	public string m_fileName;

	protected virtual string Extension => ".dat";

	protected virtual int MemoryUse => 100;

	public string FileName => m_fileName + Extension;
}
