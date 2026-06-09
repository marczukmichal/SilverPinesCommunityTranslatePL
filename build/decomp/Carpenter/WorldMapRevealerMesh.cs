using UnityEngine;
using UnityEngine.Rendering;

public class WorldMapRevealerMesh : MonoBehaviour
{
	[SerializeField]
	private Mesh m_mesh;

	public void DrawFogOfWarVisibilityMeshes(CommandBuffer cmd, Material material)
	{
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		cmd.DrawMesh(m_mesh, localToWorldMatrix, material, 0, 0);
	}

	private void OnDrawGizmosSelected()
	{
		if (!(m_mesh == null))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireMesh(m_mesh);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
