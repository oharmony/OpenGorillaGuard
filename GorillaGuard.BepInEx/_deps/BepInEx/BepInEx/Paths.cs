using System.IO;
using System.Linq;
using System.Reflection;
using MonoMod.Utils;

namespace BepInEx;

public static class Paths
{

	public static string[] DllSearchPaths { get; private set; }

	public static string BepInExAssemblyDirectory { get; private set; }

	public static string BepInExAssemblyPath { get; private set; }

	public static string BepInExRootPath { get; private set; }

	public static string ExecutablePath { get; private set; }

	public static string GameRootPath { get; private set; }

	public static string ManagedPath { get; private set; }

	public static string ConfigPath { get; private set; }

	public static string BepInExConfigPath { get; private set; }

	public static string CachePath { get; private set; }

	public static string PatcherPluginPath { get; private set; }

	public static string PluginPath { get; private set; }

	public static string ProcessName { get; private set; }

	internal static void SetExecutablePath(string executablePath, string bepinRootPath = null, string managedPath = null, string[] dllSearchPath = null)
	{
		ExecutablePath = executablePath;
		ProcessName = Path.GetFileNameWithoutExtension(executablePath);
		GameRootPath = (PlatformHelper.Is(Platform.MacOS) ? Utility.ParentDirectory(executablePath, 4) : Path.GetDirectoryName(executablePath));
		ManagedPath = managedPath ?? Utility.CombinePaths(GameRootPath, ProcessName + "_Data", "Managed");
		BepInExRootPath = bepinRootPath ?? Path.Combine(GameRootPath, "BepInEx");
		ConfigPath = Path.Combine(BepInExRootPath, "config");
		BepInExConfigPath = Path.Combine(ConfigPath, "BepInEx.cfg");
		PluginPath = Path.Combine(BepInExRootPath, "plugins");
		PatcherPluginPath = Path.Combine(BepInExRootPath, "patchers");
		BepInExAssemblyDirectory = Path.Combine(BepInExRootPath, "core");
		BepInExAssemblyPath = Path.Combine(BepInExAssemblyDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".dll");
		CachePath = Path.Combine(BepInExRootPath, "cache");
		DllSearchPaths = (dllSearchPath ?? new string[0]).Concat(new string[1] { ManagedPath }).Distinct().ToArray();
	}

	internal static void SetManagedPath(string managedPath)
	{
		if (managedPath != null)
		{
			ManagedPath = managedPath;
		}
	}

	internal static void SetPluginPath(string pluginPath)
	{
		PluginPath = Utility.CombinePaths(BepInExRootPath, pluginPath);
	}
}
