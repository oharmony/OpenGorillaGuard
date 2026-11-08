using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityInjector.ConsoleUtil;

internal class ConsoleEncoding : Encoding
{
	private byte[] _byteBuffer = new byte[256];

	private char[] _charBuffer = new char[256];

	private byte[] _zeroByte = new byte[0];

	private char[] _zeroChar = new char[0];

	private readonly uint _codePage;

	public override int CodePage => (int)_codePage;

	public static Encoding OutputEncoding => new ConsoleEncoding(ConsoleCodePage);

	public static uint ConsoleCodePage
	{
		get
		{
			return GetConsoleOutputCP();
		}
		set
		{
			SetConsoleOutputCP(value);
		}
	}

	private void ExpandByteBuffer(int count)
	{
		if (_byteBuffer.Length < count)
		{
			_byteBuffer = new byte[count];
		}
	}

	private void ExpandCharBuffer(int count)
	{
		if (_charBuffer.Length < count)
		{
			_charBuffer = new char[count];
		}
	}

	private void ReadByteBuffer(byte[] bytes, int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			bytes[index + i] = _byteBuffer[i];
		}
	}

	private void ReadCharBuffer(char[] chars, int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			chars[index + i] = _charBuffer[i];
		}
	}

	private void WriteByteBuffer(byte[] bytes, int index, int count)
	{
		ExpandByteBuffer(count);
		for (int i = 0; i < count; i++)
		{
			_byteBuffer[i] = bytes[index + i];
		}
	}

	private void WriteCharBuffer(char[] chars, int index, int count)
	{
		ExpandCharBuffer(count);
		for (int i = 0; i < count; i++)
		{
			_charBuffer[i] = chars[index + i];
		}
	}

	public static uint GetActiveCodePage()
	{
		return GetACP();
	}

	private ConsoleEncoding(uint codePage)
	{
		_codePage = codePage;
	}

	public static ConsoleEncoding GetEncoding(uint codePage)
	{
		return new ConsoleEncoding(codePage);
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		WriteCharBuffer(chars, index, count);
		return WideCharToMultiByte(_codePage, 0u, _charBuffer, count, _zeroByte, 0, IntPtr.Zero, IntPtr.Zero);
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		int byteCount = GetByteCount(chars, charIndex, charCount);
		WriteCharBuffer(chars, charIndex, charCount);
		ExpandByteBuffer(byteCount);
		int result = WideCharToMultiByte(_codePage, 0u, chars, charCount, _byteBuffer, byteCount, IntPtr.Zero, IntPtr.Zero);
		ReadByteBuffer(bytes, byteIndex, byteCount);
		return result;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		WriteByteBuffer(bytes, index, count);
		return MultiByteToWideChar(_codePage, 0u, bytes, count, _zeroChar, 0);
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		int charCount = GetCharCount(bytes, byteIndex, byteCount);
		WriteByteBuffer(bytes, byteIndex, byteCount);
		ExpandCharBuffer(charCount);
		int result = MultiByteToWideChar(_codePage, 0u, bytes, byteCount, _charBuffer, charCount);
		ReadCharBuffer(chars, charIndex, charCount);
		return result;
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount * 2;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}

	[DllImport("kernel32.dll")]
	private static extern uint GetConsoleOutputCP();

	[DllImport("kernel32.dll")]
	private static extern uint GetACP();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int MultiByteToWideChar(uint codePage, uint dwFlags, [In][MarshalAs(UnmanagedType.LPArray)] byte[] lpMultiByteStr, int cbMultiByte, [Out][MarshalAs(UnmanagedType.LPWStr)] char[] lpWideCharStr, int cchWideChar);

	[DllImport("kernel32.dll")]
	private static extern IntPtr SetConsoleOutputCP(uint codepage);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int WideCharToMultiByte(uint codePage, uint dwFlags, [In][MarshalAs(UnmanagedType.LPWStr)] char[] lpWideCharStr, int cchWideChar, [Out][MarshalAs(UnmanagedType.LPArray)] byte[] lpMultiByteStr, int cbMultiByte, IntPtr lpDefaultChar, IntPtr lpUsedDefaultChar);
}
