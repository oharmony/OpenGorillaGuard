using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GorillaGuard.Launcher;

internal sealed class VrCodeWatcher : IDisposable
{
	private readonly FileSystemWatcher _watcher;

	private string? _failure;

	internal string? Failure => Volatile.Read(in _failure);

	internal VrCodeWatcher(string root)
	{
		_watcher = new FileSystemWatcher(root)
		{
			IncludeSubdirectories = true,
			NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite)
		};
		_watcher.Created += A;
		_watcher.Changed += A;
		_watcher.Deleted += A;
		_watcher.Renamed += delegate(object P_0, RenamedEventArgs P_1)
		{
			A(P_1.OldFullPath);
			A(P_1.FullPath);
		};
		_watcher.Error += delegate
		{
			Interlocked.CompareExchange(ref _failure, _"VR code watcher lost events; restart protection.", null);
		};
		_watcher.EnableRaisingEvents = true;
	}

	private void A(object P_0, FileSystemEventArgs P_1)
	{
		A(P_1.FullPath);
	}

	private void A(string P_0)
	{
		string text = Path.GetExtension(P_0).ToLowerInvariant();
		if ((text == _".dll" || text == _".exe" || text == _".vrdrivermanifest") ? true : false)
		{
			Interlocked.CompareExchange(ref _failure, _"VR code or manifest changed during the protected session: " + P_0, null);
		}
	}

	public void Dispose()
	{
		_watcher.Dispose();
	}

		private void A(object P_0, RenamedEventArgs P_1)
	{
		A(P_1.OldFullPath);
		A(P_1.FullPath);
	}

		private void A(object P_0, ErrorEventArgs P_1)
	{
		Interlocked.CompareExchange(ref _failure, _"VR code watcher lost events; restart protection.", null);
	}
}
