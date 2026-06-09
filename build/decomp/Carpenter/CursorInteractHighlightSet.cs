using UnityEngine;

[CreateAssetMenu(menuName = "Sets/Cursor Interact Highlight")]
public class CursorInteractHighlightSet : RuntimeSet<CursorInteractHighlightController>
{
	public CursorInteractHighlightController GetController()
	{
		int num = int.MinValue;
		CursorInteractHighlightController result = null;
		foreach (CursorInteractHighlightController item in m_items)
		{
			if (item.Priority > num)
			{
				result = item;
				num = item.Priority;
			}
		}
		return result;
	}
}
