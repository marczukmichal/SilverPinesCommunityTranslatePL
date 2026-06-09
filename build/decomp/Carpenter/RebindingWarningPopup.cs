using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class RebindingWarningPopup : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_actionText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	public void WarnForClearedBinding(LocalizedString controlLabel)
	{
		base.gameObject.SetActive(value: true);
		m_actionText.text = controlLabel.GetLocalizedString();
		DOTween.Kill(m_canvasGroup);
		StopAllCoroutines();
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		m_canvasGroup.alpha = 0f;
		yield return m_canvasGroup.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		yield return new WaitForSecondsRealtime(2f);
		yield return m_canvasGroup.DOFade(0f, 2f).SetUpdate(isIndependentUpdate: true).WaitForCompletion();
		base.gameObject.SetActive(value: false);
	}
}
