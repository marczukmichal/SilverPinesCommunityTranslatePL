using System;

[Flags]
public enum MapObjectiveState
{
	None = 0,
	Motel = 1,
	Ferry = 2,
	CableCarStation = 4,
	Sheriffs = 8,
	Bells = 0x10,
	Peak = 0x20
}
