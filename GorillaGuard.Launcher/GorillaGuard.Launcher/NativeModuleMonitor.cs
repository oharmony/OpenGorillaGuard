using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace GorillaGuard.Launcher;

internal sealed class NativeModuleMonitor
{
	private readonly HashSet<string> _reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal NativeModuleScan A(IEnumerable<string> P_0, Func<string, LoaderFinding?> inspect, Func<string, bool> P_2, Action<string> P_3)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		LoaderFinding loaderFinding = null;
		foreach (string item in P_0)
		{
			if (string.IsNullOrWhiteSpace(item) || !hashSet.Add(item))
			{
				continue;
			}
			try
			{
				if ((object)loaderFinding == null)
				{
					loaderFinding = inspect(item);
				}
				bool flag = P_2(item);
				if (_reported.Add(item))
				{
					P_3(item);
					if (flag)
					{
						list.Add(item);
					}
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is Win32Exception) ? 1 : 0) != 0)
			{
				list2.Add(item + _": " + ex.Message);
			}
		}
		_reported.IntersectWith(hashSet);
		return new NativeModuleScan(list, loaderFinding, list2);
	}
}
