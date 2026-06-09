using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization.Components;

public class MenuInfoMessagePanel : MonoBehaviour
{
	[SerializeField]
	private LocalizeStringEvent m_localizeStringEvent;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private static WaitForSecondsRealtime m_fadeWaitTime = new WaitForSecondsRealtime(4f);

	private void OnEnable()
	{
		m_canvasGroup.alpha = 0f;
		GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Register(OnMessageEvent);
	}

	private void OnDisable()
	{
		DOTween.Kill(m_canvasGroup);
		StopAllCoroutines();
		GlobalReferences.Instance.EventChannels.InGameMenu.MenuInfoMessage.Unregister(OnMessageEvent);
	}

	private void OnMessageEvent(MenuInfoMessageData message)
	{
		m_localizeStringEvent.StringReference = message.m_stringReference;
		StopAllCoroutines();
		StartCoroutine(ShowFlow());
	}

	private IEnumerator ShowFlow()
	{
		yield return m_canvasGroup.DOFade(1f, 0.1f).WaitForCompletion();
		m_fadeWaitTime.Reset();
		yield return m_fadeWaitTime;
		yield return m_canvasGroup.DOFade(0f, 0.1f).WaitForCompletion();
	}
}
