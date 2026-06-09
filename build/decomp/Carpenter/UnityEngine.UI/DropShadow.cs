using System.Collections.Generic;

namespace UnityEngine.UI;

[AddComponentMenu("UI/Effects/DropShadow", 14)]
public class DropShadow : BaseMeshEffect
{
	[SerializeField]
	private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

	[SerializeField]
	private Vector2 shadowDistance = new Vector2(1f, -1f);

	[SerializeField]
	private bool m_UseGraphicAlpha = true;

	public int iterations = 5;

	public Vector2 shadowSpread = Vector2.one;

	public Color effectColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			shadowColor = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public Vector2 ShadowSpread
	{
		get
		{
			return shadowSpread;
		}
		set
		{
			shadowSpread = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public int Iterations
	{
		get
		{
			return iterations;
		}
		set
		{
			iterations = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public Vector2 EffectDistance
	{
		get
		{
			return shadowDistance;
		}
		set
		{
			shadowDistance = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	public bool useGraphicAlpha
	{
		get
		{
			return m_UseGraphicAlpha;
		}
		set
		{
			m_UseGraphicAlpha = value;
			if (base.graphic != null)
			{
				base.graphic.SetVerticesDirty();
			}
		}
	}

	protected DropShadow()
	{
	}

	private void DropShadowEffect(List<UIVertex> verts)
	{
		int count = verts.Count;
		List<UIVertex> list = new List<UIVertex>(verts);
		verts.Clear();
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < count; j++)
			{
				UIVertex item = list[j];
				Vector3 position = item.position;
				float num = (float)i / (float)iterations;
				position.x *= 1f + shadowSpread.x * num * 0.01f;
				position.y *= 1f + shadowSpread.y * num * 0.01f;
				position.x += shadowDistance.x * num;
				position.y += shadowDistance.y * num;
				item.position = position;
				Color32 color = shadowColor;
				color.a = (byte)((float)(int)color.a / (float)iterations);
				item.color = color;
				verts.Add(item);
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			verts.Add(list[k]);
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			List<UIVertex> list = new List<UIVertex>();
			vh.GetUIVertexStream(list);
			DropShadowEffect(list);
			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
		}
	}
}
