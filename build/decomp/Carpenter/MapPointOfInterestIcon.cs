using System;
using TMPro;
using UnityEngine;

public class MapPointOfInterestIcon : MapGeneralIcon
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private float m_radius = 10f;

	public override void Setup(LevelMapAOMMetadata.AppearsOnMapInstance aomInstance, MapArea3D.AreaType areaType)
	{
		base.Setup(aomInstance, areaType);
		if (!aomInstance.Settings.CustomText.IsEmpty)
		{
			m_text.text = aomInstance.Settings.CustomText.GetLocalizedString();
		}
		else
		{
			Debug.LogError("Missing custom text for Point of Interest " + aomInstance.Settings.m_mapIconGUID);
		}
		PositionTextOrbit(aomInstance.Settings.MapIconRotation);
	}

	private void PositionTextOrbit(float angle)
	{
		RectTransform rectTransform = m_text.rectTransform;
		if (angle >= 45f && angle <= 135f)
		{
			m_text.horizontalAlignment = HorizontalAlignmentOptions.Left;
			rectTransform.pivot = new Vector2(0f, 0.5f);
		}
		else if (angle >= 225f && angle <= 315f)
		{
			m_text.horizontalAlignment = HorizontalAlignmentOptions.Right;
			rectTransform.pivot = new Vector2(1f, 0.5f);
		}
		else
		{
			m_text.horizontalAlignment = HorizontalAlignmentOptions.Center;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}
		angle = Mathf.Clamp(angle, 0f, 360f);
		angle = 360f - angle;
		angle += 90f;
		angle %= 360f;
		float f = angle * (MathF.PI / 180f);
		Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * m_radius;
		rectTransform.localPosition = vector;
	}
}
