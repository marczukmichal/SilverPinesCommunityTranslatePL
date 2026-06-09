using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyAmountChangedPopup : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_moneyCurrent;

	[SerializeField]
	private TextMeshProUGUI m_moneyDelta;

	[SerializeField]
	private RectTransform m_container;

	[SerializeField]
	private Color m_positiveDeltaColor;

	[SerializeField]
	private Color m_negativeDeltaColor;

	[Header("Animation")]
	[SerializeField]
	private float m_tickTime = 0.1f;

	[SerializeField]
	private float m_animInTime = 0.1f;

	[SerializeField]
	private float m_inHoldTime = 0.5f;

	[Header("Audio")]
	[SerializeField]
	private AudioEvent m_moneyIncreaseAudio;

	private int m_currentMoney;

	private void OnEnable()
	{
		m_container.pivot = new Vector2(1f, 0.5f);
		m_container.gameObject.SetActive(value: false);
		m_currentMoney = GlobalReferences.Instance.MainInventory.Money;
		GlobalReferences.Instance.EventChannels.Inventory.OnPlayerMoneyUpdated.Register(OnMoneyChanged);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Register(RefreshMoney);
	}

	private void OnDisable()
	{
		GlobalReferences.Instance.EventChannels.Inventory.OnPlayerMoneyUpdated.Unregister(OnMoneyChanged);
		GlobalReferences.Instance.EventChannels.SaveLoad.OnGameReloadedEvent.Unregister(RefreshMoney);
	}

	private void RefreshMoney()
	{
		m_currentMoney = GlobalReferences.Instance.MainInventory.Money;
	}

	private void OnMoneyChanged(int money)
	{
		if (money != m_currentMoney)
		{
			bool flag = money - m_currentMoney > 0;
			if (m_moneyIncreaseAudio != null && flag)
			{
				m_moneyIncreaseAudio.Play2D();
			}
			StopAllCoroutines();
			StartCoroutine(Animate(m_currentMoney, money));
		}
		m_currentMoney = money;
	}

	private IEnumerator Animate(int startingMoney, int endingMoney)
	{
		int delta = endingMoney - startingMoney;
		int currentMoney = startingMoney;
		m_container.gameObject.SetActive(value: true);
		m_moneyCurrent.text = "$" + currentMoney;
		bool isPositive = delta > 0;
		m_moneyDelta.color = (isPositive ? m_positiveDeltaColor : m_negativeDeltaColor);
		m_moneyDelta.text = (isPositive ? "+" : "-") + "$" + Mathf.Abs(delta);
		yield return m_container.DOPivotX(1f, m_animInTime).SetEase(Ease.InSine).SetUpdate(isIndependentUpdate: true)
			.WaitForCompletion();
		yield return new WaitForSecondsRealtime(m_inHoldTime);
		while (delta != 0)
		{
			if (isPositive)
			{
				delta--;
				currentMoney++;
			}
			else
			{
				delta++;
				currentMoney--;
			}
			m_moneyCurrent.text = "$" + currentMoney;
			if (delta == 0)
			{
				break;
			}
			m_moneyDelta.text = (isPositive ? "+" : "-") + "$" + delta;
			yield return new WaitForSecondsRealtime(m_tickTime);
		}
		m_moneyDelta.text = "";
		yield return new WaitForSecondsRealtime(1f);
		yield return m_container.DOPivotX(0f, m_animInTime).SetEase(Ease.OutSine).SetUpdate(isIndependentUpdate: true)
			.WaitForCompletion();
		m_container.gameObject.SetActive(value: false);
	}
}
