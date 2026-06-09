using UnityEngine;
using UnityEngine.UI;

public class UIUserCanvasScaler : MonoBehaviour
{
	[SerializeField]
	private RectTransform[] m_16by9containers;

	[SerializeField]
	private bool m_onlyFixAspectRatio;

	private void OnEnable()
	{
		Apply();
		GlobalReferences.Instance.EventChannels.UserPreferences.UIScaleChanged.Register(Apply);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.UserPreferences.UIScaleChanged.Unregister(Apply);
	}

	private void OnRectTransformDimensionsChange()
	{
		Apply();
	}

	private void Apply()
	{
		UserPreferences userPreferences = GlobalReferences.Instance.UserPreferences;
		float num = (m_onlyFixAspectRatio ? 1f : userPreferences.DisplaySettings.UIScale);
		float x = 1920f / num;
		float y = 1080f / num;
		GetComponent<CanvasScaler>().referenceResolution = new Vector2(x, y);
		RectTransform component = GetComponent<RectTransform>();
		float num2 = (float)Screen.width / (float)Screen.height;
		float num3 = 1.7777778f;
		float height;
		float x2;
		if (num2 > num3)
		{
			height = component.rect.height;
			x2 = height * num3;
		}
		else
		{
			x2 = component.rect.width;
			height = component.rect.height;
		}
		if (m_16by9containers != null)
		{
			RectTransform[] _16by9containers = m_16by9containers;
			for (int i = 0; i < _16by9containers.Length; i++)
			{
				_16by9containers[i].sizeDelta = new Vector2(x2, height);
			}
		}
	}
}
