using UnityEngine;

[CreateAssetMenu(fileName = "Achievements", menuName = "Misc/Achievements")]
public class AchievementDefinition : ScriptableObject
{
	[SerializeField]
	private string m_achievementID;

	[SerializeField]
	private string m_comment;

	[Header("Optional: Goal Value Achievements")]
	[SerializeField]
	private int m_goalValue;

	public string AchievementID => m_achievementID;

	public void UnlockAchievement()
	{
		if ((bool)GlobalReferences.Instance.Anchors.Achievements.AchievementManagerAnchor.Item)
		{
			GlobalReferences.Instance.Anchors.Achievements.AchievementManagerAnchor.Item.UnlockAchievement(this);
		}
	}

	public void UnlockIfValueReached(int value)
	{
		if (m_goalValue != 0 && value >= m_goalValue)
		{
			UnlockAchievement();
		}
	}
}
