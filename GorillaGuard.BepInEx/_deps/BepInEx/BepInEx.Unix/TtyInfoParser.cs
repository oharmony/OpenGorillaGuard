using System.Linq;
using System.Text;

namespace BepInEx.Unix;

internal static class TtyInfoParser
{
	internal enum TermInfoNumbers
	{
		MaxColors = 13
	}

	internal enum TermInfoStrings
	{
		SetAForeground = 359
	}

	private static readonly int[] ansiColorMapping = new int[16]
	{
		0, 4, 2, 6, 1, 5, 3, 7, 8, 12,
		10, 14, 9, 13, 11, 15
	};

	public static TtyInfo Parse(byte[] buffer)
	{
		int num;
		switch (GetInt16(buffer, 0))
		{
		case 282:
			num = 2;
			break;
		case 542:
			num = 4;
			break;
		default:
			return TtyInfo.Default;
		}
		int @int = GetInt16(buffer, 4);
		GetInt16(buffer, 6);
		GetInt16(buffer, 8);
		int num2 = 12 + GetString(buffer, 12).Length + 1 + @int;
		int offset = num2 + num2 % 2 + num * 13;
		return new TtyInfo
		{
			MaxColors = GetInteger(num, buffer, offset),
			ForegroundColorStrings = ansiColorMapping.Select((int x) => $"\u001b[{((x > 7) ? (82 + x) : (30 + x))}m").ToArray()
		};
	}

	private static int GetInt32(byte[] buffer, int offset)
	{
		return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
	}

	private static short GetInt16(byte[] buffer, int offset)
	{
		return (short)(buffer[offset] | (buffer[offset + 1] << 8));
	}

	private static int GetInteger(int intSize, byte[] buffer, int offset)
	{
		if (intSize != 2)
		{
			return GetInt32(buffer, offset);
		}
		return GetInt16(buffer, offset);
	}

	private static string GetString(byte[] buffer, int offset)
	{
		int i;
		for (i = 0; buffer[offset + i] != 0; i++)
		{
		}
		return Encoding.ASCII.GetString(buffer, offset, i);
	}
}
