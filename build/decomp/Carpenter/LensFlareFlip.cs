using UnityEngine;
using UnityEngine.Rendering;

public class LensFlareFlip : MonoBehaviour
{
	[SerializeField]
	private Transform m_flipTransform;

	[SerializeField]
	private LensFlareComponentSRP m_lensFlare;

	private LensFlareDataSRP m_localLensFlareData;

	private Vector2 m_baseScale;

	private void Awake()
	{
		m_localLensFlareData = Object.Instantiate(m_lensFlare.lensFlareData);
		m_lensFlare.lensFlareData = m_localLensFlareData;
		m_baseScale = m_localLensFlareData.elements[0].sizeXY;
	}

	private void OnDestroy()
	{
		if ((bool)m_localLensFlareData)
		{
			Object.Destroy(m_localLensFlareData);
			m_localLensFlareData = null;
		}
	}

	private void Update()
	{
		if (m_flipTransform.localScale.x < 0f)
		{
			m_lensFlare.lensFlareData.elements[0].sizeXY = new Vector2(m_baseScale.x * -1f, m_baseScale.y);
		}
		else
		{
			m_lensFlare.lensFlareData.elements[0].sizeXY = m_baseScale;
		}
		m_lensFlare.lensFlareData.elements[0].rotation = m_flipTransform.rotation.eulerAngles.z;
	}
}
