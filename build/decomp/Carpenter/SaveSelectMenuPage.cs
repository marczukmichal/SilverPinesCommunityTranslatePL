using UnityEngine;

public class SaveSelectMenuPage : MenuPage
{
	[SerializeField]
	private SaveSelectPanel m_saveSelectPanel;

	public override void Show(bool instant = false, bool onAwake = false)
	{
		base.Show(instant, onAwake);
		m_saveSelectPanel.Show();
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		m_saveSelectPanel.Hide();
	}

	public override bool OnBackInput()
	{
		if (m_saveSelectPanel.IsConfirmationDialogueActive())
		{
			m_saveSelectPanel.CancelAction();
			return true;
		}
		return base.OnBackInput();
	}
}
