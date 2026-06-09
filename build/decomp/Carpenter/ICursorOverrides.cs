public interface ICursorOverrides
{
	public enum CursorOverrideOption
	{
		Unchanged,
		ForceOn,
		ForceOff
	}

	CursorOverrideOption ShouldShowCursor { get; }
}
