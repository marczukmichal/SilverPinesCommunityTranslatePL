using UnityEngine;

[ExecuteInEditMode]
public class SpriteToMeshConverter : MonoBehaviour
{
	[ContextMenu("Convert Sprite To Mesh")]
	public void ConvertSpriteToMesh()
	{
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component == null)
		{
			Debug.LogError("SpriteRenderer component not found!");
			return;
		}
		Sprite sprite = component.sprite;
		if (sprite == null)
		{
			Debug.LogError("Sprite not found!");
			return;
		}
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[4];
		Vector2[] array2 = new Vector2[4];
		int[] array3 = new int[6];
		float num = sprite.rect.width / sprite.pixelsPerUnit;
		float num2 = sprite.rect.height / sprite.pixelsPerUnit;
		Vector3 vector = new Vector3(sprite.pivot.x / sprite.pixelsPerUnit, sprite.pivot.y / sprite.pixelsPerUnit, 0f);
		vector.x -= num * 0.5f;
		vector.y += num2 * 0.5f;
		Vector3 vector2 = new Vector3((0f - num) / 2f, (0f - num2) / 2f, 0f) + vector;
		Vector3 vector3 = new Vector3((0f - num) / 2f, num2 / 2f, 0f) + vector;
		Vector3 vector4 = new Vector3(num / 2f, num2 / 2f, 0f) + vector;
		Vector3 vector5 = new Vector3(num / 2f, (0f - num2) / 2f, 0f) + vector;
		array[0] = vector2;
		array[1] = vector3;
		array[2] = vector4;
		array[3] = vector5;
		array2[0] = new Vector2(sprite.rect.x / (float)sprite.texture.width, sprite.rect.y / (float)sprite.texture.height);
		array2[1] = new Vector2(sprite.rect.x / (float)sprite.texture.width, (sprite.rect.y + sprite.rect.height) / (float)sprite.texture.height);
		array2[2] = new Vector2((sprite.rect.x + sprite.rect.width) / (float)sprite.texture.width, (sprite.rect.y + sprite.rect.height) / (float)sprite.texture.height);
		array2[3] = new Vector2((sprite.rect.x + sprite.rect.width) / (float)sprite.texture.width, sprite.rect.y / (float)sprite.texture.height);
		array3[0] = 0;
		array3[1] = 1;
		array3[2] = 2;
		array3[3] = 0;
		array3[4] = 2;
		array3[5] = 3;
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		Object.DestroyImmediate(component);
		MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		base.gameObject.AddComponent<MeshFilter>().mesh = mesh;
		meshRenderer.material = new Material(Shader.Find("Sprites/Default"))
		{
			mainTexture = sprite.texture
		};
	}
}
