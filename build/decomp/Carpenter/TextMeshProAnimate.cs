using TMPro;
using UnityEngine;

public class TextMeshProAnimate : MonoBehaviour
{
	private TextMeshProUGUI m_textComponent;

	private float m_time;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_characterOffset;

	[SerializeField]
	private float m_amplitude;

	private void OnEnable()
	{
		m_textComponent = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		m_time += Time.unscaledDeltaTime * m_speed;
		m_textComponent.ForceMeshUpdate();
		Mesh mesh = m_textComponent.mesh;
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < m_textComponent.textInfo.characterCount; i++)
		{
			int vertexIndex = m_textComponent.textInfo.characterInfo[i].vertexIndex;
			Vector3 vector = new Vector3(0f, Mathf.Sin(m_time + (float)i * m_characterOffset) * m_amplitude, 0f);
			vertices[vertexIndex] += vector;
			vertices[vertexIndex + 1] += vector;
			vertices[vertexIndex + 2] += vector;
			vertices[vertexIndex + 3] += vector;
		}
		mesh.vertices = vertices;
		m_textComponent.canvasRenderer.SetMesh(mesh);
	}
}
