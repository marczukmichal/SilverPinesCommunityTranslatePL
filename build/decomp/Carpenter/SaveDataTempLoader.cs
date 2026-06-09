public class SaveDataTempLoader
{
	private ISaveDataSerializer m_serialiser;

	private bool m_isDone;

	private PersistentData m_persistentData;

	private ResultCode m_resultCode;

	private int m_profileIndex;

	private int m_slotIndex;

	public PersistentData ResultPersistentData => m_persistentData;

	public ResultCode ResultCode => m_resultCode;

	public bool IsDone => m_isDone;

	public SaveDataTempLoader(ISaveDataSerializer serialiser, int profileIndex, int slotIndex)
	{
		m_serialiser = serialiser;
		m_profileIndex = profileIndex;
		m_slotIndex = slotIndex;
	}

	public void LoadData()
	{
		m_isDone = false;
		m_serialiser.LoadPersistentData(m_profileIndex, m_slotIndex, null, OnLoadedCallback);
	}

	private void OnLoadedCallback(DataLoadedCallbackEvent<PersistentData> result)
	{
		m_isDone = true;
		m_resultCode = result.m_resultCode;
		m_persistentData = result.m_loadedObject;
	}
}
