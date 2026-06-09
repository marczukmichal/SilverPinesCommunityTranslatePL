using Cinemachine;
using UnityEngine;

public class CinemachineCameraBlend : MonoBehaviour
{
	[Header("Depth of Field")]
	[SerializeField]
	private bool m_changeDepthOfField;

	[SerializeField]
	private float m_depthOfFieldOffset;

	[Header("Fog")]
	[SerializeField]
	private bool m_changeFog;

	[SerializeField]
	private float m_fogStartPositionOffset;

	private CinemachineBrain m_brain;

	private CinemachineVirtualCamera m_virtualCamera;

	public bool ChangeDepthOfField => m_changeDepthOfField;

	public float DepthOfFieldOffset => m_depthOfFieldOffset * CurrentBlendWeight;

	public bool ChangeFog => m_changeFog;

	public float FogStartPositionOffset => m_fogStartPositionOffset * CurrentBlendWeight;

	public bool IsCameraActive
	{
		get
		{
			if (m_brain != null)
			{
				if (m_brain.ActiveVirtualCamera.Equals(m_virtualCamera))
				{
					return true;
				}
				if (m_brain.IsBlending)
				{
					if (m_brain.ActiveBlend.CamA.Equals(m_virtualCamera))
					{
						return true;
					}
					if (m_brain.ActiveBlend.CamB.Equals(m_virtualCamera))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public float CurrentBlendWeight
	{
		get
		{
			float num = 0f;
			if (m_brain.IsBlending)
			{
				CinemachineBlend activeBlend = m_brain.ActiveBlend;
				if (activeBlend != null)
				{
					num = activeBlend.BlendWeight;
					if (!m_virtualCamera.Equals(m_brain.ActiveVirtualCamera))
					{
						num = 1f - num;
					}
				}
			}
			else if (m_virtualCamera.Equals(m_brain.ActiveVirtualCamera))
			{
				num = 1f;
			}
			return num;
		}
	}

	private void Awake()
	{
		m_brain = Object.FindFirstObjectByType<CinemachineBrain>();
		m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
	}

	private void OnEnable()
	{
		GlobalReferences.Instance.Anchors.Camera.CinemachineCameraBlendAnchor.Set(this);
	}

	private void OnDestroy()
	{
		if (GlobalReferences.Instance.Anchors.Camera.CinemachineCameraBlendAnchor.Item == this)
		{
			GlobalReferences.Instance.Anchors.Camera.CinemachineCameraBlendAnchor.Set(this);
		}
	}
}
