using UnityEngine;
using UnityEngine.Localization;

public class MapPlayerIcon : Map3DIcon
{
	[SerializeField]
	private LocalizedString m_hoverText;

	public override bool CanBeGrouped => false;

	public override bool UseZoomLevelAlpha => false;

	public override bool ShouldScale => true;

	public override string GetHoverName()
	{
		return m_hoverText.GetLocalizedString();
	}

	public void UpdateRotation(Quaternion targetRotation, bool instant)
	{
		GameObject item = GlobalReferences.Instance.Anchors.Gameplay.PlayerAnchor.Item;
		if (item != null && item.GetComponent<CharacterDirection>().DesiredDirection == CharacterDirection.Facing.Left)
		{
			targetRotation *= Quaternion.Euler(0f, 0f, 180f);
		}
		Quaternion rotation = ((!instant) ? Quaternion.RotateTowards(base.transform.rotation, targetRotation, Time.unscaledDeltaTime * 750f) : targetRotation);
		base.transform.rotation = rotation;
	}
}
