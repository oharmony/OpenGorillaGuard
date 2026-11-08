using System;
using System.Threading.Tasks;

namespace GorillaGuard.Launcher;

internal sealed class RemoteClient : IDisposable
{
	internal bool Enabled => false;

	internal RemoteClient(RemoteSettings settings, string gameRoot)
	{
	}

	internal Task<bool> A(string P_0)
	{
		return Task.FromResult(result: false);
	}

	internal void A(string P_0, string P_1, string P_2)
	{
	}

	internal Task A()
	{
		return Task.CompletedTask;
	}

	public void Dispose()
	{
	}
}
