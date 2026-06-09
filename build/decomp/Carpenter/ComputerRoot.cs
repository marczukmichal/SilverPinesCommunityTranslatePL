using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComputerRoot : ComputerFolder
{
	[SerializeField]
	private List<ComputerFolder> m_subFolders;

	public List<ComputerFolder> SubFolders => m_subFolders;
}
