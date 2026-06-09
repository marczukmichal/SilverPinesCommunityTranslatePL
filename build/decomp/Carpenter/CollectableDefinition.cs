using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Collectable", menuName = "Collectables/Collectable Definition")]
public class CollectableDefinition : ScriptableObject
{
	[SerializeField]
	private string m_name;

	[SerializeField]
	private AssetReference m_collectableVisualsTemplate;

	[Header("Collectable Visuals")]
	[SerializeField]
	private Sprite m_sprite;

	[SerializeField]
	private string m_firstName;

	[SerializeField]
	private string m_lastName;

	[SerializeField]
	private string m_dateOfBirth;

	[SerializeField]
	private string m_sex;

	[SerializeField]
	private string m_occupation;

	public string CollectableName => m_name;

	public AssetReference CollectableVisualsTemplate => m_collectableVisualsTemplate;

	public Sprite Sprite => m_sprite;

	public string FirstName => m_firstName;

	public string LastName => m_lastName;

	public string DateOfBirth => m_dateOfBirth;

	public string Sex => m_sex;

	public string Occupation => m_occupation;
}
