using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GammaTestPanel : MonoBehaviour
{
	[SerializeField]
	private Image m_imageTemplate;

	[SerializeField]
	private int m_imageCount;

	[SerializeField]
	private Color m_darkestColor;

	[SerializeField]
	private Color m_brightestColor;

	[SerializeField]
	private RectTransform m_layoutGroupTransform;

	private List<Image> m_images;

	private void Awake()
	{
		m_images = new List<Image>();
		for (int i = 0; i < m_imageCount; i++)
		{
			Image image = Object.Instantiate(m_imageTemplate, m_layoutGroupTransform);
			m_images.Add(image);
			image.material = new Material(image.material);
			image.gameObject.SetActive(value: true);
		}
		m_imageTemplate.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		UpdatePostProcessing();
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Register(UpdatePostProcessing);
		GlobalReferences.Instance.EventChannels.UserPreferences.SetTemporaryGamma.Register(SetTemporaryGamma);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.GraphicsQualityChanged.Unregister(UpdatePostProcessing);
		GlobalReferences.Instance.EventChannels.UserPreferences.SetTemporaryGamma.Unregister(SetTemporaryGamma);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < m_images.Count; i++)
		{
			Image image = m_images[i];
			if (image != null)
			{
				Object.Destroy(image.material);
			}
		}
	}

	private void UpdatePostProcessing()
	{
		float gammaAdjustment = GlobalReferences.Instance.UserPreferences.DisplaySettings.GammaAdjustment;
		SetupImages(gammaAdjustment);
	}

	private void SetTemporaryGamma(float newGamma)
	{
		SetupImages(newGamma);
	}

	private void SetupImages(float gamma)
	{
		gamma += 1f;
		for (int i = 0; i < m_images.Count; i++)
		{
			Image image = m_images[i];
			if (image != null)
			{
				Color value = Color.Lerp(m_darkestColor, m_brightestColor, Mathf.Pow((float)i / (float)(m_images.Count - 1), 2f));
				image.material.SetColor("_BaseColor", value);
				image.material.SetFloat("_Gamma", gamma);
			}
		}
	}
}
