using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Hints/Pause Hint Info")]
public class PauseHintInfo : ScriptableObject
{
	[SerializeField]
	private LocalizedString m_titleStringReference;

	[SerializeField]
	private LocalizedString m_hintStringReference;

	[SerializeField]
	private Sprite m_image;

	[SerializeField]
	private VideoClip m_videoClip;

	[SerializeField]
	private InputActionReference m_inputAction;

	[SerializeField]
	private LocalizedString m_inputLabelStringReference;

	[Header("Inline text inputs {input_0}, {input_1} etc.")]
	[SerializeField]
	private InputActionReference[] m_inlineTextInputActions;

	public string TitleText => m_titleStringReference.GetLocalizedString();

	public string HintText => m_hintStringReference.GetLocalizedString();

	public Sprite Image => m_image;

	public VideoClip VideoClip => m_videoClip;

	public InputActionReference InputAction => m_inputAction;

	public string InputLabel
	{
		get
		{
			if (m_inputLabelStringReference == null)
			{
				return "";
			}
			return m_inputLabelStringReference.GetLocalizedString();
		}
	}

	public InputActionReference[] InlineTextInputActions => m_inlineTextInputActions;
}
