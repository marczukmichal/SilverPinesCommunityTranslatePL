using System.Collections.Generic;
using UnityEngine;

public class LineMeshGenerator
{
	private List<Vector3[]> m_lineSegments = new List<Vector3[]>();

	private Mesh m_mesh;

	private float m_thickness = 0.1f;

	public Mesh Mesh => m_mesh;

	public LineMeshGenerator(float thickness = 0.1f)
	{
		m_thickness = thickness;
		m_mesh = new Mesh();
	}

	public void Cleanup()
	{
		Object.Destroy(m_mesh);
		m_mesh = null;
	}

	public void AddLine(Vector3 start, Vector3 end)
	{
		m_lineSegments.Add(new Vector3[2] { start, end });
		GenerateMesh();
	}

	public void GenerateMesh()
	{
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		new List<Vector2>();
		int num = 0;
		foreach (Vector3[] lineSegment in m_lineSegments)
		{
			Vector3 vector = lineSegment[0];
			Vector3 vector2 = lineSegment[1];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 vector3 = Vector3.up * m_thickness;
			Vector3 vector4 = Vector3.Cross(normalized, Vector3.up).normalized * m_thickness;
			Vector3 vector5 = vector - vector4 + vector3;
			Vector3 vector6 = vector + vector4 + vector3;
			Vector3 vector7 = vector - vector4 - vector3;
			Vector3 vector8 = vector + vector4 - vector3;
			Vector3 vector9 = vector2 - vector4 + vector3;
			Vector3 vector10 = vector2 + vector4 + vector3;
			Vector3 vector11 = vector2 - vector4 - vector3;
			Vector3 vector12 = vector2 + vector4 - vector3;
			list.AddRange(new Vector3[8] { vector5, vector6, vector7, vector8, vector9, vector10, vector11, vector12 });
			list2.AddRange(new int[36]
			{
				num,
				num + 1,
				num + 2,
				num + 2,
				num + 1,
				num + 3,
				num + 4,
				num + 6,
				num + 5,
				num + 5,
				num + 6,
				num + 7,
				num,
				num + 2,
				num + 4,
				num + 4,
				num + 2,
				num + 6,
				num + 1,
				num + 5,
				num + 3,
				num + 3,
				num + 5,
				num + 7,
				num,
				num + 4,
				num + 1,
				num + 1,
				num + 4,
				num + 5,
				num + 2,
				num + 3,
				num + 6,
				num + 6,
				num + 3,
				num + 7
			});
			num += 8;
		}
		m_mesh.Clear();
		m_mesh.vertices = list.ToArray();
		m_mesh.triangles = list2.ToArray();
		m_mesh.RecalculateNormals();
	}
}
