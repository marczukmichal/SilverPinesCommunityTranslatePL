using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingActiveItem : MonoBehaviour
{
	[SerializeField]
	private Inventory m_inventory;

	[SerializeField]
	private ItemDefinition m_itemDefinition;

	[SerializeField]
	private bool m_ignoreDeltaTime;

	[SerializeField]
	private float m_transitionSpeed = 1f;

	[SerializeField]
	private float m_activeValue = 1f;

	[SerializeField]
	private float m_inactiveValue;

	private float m_targetValue;

	private float m_currentValue;

	private Volume m_volume;

	private void Awake()
	{
		m_volume = GetComponent<Volume>();
		m_volume.enabled = false;
	}

	public void Update()
	{
		bool flag = false;
		ItemInstance itemOfType = m_inventory.GetItemOfType(m_itemDefinition);
		if (itemOfType != null)
		{
			flag = itemOfType.Activated;
		}
		m_targetValue = (flag ? m_activeValue : m_inactiveValue);
		float num = Mathf.MoveTowards(m_currentValue, m_targetValue, (m_ignoreDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime) * m_transitionSpeed);
		if (num != m_currentValue)
		{
			ApplyValue(num);
		}
	}

	private void ApplyValue(float newValue)
	{
		m_currentValue = newValue;
		m_volume.weight = newValue;
		m_volume.enabled = newValue > 0f;
	}
}
