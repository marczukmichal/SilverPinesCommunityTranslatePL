using UnityEngine;

[DisallowMultipleComponent]
[ShowInDesignerInspector]
public class KeypadMinigameData : MonoBehaviour
{
	[SerializeField]
	private bool m_useRandomCode;

	[SerializeField]
	private DeterministicCodeGenerator.Settings m_randomCodeSettings;

	[SerializeField]
	private string m_code;

	public string Code
	{
		get
		{
			if (m_useRandomCode)
			{
				return DeterministicCodeGenerator.GetNumbersCode(m_randomCodeSettings);
			}
			return m_code;
		}
	}
}
