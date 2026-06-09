using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlaytestNotice : MonoBehaviour
{
	[SerializeField]
	private string m_fileToReadFrom;

	[SerializeField]
	private TextMeshProUGUI m_text;

	private void PopulateFromFile()
	{
		m_text.text = File.ReadAllText(Application.streamingAssetsPath + "/" + m_fileToReadFrom);
	}

	public IEnumerator ShowAnimation()
	{
		PopulateFromFile();
		base.gameObject.SetActive(value: true);
		yield return m_text.DOFade(1f, 0.5f).WaitForCompletion();
		yield return new WaitForSeconds(15f);
		yield return m_text.DOFade(0f, 2f).WaitForCompletion();
		base.gameObject.SetActive(value: false);
	}
}
