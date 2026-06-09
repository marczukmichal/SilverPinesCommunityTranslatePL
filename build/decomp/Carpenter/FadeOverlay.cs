using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeOverlay : MonoBehaviour
{
	[SerializeField]
	private BoolGameEventChannel m_fadeScreenEvent;

	[SerializeField]
	private Image m_overlayImage;

	[SerializeField]
	private Color m_redDoorColor;

	public static readonly float BaseFadeTime = 0.5f;

	private FadeType m_previousFadeType;

	private float GetFadeTime(FadeType fadeType)
	{
		switch (fadeType)
		{
		case FadeType.RedDoor:
			return 1f;
		case FadeType.FadeEndFast:
			return 0.25f;
		case FadeType.FadeEndSlow:
			return 1f;
		case FadeType.BlackFadeVerySlow:
			return 3f;
		case FadeType.Minigame:
			return 0.25f;
		case FadeType.BlackInstant:
		case FadeType.RedInstant:
			return 0f;
		default:
			return BaseFadeTime;
		}
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.EventChannels.Generic.FadeScreen.Register(OnFadeEvent);
		GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Register(OnFadeEventTyped);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Generic.FadeScreen.Unregister(OnFadeEvent);
		GlobalReferences.Instance.EventChannels.Generic.ScreenFadeOfType.Unregister(OnFadeEventTyped);
	}

	private void Reset()
	{
		m_overlayImage = GetComponent<Image>();
	}

	private void OnFadeEvent(bool fadeScreen)
	{
		OnFadeEventTyped(fadeScreen ? FadeType.Black : FadeType.None);
	}

	private void OnFadeEventTyped(FadeType fadeType)
	{
		switch (fadeType)
		{
		case FadeType.None:
		{
			FadeType fadeType2 = m_previousFadeType;
			switch (fadeType2)
			{
			case FadeType.RedInstant:
				fadeType2 = FadeType.RedDoor;
				break;
			case FadeType.BlackInstant:
			case FadeType.BlackFadeVerySlow:
				fadeType2 = FadeType.Black;
				break;
			}
			m_overlayImage.DOFade(0f, GetFadeTime(fadeType2)).SetUpdate(isIndependentUpdate: true);
			break;
		}
		case FadeType.FadeEndFast:
		case FadeType.FadeEndSlow:
			m_overlayImage.DOFade(0f, GetFadeTime(fadeType)).SetUpdate(isIndependentUpdate: true);
			break;
		default:
			SetFadeColor(fadeType);
			m_overlayImage.DOFade(1f, GetFadeTime(fadeType)).SetUpdate(isIndependentUpdate: true);
			m_previousFadeType = fadeType;
			break;
		}
	}

	private void SetFadeColor(FadeType fade)
	{
		Color color;
		switch (fade)
		{
		case FadeType.RedDoor:
		case FadeType.RedInstant:
			color = m_redDoorColor;
			break;
		case FadeType.White:
			color = Color.white;
			break;
		default:
			color = Color.black;
			break;
		}
		color.a = m_overlayImage.color.a;
		m_overlayImage.color = color;
	}
}
