using System;
using System.Text;

public class DeterministicCodeGenerator
{
	[Serializable]
	public struct Settings
	{
		public int m_length;

		public int m_id;
	}

	private const string m_numbers = "0123456789";

	public static string GetNumbersCode(int length, int id)
	{
		Random random = new Random(CreateStableSeed(GlobalReferences.Instance.DataStore.Data.RandomCodeSeed, id));
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			int index = random.Next("0123456789".Length);
			stringBuilder.Append("0123456789"[index]);
		}
		return stringBuilder.ToString();
	}

	public static string GetNumbersCode(Settings settings)
	{
		return GetNumbersCode(settings.m_length, settings.m_id);
	}

	private static int CreateStableSeed(int seed, int id)
	{
		return (seed * id + id * 2) % seed;
	}
}
