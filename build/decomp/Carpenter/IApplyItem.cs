using UnityEngine;

public interface IApplyItem
{
	bool CanApplyItem(ItemInstance item);

	GameObject GetApplyItemInteractUIPrefab();
}
