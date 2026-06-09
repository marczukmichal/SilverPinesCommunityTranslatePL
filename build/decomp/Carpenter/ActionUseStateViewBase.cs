using UnityEngine;

public abstract class ActionUseStateViewBase : MonoBehaviour
{
	public abstract void Setup(int currentCount, int maxCount);

	public abstract void UpdateAmmoCount(int newAmmoAmount, bool isLast);
}
