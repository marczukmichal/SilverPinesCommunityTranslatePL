using System.Collections;
using UnityEngine;

public class AchievementBirdFlock : MonoBehaviour
{
	[SerializeField]
	private AchievementDefinition m_achievementDefinition;

	private int m_birdCount;

	private bool m_startTimer;

	private void Start()
	{
		CharacterHealth[] componentsInChildren = GetComponentsInChildren<CharacterHealth>();
		m_birdCount = componentsInChildren.Length;
		if (m_birdCount == 0)
		{
			base.enabled = false;
			return;
		}
		CharacterHealth[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsDead)
			{
				base.enabled = false;
				return;
			}
		}
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnDead.AddListener(OnBirdKilled);
		}
	}

	private void OnBirdKilled()
	{
		if (base.enabled && !m_startTimer)
		{
			m_startTimer = true;
			StartCoroutine(ActiveCoroutine());
		}
	}

	private IEnumerator ActiveCoroutine()
	{
		yield return new WaitForSeconds(1f);
		CharacterHealth[] componentsInChildren = GetComponentsInChildren<CharacterHealth>();
		bool flag = true;
		CharacterHealth[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsDead)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			m_achievementDefinition.UnlockAchievement();
		}
		base.enabled = false;
	}
}
