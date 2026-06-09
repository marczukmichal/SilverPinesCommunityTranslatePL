using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseCodeCombinationValue : MonoBehaviour
{
	private enum QueuedInput
	{
		None,
		Increment,
		Decrement
	}

	[SerializeField]
	protected int m_numValues = 10;

	[SerializeField]
	private AudioEvent m_valueChangedAudioEvent;

	private bool m_isAnimating;

	protected int m_currentIndex;

	public UnityAction<int> OnValueChanged;

	private QueuedInput m_queuedInput;

	public int CurrentIndex => m_currentIndex;

	public abstract string CurrentValue { get; }

	public void SetValueToIndex(int index)
	{
		m_currentIndex = index;
		UpdateDecoration();
	}

	protected virtual void UpdateDecoration()
	{
	}

	private void Start()
	{
		UpdateDecoration();
	}

	public void Increment()
	{
		if (m_isAnimating)
		{
			m_queuedInput = QueuedInput.Increment;
			return;
		}
		m_currentIndex = GetLoopedValue(m_currentIndex + 1);
		StartCoroutine(Animate(increment: true));
		OnValueChanged?.Invoke(m_currentIndex);
	}

	public void Decrement()
	{
		if (m_isAnimating)
		{
			m_queuedInput = QueuedInput.Decrement;
			return;
		}
		m_currentIndex = GetLoopedValue(m_currentIndex - 1);
		StartCoroutine(Animate(increment: false));
		OnValueChanged?.Invoke(m_currentIndex);
	}

	protected abstract IEnumerator AnimateInternal(bool increment);

	private IEnumerator Animate(bool increment)
	{
		if (m_valueChangedAudioEvent != null)
		{
			m_valueChangedAudioEvent.Play2D();
		}
		m_isAnimating = true;
		yield return AnimateInternal(increment);
		yield return new WaitForSeconds(0.1f);
		m_isAnimating = false;
	}

	private void Update()
	{
		if (m_queuedInput != 0 && !m_isAnimating)
		{
			if (m_queuedInput == QueuedInput.Increment)
			{
				Increment();
			}
			else if (m_queuedInput == QueuedInput.Decrement)
			{
				Decrement();
			}
			m_queuedInput = QueuedInput.None;
		}
	}

	protected int GetLoopedValue(int value)
	{
		if (value < 0)
		{
			return m_numValues + value;
		}
		return value % m_numValues;
	}
}
