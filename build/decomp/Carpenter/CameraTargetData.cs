using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraTargetData", menuName = "Misc/Camera Target Data")]
public class CameraTargetData : ScriptableObject
{
	public bool m_active;

	public Vector3 m_position;

	public GameObject m_targetObject;

	public Vector3 m_baseOffset;

	public Vector3 m_userOffset;

	public Bounds m_characterBounds;

	public CharacterDirection.Facing m_facing;

	public float m_zoomScale;
}
