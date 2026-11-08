using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace BepInEx.Preloader.RuntimeFixes;

internal static class TraceFix
{
	private static Type TraceImplType;

	private static object ListenersSyncRoot;

	private static TraceListenerCollection Listeners;

	private static PropertyInfo prop_AutoFlush;

	private static bool AutoFlush => (bool)prop_AutoFlush.GetValue(null, null);

	public static void ApplyFix()
	{
		TraceImplType = AppDomain.CurrentDomain.GetAssemblies().First((Assembly x) => x.GetName().Name == "System").GetTypes()
			.FirstOrDefault((Type x) => x.Name == "TraceImpl");
		if (TraceImplType != null)
		{
			ListenersSyncRoot = AccessTools.Property(TraceImplType, "ListenersSyncRoot").GetValue(null, null);
			Listeners = (TraceListenerCollection)AccessTools.Property(TraceImplType, "Listeners").GetValue(null, null);
			prop_AutoFlush = AccessTools.Property(TraceImplType, "AutoFlush");
			new Harmony("com.bepis.bepinex.tracefix").Patch(typeof(Trace).GetMethod("DoTrace", BindingFlags.Static | BindingFlags.NonPublic), new HarmonyMethod(typeof(TraceFix).GetMethod("DoTraceReplacement", BindingFlags.Static | BindingFlags.NonPublic)));
		}
	}

	private static bool DoTraceReplacement(string kind, Assembly report, string message)
	{
		string source = string.Empty;
		try
		{
			source = report.GetName().Name;
		}
		catch (MethodAccessException)
		{
		}
		TraceEventType eventType = (TraceEventType)Enum.Parse(typeof(TraceEventType), kind);
		lock (ListenersSyncRoot)
		{
			foreach (TraceListener listener in Listeners)
			{
				listener.TraceEvent(new TraceEventCache(), source, eventType, 0, message);
				if (AutoFlush)
				{
					listener.Flush();
				}
			}
		}
		return false;
	}
}
