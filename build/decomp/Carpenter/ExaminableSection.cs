using System;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class ExaminableSection
{
	[Multiline(4)]
	public string m_text;

	public LocalizedString m_textStringReference;

	public Sprite m_image;
}
