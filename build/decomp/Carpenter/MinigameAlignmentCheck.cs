using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MinigameAlignmentCheck : MonoBehaviour, IMinigameComponent
{
	public enum AlignMode
	{
		Static,
		FirstElement
	}

	[SerializeField]
	private MinigameFreeMoveObject[] m_alignObjects;

	[SerializeField]
	private float m_maxDistance = 10f;

	[SerializeField]
	private float m_maxRotationOffset = 2f;

	[SerializeField]
	private UnityEvent m_onCompleteEvent;

	[SerializeField]
	private AlignMode m_alignMode;

	[SerializeField]
	private float m_requiredHoverTime = 1f;

	private float m_currentPositionDistance;

	private float m_currentRotationOffset;

	private bool m_isComplete;

	private float m_successHoverTimer;

	public MinigameFreeMoveObject[] AlignObjects => m_alignObjects;

	private void Update()
	{
		if (!m_isComplete)
		{
			if (DoTest())
			{
				m_successHoverTimer += Time.unscaledDeltaTime;
			}
			else
			{
				m_successHoverTimer = 0f;
			}
			if (m_successHoverTimer > m_requiredHoverTime)
			{
				m_isComplete = true;
				StartCoroutine(SnapCompletion());
			}
		}
	}

	private IEnumerator SnapCompletion()
	{
		Vector2 basePosition;
		float baseRotation;
		if (m_alignMode == AlignMode.FirstElement)
		{
			basePosition = m_alignObjects[0].MatchCenterPosition;
			baseRotation = m_alignObjects[0].Rotation;
		}
		else
		{
			basePosition = base.transform.position;
			baseRotation = base.transform.rotation.eulerAngles.z;
		}
		int startingIndex = 0;
		if (m_alignMode == AlignMode.FirstElement)
		{
			startingIndex = 1;
		}
		for (int i = 0; i < m_alignObjects.Length; i++)
		{
			m_alignObjects[i].enabled = false;
		}
		yield return new WaitForEndOfFrame();
		for (int j = startingIndex; j < m_alignObjects.Length; j++)
		{
			m_alignObjects[j].transform.rotation = Quaternion.Euler(0f, 0f, baseRotation);
		}
		yield return new WaitForEndOfFrame();
		for (int k = startingIndex; k < m_alignObjects.Length; k++)
		{
			Vector2 vector = basePosition - m_alignObjects[k].MatchCenterPosition;
			m_alignObjects[k].transform.Translate(vector);
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForSecondsRealtime(2f);
		m_onCompleteEvent.Invoke();
	}

	private bool DoTest()
	{
		if (m_alignObjects.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < m_alignObjects.Length; i++)
		{
			if (!m_alignObjects[i].gameObject.activeSelf)
			{
				return false;
			}
		}
		Vector2 a;
		float num;
		if (m_alignMode == AlignMode.FirstElement)
		{
			a = m_alignObjects[0].MatchCenterPosition;
			num = m_alignObjects[0].Rotation;
		}
		else
		{
			a = base.transform.position;
			num = base.transform.rotation.eulerAngles.z;
		}
		m_currentPositionDistance = 0f;
		m_currentRotationOffset = 0f;
		int num2 = 0;
		if (m_alignMode == AlignMode.FirstElement)
		{
			num2 = 1;
		}
		for (int j = num2; j < m_alignObjects.Length; j++)
		{
			float b = Vector2.Distance(a, m_alignObjects[j].MatchCenterPosition);
			float num3 = Mathf.Abs(num - m_alignObjects[j].Rotation);
			if (num3 > 180f)
			{
				num3 = 360f - num3;
			}
			m_currentPositionDistance = Mathf.Max(m_currentPositionDistance, b);
			m_currentRotationOffset = Mathf.Max(m_currentRotationOffset, num3);
		}
		if (m_currentPositionDistance > m_maxDistance || m_currentRotationOffset > m_maxRotationOffset)
		{
			return false;
		}
		return true;
	}

	public void Setup(GameObject parent)
	{
		MinigameOverheadProjectorListener component = parent.GetComponent<MinigameOverheadProjectorListener>();
		if (component != null)
		{
			component.RegisterAlignmentMinigame(this);
		}
	}
}
