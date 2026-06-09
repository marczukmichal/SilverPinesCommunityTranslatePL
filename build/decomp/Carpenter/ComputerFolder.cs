using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComputerFolder
{
	[SerializeField]
	public string m_folderName;

	[SerializeField]
	private List<ComputerFile> m_files;

	public string FolderName => m_folderName;

	public List<ComputerFile> Files => m_files;
}
