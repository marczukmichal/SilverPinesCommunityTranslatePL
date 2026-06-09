using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComputerImageFileViewer : MonoBehaviour, IFileViewer
{
	[SerializeField]
	private Image m_image;

	[SerializeField]
	private Vector2Int m_updateImageEveryNTicks = new Vector2Int(5, 15);

	private bool m_isLoading;

	private float m_percentLoaded;

	public void AttachFile(ComputerFile file)
	{
		ComputerFileImage computerFileImage = file as ComputerFileImage;
		if ((bool)computerFileImage)
		{
			m_image.sprite = computerFileImage.Image;
		}
	}

	private void Start()
	{
		StartCoroutine(StartLoadingImage(GetComponent<ComputerWindow>().AttachedDesktop));
	}

	private void OnDisable()
	{
		if (m_isLoading)
		{
			GetComponent<ComputerWindow>().AttachedDesktop.RemoveProcess();
		}
	}

	private IEnumerator StartLoadingImage(ComputerDesktop desktop)
	{
		m_isLoading = true;
		m_percentLoaded = 0f;
		desktop.AddProcess();
		int requiredProcessing = m_image.sprite.texture.width * m_image.sprite.texture.height / 300;
		int amountProcessed = 0;
		int tickUntilUpdateVisuals = 1;
		while (amountProcessed < requiredProcessing)
		{
			yield return new WaitForSecondsRealtime(desktop.TickTime);
			int availableTicks = desktop.GetAvailableTicks();
			amountProcessed += availableTicks;
			tickUntilUpdateVisuals--;
			if (tickUntilUpdateVisuals == 0)
			{
				UpdateImage((float)amountProcessed / (float)requiredProcessing);
				tickUntilUpdateVisuals = Random.Range(m_updateImageEveryNTicks.x, m_updateImageEveryNTicks.y);
			}
			m_percentLoaded = (float)amountProcessed / (float)requiredProcessing;
		}
		UpdateImage(1f);
		m_percentLoaded = 1f;
		m_isLoading = false;
		desktop.RemoveProcess();
	}

	private void UpdateImage(float percentLoaded)
	{
		m_image.fillAmount = percentLoaded;
	}

	public int GetMemoryUsage()
	{
		int num = (int)(m_percentLoaded * (float)m_image.sprite.texture.width * (float)m_image.sprite.texture.height * 4f);
		if (m_isLoading)
		{
			num += Random.Range(0, 100);
		}
		return num;
	}
}
