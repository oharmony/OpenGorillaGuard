using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal static class InjectionPolicy
{
	internal const string AbsenseSha256 = "DA9816B556B81B2444C12D42E67D5C25FB51F06E6BF884E553733770438775B3";

	internal const long AbsenseLength = 9175040L;

	internal static bool A(IEnumerable<string> P_0, IEnumerable<string> P_1)
	{
		HashSet<string> hashSet = new HashSet<string>(P_0, StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(P_1, StringComparer.Ordinal);
		if (hashSet.Contains(_"Absense.Injector") && hashSet.Contains(_"Absense.Memory") && hashSet.Contains(_"Absense.Native") && hashSet2.Contains(_"WriteProcessMemory") && hashSet2.Contains(_"VirtualAllocEx"))
		{
			return hashSet2.Contains(_"CreateRemoteThread");
		}
		return false;
	}

	internal static bool A(bool P_0, bool P_1, bool P_2)
	{
		if (P_0 && P_1)
		{
			return !P_2;
		}
		return false;
	}
}
