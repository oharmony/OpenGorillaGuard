using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace BepInEx.Unix;

internal static class ConsoleWriter
{
	private static Func<Stream, Encoding, bool, StreamWriter> cStreamWriterConstructor;

	private static Func<Stream, Encoding, bool, StreamWriter> CStreamWriterConstructor
	{
		get
		{
			if (cStreamWriterConstructor != null)
			{
				return cStreamWriterConstructor;
			}
			Type cStreamWriter = AccessTools.TypeByName("System.IO.CStreamWriter");
			cStreamWriterConstructor = new int[2][]
			{
				new int[3] { 0, 1, 2 },
				new int[2] { 0, 1 }
			}.Select(GetCtor).FirstOrDefault((Func<Stream, Encoding, bool, StreamWriter> f) => f != null);
			if (cStreamWriterConstructor == null)
			{
				throw new AmbiguousMatchException("Failed to find suitable constructor for CStreamWriter");
			}
			return cStreamWriterConstructor;
			Func<Stream, Encoding, bool, StreamWriter> GetCtor(int[] perm)
			{
				Type[] parameters = new Type[3]
				{
					typeof(Stream),
					typeof(Encoding),
					typeof(bool)
				};
				ConstructorInfo ctor = AccessTools.Constructor(cStreamWriter, perm.Select((int i) => parameters[i]).ToArray());
				if (ctor != null)
				{
					return delegate(Stream stream, Encoding encoding, bool l)
					{
						object[] vals = new object[3] { stream, encoding, l };
						return (StreamWriter)ctor.Invoke(perm.Select((int i) => vals[i]).ToArray());
					};
				}
				return null;
			}
		}
	}

	public static TextWriter CreateConsoleStreamWriter(Stream stream, Encoding encoding, bool leaveOpen)
	{
		StreamWriter streamWriter = CStreamWriterConstructor(stream, encoding, leaveOpen);
		streamWriter.AutoFlush = true;
		return streamWriter;
	}
}
