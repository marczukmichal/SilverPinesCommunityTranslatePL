using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private GameObject m_toggleContainer;

	[SerializeField]
	private Image m_leftSpeakerImage;

	[SerializeField]
	private Image m_rightSpeakerImage;

	[SerializeField]
	private GameObject m_leftSpeakerContainer;

	[SerializeField]
	private GameObject m_rightSpeakerContainer;

	[SerializeField]
	private float m_speakerLeftTextXAnchorPosition;

	[SerializeField]
	private CanvasGroup m_leftSpeakerCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightSpeakerCanvasGroup;

	[SerializeField]
	private float m_speakerRightTextXAnchorPosition;

	[SerializeField]
	private TextMeshProUGUI m_leftSpeakerName;

	[SerializeField]
	private TextMeshProUGUI m_rightSpeakerName;

	[SerializeField]
	private TextMeshProUGUI m_subtitlesText;

	[SerializeField]
	private Color m_fadedSpeakerColor;

	[SerializeField]
	private GameObject m_morePrompt;

	[Header("Animation")]
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private float m_defaultAnimTime = 0.04f;

	[SerializeField]
	private float m_speakerColorFadeTime = 0.1f;

	private bool m_doneAnimating;

	private Coroutine m_textRevealCoroutine;

	private DialogueSpeakerSide m_activeSpeakerSide;

	private bool m_isShown;

	private void OnEnable()
	{
		m_toggleContainer.gameObject.SetActive(value: false);
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Register(OnDialogueEvent);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Dialogue.Dialogue.Unregister(OnDialogueEvent);
		if (GameInputManager.GameInputActions != null)
		{
			GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressPressed;
			GameInputManager.GameInputActions.Game.Skip.performed -= SkipPressed;
		}
	}

	private void OnDialogueEvent(DialogueEventData dialogueEvent)
	{
		DialogueSpeakerSide dialogueSpeakerSide = DialogueSpeakerSide.Left;
		if (dialogueEvent.m_dialogue != null && dialogueEvent.m_dialogueLine != null)
		{
			dialogueSpeakerSide = dialogueEvent.m_dialogue.GetSpeakerSide(dialogueEvent.m_dialogueLine);
		}
		if (dialogueEvent.m_show != m_isShown)
		{
			if (dialogueEvent.m_show)
			{
				Show();
				SetSpeakerSide(dialogueSpeakerSide, force: true);
			}
			else
			{
				Hide();
			}
		}
		if (dialogueEvent.m_show)
		{
			ShowLine(dialogueEvent.m_dialogueLine, dialogueSpeakerSide, dialogueEvent.m_instant);
		}
	}

	private void ShowLine(DialogueLine line, DialogueSpeakerSide speakerSide, bool instant)
	{
		SetSpeakerSide(speakerSide, force: false);
		GameObject gameObject = ((speakerSide == DialogueSpeakerSide.Right) ? m_rightSpeakerContainer : m_leftSpeakerContainer);
		TextMeshProUGUI obj = ((speakerSide == DialogueSpeakerSide.Right) ? m_rightSpeakerName : m_leftSpeakerName);
		Image image = ((speakerSide == DialogueSpeakerSide.Right) ? m_rightSpeakerImage : m_leftSpeakerImage);
		obj.color = ((line.SpeakerSettings != null) ? line.SpeakerSettings.SubtitlesColor : Color.white);
		obj.text = ((line.SpeakerSettings != null) ? line.SpeakerSettings.CharacterName : "");
		m_subtitlesText.rectTransform.anchoredPosition = new Vector2((speakerSide == DialogueSpeakerSide.Left) ? m_speakerLeftTextXAnchorPosition : m_speakerRightTextXAnchorPosition, m_subtitlesText.rectTransform.anchoredPosition.y);
		m_morePrompt.SetActive(value: false);
		if (!instant)
		{
			AnimateTextOut();
		}
		else
		{
			m_subtitlesText.maxVisibleCharacters = m_subtitlesText.text.Length;
			m_doneAnimating = true;
			m_morePrompt.SetActive(value: true);
		}
		m_subtitlesText.text = line.Subtitles;
		if (line.SpeakerSettings != null)
		{
			gameObject.SetActive(value: true);
			image.sprite = line.SpeakerSettings.GetPortraitSprite(line.AlternativeSpriteID);
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}

	private void AnimateTextOut()
	{
		if (m_textRevealCoroutine != null)
		{
			StopCoroutine(m_textRevealCoroutine);
		}
		m_doneAnimating = false;
		m_textRevealCoroutine = StartCoroutine(DialogueTextRevealCoroutine());
	}

	private void SetSpeakerSide(DialogueSpeakerSide side, bool force)
	{
		if (m_activeSpeakerSide != side || force)
		{
			m_activeSpeakerSide = side;
			switch (side)
			{
			case DialogueSpeakerSide.Left:
				m_leftSpeakerCanvasGroup.DOFade(1f, force ? 0f : m_speakerColorFadeTime);
				m_rightSpeakerCanvasGroup.DOFade(0f, force ? 0f : m_speakerColorFadeTime);
				m_rightSpeakerName.gameObject.SetActive(value: false);
				break;
			case DialogueSpeakerSide.Right:
				m_rightSpeakerCanvasGroup.DOFade(1f, force ? 0f : m_speakerColorFadeTime);
				m_leftSpeakerCanvasGroup.DOFade(0f, force ? 0f : m_speakerColorFadeTime);
				m_leftSpeakerName.gameObject.SetActive(value: false);
				break;
			}
		}
	}

	private void Show()
	{
		m_isShown = true;
		m_toggleContainer.gameObject.SetActive(value: true);
		m_leftSpeakerContainer.SetActive(value: false);
		m_rightSpeakerContainer.SetActive(value: false);
		m_leftSpeakerName.gameObject.SetActive(value: false);
		m_rightSpeakerName.gameObject.SetActive(value: false);
		GameInputManager.GameInputActions.Game.Proceed.performed += ProgressPressed;
		DOTween.Kill(m_canvasGroup);
		m_canvasGroup.alpha = 0f;
		m_canvasGroup.DOFade(1f, 0.2f);
	}

	private void Hide()
	{
		m_isShown = false;
		DOTween.Kill(m_canvasGroup);
		m_canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			m_toggleContainer.gameObject.SetActive(value: false);
		});
		GameInputManager.GameInputActions.Game.Proceed.performed -= ProgressPressed;
	}

	private void ProgressPressed(InputAction.CallbackContext inputCallback)
	{
		if (!m_doneAnimating)
		{
			m_subtitlesText.maxVisibleCharacters = m_subtitlesText.text.Length;
		}
		else
		{
			GlobalReferences.Instance.EventChannels.Dialogue.ProgressDialogue.Raise();
		}
	}

	private void SkipPressed(InputAction.CallbackContext inputCabllback)
	{
		GlobalReferences.Instance.EventChannels.Dialogue.SkipDialogue.Raise();
	}

	private IEnumerator DialogueTextRevealCoroutine()
	{
		m_subtitlesText.maxVisibleCharacters = 0;
		while (m_subtitlesText.maxVisibleCharacters < m_subtitlesText.text.Length)
		{
			m_subtitlesText.maxVisibleCharacters++;
			_ = m_subtitlesText.text[m_subtitlesText.maxVisibleCharacters - 1];
			float defaultAnimTime = m_defaultAnimTime;
			yield return new WaitForSecondsRealtime(defaultAnimTime);
		}
		m_doneAnimating = true;
		m_morePrompt.SetActive(value: true);
		StopTalking();
	}

	private void StopTalking()
	{
	}
}
