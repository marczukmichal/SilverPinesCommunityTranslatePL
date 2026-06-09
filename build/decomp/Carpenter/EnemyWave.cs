using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyWave : MonoBehaviour
{
	[Serializable]
	public struct EnemyInstance
	{
		public GameObject m_enemy;

		public float m_time;
	}

	[SerializeField]
	private float m_minWaitTimeAfterWaveDone;

	[SerializeField]
	private float m_maxWaitTimeAfterWaveDone;

	public EnemyInstance[] m_enemyInstances;

	public UnityAction OnWaveDone;

	private List<CharacterHealth> m_enemyHealthComponents;

	public void StartWave()
	{
		EnemyInstance[] enemyInstances = m_enemyInstances;
		for (int i = 0; i < enemyInstances.Length; i++)
		{
			EnemyInstance enemyInstance = enemyInstances[i];
			if (enemyInstance.m_enemy != null)
			{
				enemyInstance.m_enemy.SetActive(value: false);
			}
		}
		base.gameObject.SetActive(value: true);
		m_enemyHealthComponents = new List<CharacterHealth>();
		enemyInstances = m_enemyInstances;
		for (int i = 0; i < enemyInstances.Length; i++)
		{
			EnemyInstance enemyInstance2 = enemyInstances[i];
			if (enemyInstance2.m_enemy != null)
			{
				CharacterHealth component = enemyInstance2.m_enemy.GetComponent<CharacterHealth>();
				if (component != null)
				{
					m_enemyHealthComponents.Add(component);
				}
			}
		}
		StartCoroutine(DoWaveCoroutine());
	}

	private IEnumerator DoWaveCoroutine()
	{
		float currentTime = 0f;
		int currentIndex = 0;
		while (currentIndex < m_enemyInstances.Length)
		{
			for (currentTime += Time.deltaTime; currentIndex < m_enemyInstances.Length && currentTime >= m_enemyInstances[currentIndex].m_time; currentIndex++)
			{
				if (m_enemyInstances[currentIndex].m_enemy != null)
				{
					m_enemyInstances[currentIndex].m_enemy.SetActive(value: true);
				}
			}
			yield return new WaitForEndOfFrame();
		}
		float postWaveTimer = m_maxWaitTimeAfterWaveDone;
		while (postWaveTimer > 0f)
		{
			postWaveTimer -= Time.deltaTime;
			bool flag = true;
			foreach (CharacterHealth enemyHealthComponent in m_enemyHealthComponents)
			{
				if (!enemyHealthComponent.IsDead)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				postWaveTimer = Mathf.Min(postWaveTimer, m_minWaitTimeAfterWaveDone);
			}
			yield return new WaitForEndOfFrame();
		}
		OnWaveDone?.Invoke();
	}
}
