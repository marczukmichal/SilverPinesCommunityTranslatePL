using System.Collections;
using TMPro;
using UnityEngine;

public class ComputerTextFileViewer : MonoBehaviour, IFileViewer
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Vector2Int m_updateImageEveryNTicks = new Vector2Int(1, 4);

	private bool m_isLoading;

	public void AttachFile(ComputerFile file)
	{
		ComputerFileText computerFileText = file as ComputerFileText;
		if ((bool)computerFileText)
		{
			m_text.text = computerFileText.Text;
			m_text.maxVisibleCharacters = 0;
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
		desktop.AddProcess();
		int requiredProcessing = m_text.text.Length / 4;
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
		}
		UpdateImage(1f);
		m_isLoading = false;
		desktop.RemoveProcess();
	}

	private void UpdateImage(float percentLoaded)
	{
		m_text.maxVisibleCharacters = Mathf.RoundToInt((float)m_text.text.Length * percentLoaded);
	}

	public int GetMemoryUsage()
	{
		return m_text.maxVisibleCharacters * 2;
	}
}
