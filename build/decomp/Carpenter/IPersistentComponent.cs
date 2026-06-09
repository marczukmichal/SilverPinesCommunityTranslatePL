public interface IPersistentComponent
{
	void ReceiveDataStoreEntry(PersistentDataObject dataEntry);

	void PostAllReceivedDatastoreEntries()
	{
	}

	bool RequiresPersistentData();
}
