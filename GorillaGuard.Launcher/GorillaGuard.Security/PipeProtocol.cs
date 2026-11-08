using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GorillaGuard.Security;

internal static class PipeProtocol
{
	internal static async Task<string?> A(StreamReader P_0, int P_1, TimeSpan P_2, CancellationToken P_3)
	{
		using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(P_3);
		deadline.CancelAfter(P_2);
		StringBuilder line = new StringBuilder(Math.Min(P_1, 256));
		char[] character = new char[1];
		try
		{
			while (await P_0.ReadAsync(character.AsMemory(), deadline.Token) != 0)
			{
				if (character[0] == '\n')
				{
					return (line.Length > 0 && line[line.Length - 1] == '\r') ? line.ToString(0, line.Length - 1) : line.ToString();
				}
				if (line.Length >= P_1)
				{
					throw new InvalidDataException(_"Pipe message exceeds the protocol size limit.");
				}
				line.Append(character[0]);
			}
			return (line.Length == 0) ? null : line.ToString();
		}
		catch (OperationCanceledException) when (!P_3.IsCancellationRequested)
		{
			throw new TimeoutException(_"The pipe client did not finish its message before the deadline.");
		}
	}
}
