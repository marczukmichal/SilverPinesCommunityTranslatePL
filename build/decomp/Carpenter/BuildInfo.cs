using System;
using System.IO;
using UnityEngine;

public static class BuildInfo
{
	private static int s_buildNumber;

	private static string s_buildDateTime;

	private static string s_buildInfoPath = Application.streamingAssetsPath + "/BuildInfo.txt";

	public static int Number
	{
		get
		{
			if (s_buildNumber == 0)
			{
				Populate();
			}
			return s_buildNumber;
		}
	}

	public static string NumberString
	{
		get
		{
			if (BuildVersion.s_teamCityBuild)
			{
				return "";
			}
			if (s_buildNumber == 0)
			{
				Populate();
			}
			return s_buildNumber.ToString();
		}
	}

	public static string DateString
	{
		get
		{
			if (BuildVersion.s_teamCityBuild)
			{
				return "";
			}
			if (s_buildNumber == 0)
			{
				Populate();
			}
			return s_buildDateTime;
		}
	}

	public static string VerboseString => NumberString + " - " + DateString;

	private static void Populate()
	{
		string[] array = File.ReadAllLines(s_buildInfoPath);
		int result = -1;
		long result2 = -1L;
		if (array.Length >= 2)
		{
			int.TryParse(array[0], out result);
			long.TryParse(array[1], out result2);
		}
		s_buildNumber = result;
		if (result2 > 0)
		{
			s_buildDateTime = new DateTime(result2).ToString();
		}
		else
		{
			s_buildDateTime = "";
		}
	}
}
