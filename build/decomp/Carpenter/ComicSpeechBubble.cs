using System;
using System.Collections.Generic;
using Shapes2D;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
public class ComicSpeechBubble : MonoBehaviour
{
	public enum TailSide
	{
		Bottom,
		Top,
		Left,
		Right
	}

	private struct CornersIndicesPair
	{
		public int m_startIndex;

		public int m_endIndex;
	}

	[SerializeField]
	private RectTransform m_rectTransform;

	[Header("Shape")]
	[SerializeField]
	private Shape m_shape;

	[SerializeField]
	private float m_cornerRadius;

	[SerializeField]
	private int m_cornerSegments = 5;

	[SerializeField]
	private bool m_useShapeTail;

	[SerializeField]
	private bool m_useAutomaticTailStartPosition;

	[SerializeField]
	private Vector2 m_shapeTailStartPosition;

	[SerializeField]
	private Vector2 m_shapeTailTargetPosition;

	[SerializeField]
	private float m_tailStartWidth = 10f;

	[Header("Speaker")]
	[SerializeField]
	private CharacterSpeakerSettings m_speakerSettings;

	[SerializeField]
	private Vector2 m_speakerOffset;

	[Header("Tail")]
	[SerializeField]
	private RectTransform m_tailTransform;

	[SerializeField]
	private RectTransform m_tailOverlapTransform;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI m_textObject;

	[Multiline(6)]
	[SerializeField]
	private string m_bubbleText;

	public bool UseShapeTail => m_useShapeTail;

	public bool UseAutomaticTailStartPosition => m_useAutomaticTailStartPosition;

	public Vector2 ShapeTailStartPosition
	{
		get
		{
			return m_shapeTailStartPosition;
		}
		set
		{
			m_shapeTailStartPosition = value;
		}
	}

	public Vector2 ShapeTailTargetPosition
	{
		get
		{
			return m_shapeTailTargetPosition;
		}
		set
		{
			m_shapeTailTargetPosition = value;
		}
	}

	public CharacterSpeakerSettings SpeakerSettings => m_speakerSettings;

	public bool HasTail => m_tailTransform != null;

	public Vector2 SpeakerOffset
	{
		get
		{
			return m_speakerOffset;
		}
		set
		{
			m_speakerOffset = value;
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			UpdateShape();
		}
	}

