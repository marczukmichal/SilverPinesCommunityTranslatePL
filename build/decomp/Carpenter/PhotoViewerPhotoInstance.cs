using UnityEngine;
using UnityEngine.UI;

public class PhotoViewerPhotoInstance : MonoBehaviour
{
	[SerializeField]
	private RawImage m_image;

	private PhotoData m_photoData;

	public PhotoData PhotoData => m_photoData;

	public void SetPhoto(PhotoData photo)
	{
		if (photo != null)
		{
			base.gameObject.SetActive(value: true);
			m_photoData = photo;
			m_image.texture = photo.Texture;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
