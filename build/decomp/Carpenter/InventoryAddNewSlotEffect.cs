using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryAddNewSlotEffect : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	private bool m_isAnimating;

	public void Animate()
	{
		base.gameObject.SetActive(value: true);
		m_isAnimating = true;
		StartCoroutine(AnimateNewInventorySlotCoroutine(m_image.gameObject));
	}

	private IEnumerator AnimateNewInventorySlotCoroutine(GameObject highlightObject)
	{
		for (int i = 0; i < 4; i++)
		{
			highlightObject.SetActive(value: true);
			yield return new WaitForSecondsRealtime(0.2f);
			highlightObject.SetActive(value: false);
			yield return new WaitForSecondsRealtime(0.1f);
		}
		Object.Destroy(highlightObject);
	}

	private void OnDisable()
	{
		if (m_isAnimating)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
