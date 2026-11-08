using System;

namespace BepInEx.Logging;

[Flags]
public enum LogLevel
{

	None = 0,

	Fatal = 1,

	Error = 2,

	Warning = 4,

	Message = 8,

	Info = 0x10,

	Debug = 0x20,

	All = 0x3F
}
