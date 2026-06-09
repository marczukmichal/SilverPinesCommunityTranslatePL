using System;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectiveShowConditional : MonoBehaviour
{
	[Serializable]
	private struct VisualsSettings
	{
		[Serializable]
		private struct ImageSettings
		{
			[SerializeField]
			private SpriteRenderer m_sprite;

			[SerializeField]
			private Color m_color;

			public void Apply()
			{
				m_sprite.color = m_color;
			}
		}

		[SerializeField]
		private ImageSettings[] m_sprites;

		public void Apply()
		{
			ImageSettings[] sprites = m_sprites;
			foreach (ImageSettings imageSettings in sprites)
			{
				imageSettings.Apply();
			}
		}
	}

	[SerializeField]
	private List<Map3DFloor> m_showOnFloors;

	[SerializeField]
	private ItemDefinition m_itemDefinition;

	[SerializeField]
	private VisualsSettings m_hasItemSettings;

	[SerializeField]
	private VisualsSettings m_doesNotHaveItemSettings;

	public void ShowForFloor(Map3DFloor floor)
	{
		bool active = true;
		if (m_showOnFloors != null && m_showOnFloors.Count > 0 && !m_showOnFloors.Contains(floor))
		{
			active = false;
		}
		if (m_itemDefinition != null)
		{
			if (GlobalReferences.Instance.MainInventory.HasSeenItemType(m_itemDefinition))
			{
				m_hasItemSettings.Apply();
			}
			else
			{
				m_doesNotHaveItemSettings.Apply();
			}
		}
		base.gameObject.SetActive(active);
	}
}
