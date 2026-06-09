public class NullPlatformDLC : IPlatformDLC
{
	bool IPlatformDLC.HasEntitlementForProduct(DLCProduct product)
	{
		return false;
	}
}
