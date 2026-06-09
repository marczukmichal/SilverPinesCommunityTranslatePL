using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PhotoViewfinderUI : MonoBehaviour
{
	[SerializeField]
	private GameObject m_container;

	[SerializeField]
	private TakingPhotoStateData m_photoStateData;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private RectTransform m_parentCanvasTransform;

	[SerializeField]
	private TextMeshProUGUI m_zoomText;

	[SerializeField]
	private GameObject m_noFilmOverlay;

	[SerializeField]
	private GameObject m_tooManyPhotos;

	[SerializeField]
	private GameObject m_buttonPrompts;

	[SerializeField]
	private Color m_filmAmountValidColor;

	[SerializeField]
	private Color m_filmAmountInvalidColor;

	private void OnEnable()
	{
		m_tooManyPhotos.gameObject.SetActive(value: false);
		m_container.SetActive(value: false);
		m_buttonPrompts.SetActive(value: false);
		GlobalReferences.Instance.EventChannels.Photos.TooManyPhotosMemory.Register(OnTooManyPhotosInMemoryEvent);
		TakingPhotoStateData photoStateData = m_photoStateData;
		photoStateData.OnSetActive = (UnityAction<bool>)Delegate.Combine(photoStateData.OnSetActive, new UnityAction<bool>(SetActive));
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Photos.TooManyPhotosMemory.Unregister(OnTooManyPhotosInMemoryEvent);
		TakingPhotoStateData photoStateData = m_photoStateData;
		photoStateData.OnSetActive = (UnityAction<bool>)Delegate.Remove(photoStateData.OnSetActive, new UnityAction<bool>(SetActive));
	}

	private void SetActive(bool active)
	{
		m_container.SetActive(active);
		m_buttonPrompts.SetActive(active);
		if (active)
		{
			UpdateSizeAndPosition();
		}
	}

	private void UpdateSizeAndPosition()
	{
		Vector2 offset = m_photoStateData.m_offset;
		offset.x /= m_parentCanvasTransform.lossyScale.x;
		offset.y /= m_parentCanvasTransform.lossyScale.y;
		m_rectTransform.anchoredPosition = offset;
		Vector2 sizeDelta = new Vector2((float)Screen.height * m_photoStateData.m_size, (float)Screen.height * m_photoStateData.m_size);
		sizeDelta.x /= m_parentCanvasTransform.lossyScale.x;
		sizeDelta.y /= m_parentCanvasTransform.lossyScale.y;
		m_rectTransform.sizeDelta = sizeDelta;
	}

	private void Update()
	{
		if (m_photoStateData.IsActive)
		{
			UpdateSizeAndPosition();
			m_zoomText.text = m_photoStateData.m_zoomNormalized.ToString("0.0") + "x";
		}
	}

	private void OnTooManyPhotosInMemoryEvent()
	{
		StopAllCoroutines();
		StartCoroutine(TooManyPhotosCoroutine());
	}

	private IEnumerator TooManyPhotosCoroutine()
	{
		m_tooManyPhotos.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(5f);
		m_tooManyPhotos.gameObject.SetActive(value: false);
	}
}
