using System;
using System.Collections.Generic;
using System.IO;
using MonoMod.Utils;

namespace BepInEx.Unix;

internal static class UnixStreamHelper
{
	public delegate int dupDelegate(int fd);

	public delegate IntPtr fdopenDelegate(int fd, string mode);

	public delegate IntPtr freadDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

	public delegate int fwriteDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

	public delegate int fcloseDelegate(IntPtr stream);

	public delegate int fflushDelegate(IntPtr stream);

	public delegate int isattyDelegate(int fd);

	[DynDllImport("libc", new string[] { })]
	public static dupDelegate dup;

	[DynDllImport("libc", new string[] { })]
	public static fdopenDelegate fdopen;

	[DynDllImport("libc", new string[] { })]
	public static freadDelegate fread;

	[DynDllImport("libc", new string[] { })]
	public static fwriteDelegate fwrite;

	[DynDllImport("libc", new string[] { })]
	public static fcloseDelegate fclose;

	[DynDllImport("libc", new string[] { })]
	public static fflushDelegate fflush;

	[DynDllImport("libc", new string[] { })]
	public static isattyDelegate isatty;

	static UnixStreamHelper()
	{
		Dictionary<string, List<DynDllMapping>> mappings = new Dictionary<string, List<DynDllMapping>> { ["libc"] = new List<DynDllMapping> { "libc.so.6", "libc", "/usr/lib/libSystem.dylib" } };
		typeof(UnixStreamHelper).ResolveDynDllImports(mappings);
	}

	public static Stream CreateDuplicateStream(int fileDescriptor)
	{
		return new UnixStream(dup(fileDescriptor), FileAccess.Write);
	}
}
