using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectableVisuals : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private TextMeshProUGUI m_firstNameField;

	[SerializeField]
	private TextMeshProUGUI m_lastNameField;

	[SerializeField]
	private TextMeshProUGUI m_dateOfBirthField;

	[SerializeField]
	private TextMeshProUGUI m_sexField;

	[SerializeField]
	private TextMeshProUGUI m_occupationField;

	[SerializeField]
	private Image m_fadeOverlay;

	private CollectableDefinition m_collectableDefinition;

	public UnityAction<CollectableVisuals> OnClicked;

	public CollectableDefinition CollectableDefinition => m_collectableDefinition;

	public void SetCollectable(CollectableDefinition collectableDefinition)
	{
		m_collectableDefinition = collectableDefinition;
		if (m_image != null)
		{
			m_image.sprite = collectableDefinition.Sprite;
		}
		if (m_firstNameField != null)
		{
			m_firstNameField.text = collectableDefinition.FirstName;
		}
		if (m_lastNameField != null)
		{
			m_lastNameField.text = collectableDefinition.LastName;
		}
		if (m_dateOfBirthField != null)
		{
			m_dateOfBirthField.text = collectableDefinition.DateOfBirth;
		}
		if (m_sexField != null)
		{
			m_sexField.text = collectableDefinition.Sex;
		}
		if (m_occupationField != null)
		{
			m_occupationField.text = collectableDefinition.Occupation;
		}
	}

	public void SetFaded(bool faded)
	{
		m_fadeOverlay.gameObject.SetActive(faded);
	}

	public void OnButtonClicked()
	{
		OnClicked?.Invoke(this);
	}
}
