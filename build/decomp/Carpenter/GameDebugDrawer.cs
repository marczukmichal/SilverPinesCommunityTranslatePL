using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class GameDebugDrawer : ImmediateModeShapeDrawer
{
	public enum DebugShapeType
	{
		Point,
		Directional,
		Cube
	}

	private struct DebugShape
	{
		public DebugShapeType m_type;

		public Vector3 m_position;

		public Vector3 m_offsetDirection;

		public Vector3 m_size;

		public Color m_color;

		public float m_killTime;
	}

	[SerializeField]
	private Color m_damageableColor = Color.green;

	[SerializeField]
	private Color m_stumbleColor = Color.blue;

	[DebugCommand("draw_damage_colliders", "Draw active damage colliders debug to screen", "draw_damage_colliders <true/false>", typeof(bool), false)]
	public static bool DRAW_DAMAGE_COLLIDERS;

	[DebugCommand("draw_damage_modifizer_zones", "Draw active damage modifier zones e.g. weakpoints to screen", "draw_damage_modifizer_zones <true/false>", typeof(bool), false)]
	public static bool DRAW_DAMAGE_MODIFIER_ZONES;

	[DebugCommand("draw_damageables", "Draw damageables to screen", "draw_damageables <true/false>", typeof(bool), false)]
	public static bool DRAW_DAMAGEABLES;

	[DebugCommand("draw_stumble_colliders", "Draw stumble colliders to screen", "draw_stumble_colliders <true/false>", typeof(bool), false)]
	public static bool DRAW_STUMBLE_COLLIDERS;

	private static GameDebugDrawer m_instance;

	private List<DebugShape> m_debugShapes = new List<DebugShape>();

	private static GameDebugDrawer Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = UnityEngine.Object.FindAnyObjectByType<GameDebugDrawer>();
			}
			return m_instance;
		}
	}

	public static void TempDebugDrawPoint(Vector3 position, float time, Color color)
	{
		GameDebugDrawer instance = Instance;
		if (instance != null)
		{
			instance.m_debugShapes.Add(new DebugShape
			{
				m_type = DebugShapeType.Point,
				m_position = position,
				m_color = color,
				m_killTime = Time.time + time
			});
		}
	}

	public static void TempDebugDrawDirection(Vector3 position, Vector3 offsetDirection, float time, Color color)
	{
		GameDebugDrawer instance = Instance;
		if (instance != null)
		{
			instance.m_debugShapes.Add(new DebugShape
			{
				m_type = DebugShapeType.Directional,
				m_position = position,
				m_offsetDirection = offsetDirection,
				m_color = color,
				m_killTime = Time.time + time
			});
		}
	}

	public static void TempDebugDrawCube(Vector3 position, Vector3 size, float time, Color color)
	{
		GameDebugDrawer instance = Instance;
		if (instance != null)
		{
			instance.m_debugShapes.Add(new DebugShape
			{
				m_type = DebugShapeType.Cube,
				m_position = position,
				m_size = size,
				m_color = color,
				m_killTime = Time.time + time
			});
		}
	}

	public override void DrawShapes(Camera cam)
	{
		using (Draw.Command(cam))
		{
			Draw.LineGeometry = LineGeometry.Volumetric3D;
			Draw.ThicknessSpace = ThicknessSpace.Pixels;
			if (DRAW_DAMAGE_COLLIDERS)
			{
				DrawDamageColliders();
			}
			if (DRAW_DAMAGE_MODIFIER_ZONES)
			{
				DrawDamageModifizerZones();
			}
			if (DRAW_DAMAGEABLES)
			{
				DrawDamageables();
			}
			if (DRAW_STUMBLE_COLLIDERS)
			{
				DrawStumbleColliders();
			}
			if (GameDebugCommands.CAMERA_DEBUG)
			{
				DrawCameraDebug();
			}
			if (CharacterLedgeGrab.LEDGE_DEBUG)
			{
				DrawLedgeDebugs();
			}
			DrawGenericDebugShapes();
		}
	}

	private void Update()
	{
		if (m_debugShapes.Count > 0)
		{
			m_debugShapes.RemoveAll((DebugShape x) => Time.time > x.m_killTime);
		}
	}

	private void DrawGenericDebugShapes()
	{
		Draw.ZTest = CompareFunction.Always;
		foreach (DebugShape debugShape in m_debugShapes)
		{
			if (debugShape.m_type == DebugShapeType.Point)
			{
				Draw.Sphere(debugShape.m_position, 0.05f, debugShape.m_color);
				continue;
			}
			if (debugShape.m_type == DebugShapeType.Cube)
			{
				Draw.Cuboid(debugShape.m_position, debugShape.m_size, debugShape.m_color);
				continue;
			}
			Draw.Sphere(debugShape.m_position, 0.05f, debugShape.m_color);
			Draw.Line(debugShape.m_position, debugShape.m_position + debugShape.m_offsetDirection, 0.5f, debugShape.m_color, debugShape.m_color * 0.5f);
		}
		Draw.ZTest = CompareFunction.LessEqual;
	}

	private void DrawDamageColliders()
	{
		DamageCollider[] array = UnityEngine.Object.FindObjectsByType<DamageCollider>(FindObjectsSortMode.None);
		foreach (DamageCollider damageCollider in array)
		{
			if (!damageCollider.enabled || !damageCollider.DamageEnabled)
			{
				continue;
			}
			Collider2D[] componentsInChildren = damageCollider.GetComponentsInChildren<Collider2D>();
			foreach (Collider2D collider2D in componentsInChildren)
			{
				DrawCollider2D(collider2D, damageCollider.GetDebugColor());
				if (damageCollider.CanRebound)
				{
					Vector3 size = collider2D.bounds.size;
					size.y = DamageCollider.s_reboundColliisionCheckHeight;
					Color yellow = Color.yellow;
					yellow.a = 0.1f;
					Draw.Cuboid(collider2D.transform.position, size, yellow);
				}
			}
		}
	}

	private void DrawDamageModifizerZones()
	{
		DamageModifierZone[] array = UnityEngine.Object.FindObjectsByType<DamageModifierZone>(FindObjectsSortMode.None);
		foreach (DamageModifierZone damageModifierZone in array)
		{
			if (damageModifierZone.enabled)
			{
				Draw.Sphere(damageModifierZone.transform.position, damageModifierZone.Radius, Color.yellow);
			}
		}
	}

	private void DrawStumbleColliders()
	{
		StumbleCollider[] array = UnityEngine.Object.FindObjectsByType<StumbleCollider>(FindObjectsSortMode.None);
		foreach (StumbleCollider stumbleCollider in array)
		{
			if (stumbleCollider.enabled && stumbleCollider.StumbleActive)
			{
				Collider2D[] componentsInChildren = stumbleCollider.GetComponentsInChildren<Collider2D>();
				foreach (Collider2D collider in componentsInChildren)
				{
					DrawCollider2D(collider, m_stumbleColor);
				}
			}
		}
	}

	private void DrawDamageables()
	{
		Collider2D[] array = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (Collider2D collider2D in array)
		{
			if ((collider2D.gameObject.layer == GameLayers.DamageableLayer || collider2D.gameObject.layer == GameLayers.GameplayElementsLayer) && collider2D.GetComponentInParent<IDamageable>() != null)
			{
				DrawCollider2D(collider2D, m_damageableColor);
			}
		}
	}

	private void DrawCameraDebug()
	{
		FollowPlayerVirtualCamera followPlayerVirtualCamera = UnityEngine.Object.FindAnyObjectByType<FollowPlayerVirtualCamera>();
		if (followPlayerVirtualCamera != null)
		{
			followPlayerVirtualCamera.DrawDebug();
		}
	}

	private void DrawLedgeDebugs()
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null)
		{
			CharacterLedgeGrab component = item.GetComponent<CharacterLedgeGrab>();
			if (component != null)
			{
				component.DrawDebugShapes();
			}
		}
	}

	private void DrawCollider2D(Collider2D collider, Color color)
	{
		if (collider is BoxCollider2D)
		{
			DrawBoxCollider((BoxCollider2D)collider, color);
		}
		else if (collider is CircleCollider2D)
		{
			DrawCircleCollider((CircleCollider2D)collider, color);
		}
		else if (collider is PolygonCollider2D)
		{
			DrawPolygonCollider((PolygonCollider2D)collider, color);
		}
		else if (collider is CapsuleCollider2D)
		{
			DrawCapsuleCollider((CapsuleCollider2D)collider, color);
		}
	}

	public static void Draw2DBounds(Bounds bounds, Color color)
	{
		PolylinePath polylinePath = new PolylinePath();
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		float z = (max.z + min.z) / 2f;
		polylinePath.AddPoint(new Vector3(min.x, min.y, z));
		polylinePath.AddPoint(new Vector3(min.x, max.y, z));
		polylinePath.AddPoint(new Vector3(max.x, max.y, z));
		polylinePath.AddPoint(new Vector3(max.x, min.y, z));
		Draw.Polyline(polylinePath, closed: true, 2f, color);
		color.a = 0.1f;
		Draw.Cuboid(bounds.center, bounds.size, color);
	}

	public static void DrawBoxCollider(BoxCollider2D collider, Color color)
	{
		Draw.Cuboid(collider.transform.TransformPoint(collider.offset), collider.transform.rotation, collider.size, color);
	}

	public static void DrawCircleCollider(CircleCollider2D collider, Color color)
	{
		Draw.Sphere(collider.transform.TransformPoint(collider.offset), collider.radius, color);
	}

	public static void DrawPolygonCollider(PolygonCollider2D collider, Color color)
	{
		PolygonPath polygonPath = new PolygonPath();
		Vector2[] points = collider.points;
		foreach (Vector2 vector in points)
		{
			polygonPath.AddPoint(collider.transform.TransformPoint(vector));
		}
		Draw.Polygon(polygonPath, color);
	}

	public static void DrawCapsuleCollider(CapsuleCollider2D collider, Color color)
	{
		Vector2[] approximatePoints = GetApproximatePoints(collider);
		PolygonPath polygonPath = new PolygonPath();
		Vector2[] array = approximatePoints;
		foreach (Vector2 vector in array)
		{
			polygonPath.AddPoint(collider.transform.TransformPoint(vector));
		}
		Draw.Polygon(polygonPath, color);
	}

	public static Vector2[] GetApproximatePoints(CapsuleCollider2D capsuleCollider, int segments = 20)
	{
		List<Vector2> list = new List<Vector2>();
		Vector2 zero = Vector2.zero;
		if ((capsuleCollider.direction == CapsuleDirection2D.Vertical && capsuleCollider.size.y < capsuleCollider.size.x) || (capsuleCollider.direction == CapsuleDirection2D.Horizontal && capsuleCollider.size.x < capsuleCollider.size.y))
		{
			float num = MathF.PI * 2f / (float)segments;
			float num2 = Mathf.Max(capsuleCollider.size.x, capsuleCollider.size.y) * 0.5f;
			for (int i = 0; i < segments; i++)
			{
				float f = num * (float)i;
				float x = zero.x + Mathf.Cos(f) * num2;
				float y = zero.y - Mathf.Sin(f) * num2;
				list.Add(new Vector2(x, y));
			}
		}
		else
		{
			float num3;
			float num4;
			if (capsuleCollider.direction == CapsuleDirection2D.Vertical)
			{
				num3 = capsuleCollider.size.x * 0.5f;
				num4 = capsuleCollider.size.y - capsuleCollider.size.x;
			}
			else
			{
				num3 = capsuleCollider.size.y * 0.5f;
				num4 = capsuleCollider.size.x - capsuleCollider.size.y;
			}
			float num5 = num4 * 0.5f;
			float num6 = MathF.PI * 2f / (float)segments;
			list.Add(new Vector2(zero.x + num3, zero.y + num5));
			list.Add(new Vector2(zero.x + num3, zero.y - num5));
			for (int j = 1; j < segments / 2; j++)
			{
				float f2 = num6 * (float)j;
				float x2 = zero.x + Mathf.Cos(f2) * num3;
				float y2 = zero.y - num5 - Mathf.Sin(f2) * num3;
				list.Add(new Vector2(x2, y2));
			}
			list.Add(new Vector2(zero.x - num3, zero.y - num5));
			list.Add(new Vector2(zero.x - num3, zero.y + num5));
			for (int k = 1; k < segments / 2; k++)
			{
				float f3 = num6 * (float)k;
				float x3 = zero.x - Mathf.Cos(f3) * num3;
				float y3 = zero.y + num5 + Mathf.Sin(f3) * num3;
				list.Add(new Vector2(x3, y3));
			}
			if (capsuleCollider.direction == CapsuleDirection2D.Horizontal)
			{
				for (int l = 0; l < list.Count; l++)
				{
					list[l] = list[l].Rotate(90f);
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			list[m] += capsuleCollider.offset;
		}
		return list.ToArray();
	}
}