	public void OnValidate()
	{
		if (m_textObject != null)
		{
			if (!string.IsNullOrEmpty(m_bubbleText))
			{
				m_textObject.text = m_bubbleText;
			}
			m_textObject.gameObject.hideFlags = HideFlags.None;
		}
		UpdateShape();
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			if (m_tailTransform != null && m_tailOverlapTransform != null)
			{
				UpdateTailPosition();
			}
			UpdateShape();
		}
	}

	private void UpdateTailPosition()
	{
		Vector2 normalized = m_speakerOffset.normalized;
		m_tailTransform.anchoredPosition = Vector2.zero;
		m_tailOverlapTransform.anchoredPosition = Vector2.zero;
		float z = Vector2.SignedAngle(Vector2.up, normalized.normalized);
		RectTransform tailOverlapTransform = m_tailOverlapTransform;
		RectTransform tailOverlapTransform2 = m_tailOverlapTransform;
		RectTransform tailTransform = m_tailTransform;
		Vector2 vector2 = (m_tailTransform.anchorMax = new Vector2(Mathf.InverseLerp(-1f, 1f, normalized.x), Mathf.InverseLerp(-1f, 1f, normalized.y)));
		Vector2 vector4 = (tailTransform.anchorMin = vector2);
		Vector2 vector7 = (tailOverlapTransform.anchorMin = (tailOverlapTransform2.anchorMax = vector4));
		m_tailTransform.rotation = Quaternion.Euler(0f, 0f, z);
		m_tailOverlapTransform.rotation = Quaternion.Euler(0f, 0f, z);
	}

	private void UpdateShape()
	{
		if (m_shape == null)
		{
			return;
		}
		RectTransform component = GetComponent<RectTransform>();
		Vector2 min = component.rect.min;
		Vector2 max = component.rect.max;
		Vector2[] array = new Vector2[4]
		{
			new Vector2(min.x, min.y),
			new Vector2(max.x, min.y),
			new Vector2(max.x, max.y),
			new Vector2(min.x, max.y)
		};
		Vector2[] array2 = new Vector2[4]
		{
			new Vector2(m_cornerRadius, m_cornerRadius),
			new Vector2(0f - m_cornerRadius, m_cornerRadius),
			new Vector2(0f - m_cornerRadius, 0f - m_cornerRadius),
			new Vector2(m_cornerRadius, 0f - m_cornerRadius)
		};
		float[] array3 = new float[4] { 180f, 270f, 0f, 90f };
		List<Vector3> list = new List<Vector3>();
		new List<CornersIndicesPair>();
		bool flag = m_useShapeTail;
		TailSide tailSide = TailSide.Bottom;
		Vector2 vector = (m_useAutomaticTailStartPosition ? m_shapeTailTargetPosition : m_shapeTailStartPosition);
		if (vector.x > max.x)
		{
			tailSide = TailSide.Right;
		}
		else if (vector.x < min.x)
		{
			tailSide = TailSide.Left;
		}
		else if (vector.y > max.y)
		{
			tailSide = TailSide.Top;
		}
		else if (vector.y < min.y)
		{
			tailSide = TailSide.Bottom;
		}
		else
		{
			flag = false;
		}
		Rect rect = component.rect;
		rect.size = new Vector2(rect.size.x - (m_cornerRadius + m_tailStartWidth * 0.5f) * 2f, rect.size.y - (m_cornerRadius + m_tailStartWidth * 0.5f) * 2f);
		rect.center = component.rect.center;
		Vector2 min2 = rect.min;
		Vector2 max2 = rect.max;
		Vector2 vector2;
		if (m_useAutomaticTailStartPosition)
		{
			Vector2 shapeTailTargetPosition = m_shapeTailTargetPosition;
			if (tailSide == TailSide.Top || tailSide == TailSide.Bottom)
			{
				shapeTailTargetPosition.x = Mathf.Lerp(component.rect.center.x, shapeTailTargetPosition.x, 0.5f);
			}
			else
			{
				shapeTailTargetPosition.y = Mathf.Lerp(component.rect.center.y, shapeTailTargetPosition.y, 0.5f);
			}
			vector2 = new Vector2(Mathf.Clamp(shapeTailTargetPosition.x, min2.x, max2.x), Mathf.Clamp(shapeTailTargetPosition.y, min2.y, max2.y));
		}
		else
		{
			vector2 = new Vector2(Mathf.Clamp(m_shapeTailStartPosition.x, min2.x, max2.x), Mathf.Clamp(m_shapeTailStartPosition.y, min2.y, max2.y));
		}
		for (int i = 0; i < 4; i++)
		{
			Vector2 vector3 = array[i] + array2[i];
			float num = array3[i];
			for (int j = 0; j <= m_cornerSegments; j++)
			{
				float num2 = num + 90f / (float)m_cornerSegments * (float)j;
				float f = MathF.PI / 180f * num2;
				list.Add(vector3 + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * m_cornerRadius);
			}
			if (!flag || ((i != 0 || tailSide != 0) && (i != 1 || tailSide != TailSide.Right) && (i != 2 || tailSide != TailSide.Top) && (i != 3 || tailSide != TailSide.Left)))
			{
				continue;
			}
			Vector2 vector4 = vector2;
			Vector2 vector5 = vector2;
			if (tailSide == TailSide.Top || tailSide == TailSide.Bottom)
			{
				vector4.y = (vector5.y = list[list.Count - 1].y);
				if (tailSide == TailSide.Top)
				{
					vector4.x += m_tailStartWidth * 0.5f;
					vector5.x -= m_tailStartWidth * 0.5f;
				}
				else
				{
					vector4.x -= m_tailStartWidth * 0.5f;
					vector5.x += m_tailStartWidth * 0.5f;
				}
			}
			else
			{
				vector4.x = (vector5.x = list[list.Count - 1].x);
				if (tailSide == TailSide.Left)
				{
					vector4.y += m_tailStartWidth * 0.5f;
					vector5.y -= m_tailStartWidth * 0.5f;
				}
				else
				{
					vector4.y -= m_tailStartWidth * 0.5f;
					vector5.y += m_tailStartWidth * 0.5f;
				}
			}
			list.Add(vector4);
			list.Add(m_shapeTailTargetPosition);
			list.Add(vector5);
		}
		List<Vector3> list2 = new List<Vector3>();
		foreach (Vector3 item in list)
		{
			list2.Add(base.transform.TransformPoint(item));
		}
		m_shape.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Max(0f, Mathf.Abs(m_shapeTailTargetPosition.x)), Mathf.Max(0f, Mathf.Abs(m_shapeTailTargetPosition.y)));
		m_shape.SetPolygonWorldVertices(list2.ToArray());
	}
}
