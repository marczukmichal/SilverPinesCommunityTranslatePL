public interface IItemPickup
{
	ItemDefinition ItemDefinition { get; }

	int ItemAmount { get; set; }

	int AmountInItem { get; }

	LootType LootType { get; }

	bool SuppressNotification { get; }
}
