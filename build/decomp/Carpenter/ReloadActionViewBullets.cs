using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ReloadActionViewBullets : ActionUseStateViewBase
{
	[SerializeField]
	private Image[] m_bulletImages;

	[SerializeField]
	private float m_startScale = 2f;

	[SerializeField]
	private float m_animSpeed = 0.2f;

	[Header("Optional")]
	[SerializeField]
	private float m_rotationDegreesAddedPerBullet;

	[SerializeField]
	private float m_rotateSpeed;

	private float m_targetRotation;

	private float m_currentRotation;

	public override void UpdateAmmoCount(int newAmmoAmount, bool isLast)
	{
		for (int i = 0; i < m_bulletImages.Length; i++)
		{
			bool activeSelf = m_bulletImages[i].gameObject.activeSelf;
			bool flag = i < newAmmoAmount;
			if (flag != activeSelf)
			{
				m_bulletImages[i].gameObject.SetActive(flag);
				if (flag)
				{
					Color white = Color.white;
					white.a = 0f;
					m_bulletImages[i].color = white;
					m_bulletImages[i].DOFade(1f, m_animSpeed);
					m_bulletImages[i].transform.localScale = new Vector3(m_startScale, m_startScale, m_startScale);
					m_bulletImages[i].transform.DOScale(1f, m_animSpeed);
					m_targetRotation += m_rotationDegreesAddedPerBullet;
				}
			}
		}
	}

	public override void Setup(int currentCount, int maxCount)
	{
		for (int i = 0; i < m_bulletImages.Length; i++)
		{
			m_bulletImages[i].transform.localScale = Vector3.one;
			m_bulletImages[i].gameObject.SetActive(i < currentCount);
		}
	}

	public void OnDestroy()
	{
		for (int i = 0; i < m_bulletImages.Length; i++)
		{
			DOTween.Kill(m_bulletImages[i]);
			DOTween.Kill(m_bulletImages[i].transform);
		}
	}

	private void Update()
	{
		if (m_targetRotation > 0f)
		{
			m_currentRotation = Mathf.MoveTowardsAngle(m_currentRotation, m_targetRotation, Time.deltaTime * m_rotateSpeed);
			base.transform.localRotation = Quaternion.Euler(0f, 0f, m_currentRotation);
		}
	}
}
