using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib.Tools;

namespace BepInEx.Logging;

internal class HarmonyLogSource : ILogSource, IDisposable
{
	private static readonly ConfigEntry<HarmonyLib.Tools.Logger.LogChannel> LogChannels = ConfigFile.CoreConfig.Bind("Harmony.Logger", "LogChannels", HarmonyLib.Tools.Logger.LogChannel.Warn | HarmonyLib.Tools.Logger.LogChannel.Error, "Specifies which Harmony log channels to listen to.\nNOTE: IL channel dumps the whole patch methods, use only when needed!");

	private static readonly Dictionary<HarmonyLib.Tools.Logger.LogChannel, LogLevel> LevelMap = new Dictionary<HarmonyLib.Tools.Logger.LogChannel, LogLevel>
	{
		[HarmonyLib.Tools.Logger.LogChannel.Info] = LogLevel.Info,
		[HarmonyLib.Tools.Logger.LogChannel.Warn] = LogLevel.Warning,
		[HarmonyLib.Tools.Logger.LogChannel.Error] = LogLevel.Error,
		[HarmonyLib.Tools.Logger.LogChannel.IL] = LogLevel.Debug
	};

	public string SourceName { get; } = "HarmonyX";

	public event EventHandler<LogEventArgs> LogEvent;

	public HarmonyLogSource()
	{
		HarmonyLib.Tools.Logger.ChannelFilter = LogChannels.Value;
		HarmonyLib.Tools.Logger.MessageReceived += HandleHarmonyMessage;
	}

	private void HandleHarmonyMessage(object sender, HarmonyLib.Tools.Logger.LogEventArgs e)
	{
		if (LevelMap.TryGetValue(e.LogChannel, out var value))
		{
			this.LogEvent?.Invoke(this, new LogEventArgs(e.Message, value, this));
		}
	}

	public void Dispose()
	{
		HarmonyLib.Tools.Logger.MessageReceived -= HandleHarmonyMessage;
	}
}
