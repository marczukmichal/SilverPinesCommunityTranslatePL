using System;

[Flags]
public enum MapIconState : short
{
	None = 0,
	Discovered = 1,
	ItemRevealed = 2,
	DoorLocked = 4,
	DoorOpen = 8,
	Cleared = 0x10
}
