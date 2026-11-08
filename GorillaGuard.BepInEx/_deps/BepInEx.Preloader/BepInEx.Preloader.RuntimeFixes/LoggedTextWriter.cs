using System.IO;
using System.Text;

namespace BepInEx.Preloader.RuntimeFixes;

internal class LoggedTextWriter : TextWriter
{
	public override Encoding Encoding { get; } = Encoding.UTF8;

	public TextWriter Parent { get; set; }

	public override void Flush()
	{
		Parent.Flush();
	}

	public override void Write(string value)
	{
		ConsoleSetOutFix.ConsoleLogSource.LogInfo(value);
		Parent.Write(value);
	}

	public override void WriteLine(string value)
	{
		ConsoleSetOutFix.ConsoleLogSource.LogInfo(value);
		Parent.WriteLine(value);
	}
}
