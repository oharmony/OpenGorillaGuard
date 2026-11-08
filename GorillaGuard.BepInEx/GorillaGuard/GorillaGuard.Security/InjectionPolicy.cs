using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal static class InjectionPolicy
{
	internal const string AbsenseSha256 = "DA9816B556B81B2444C12D42E67D5C25FB51F06E6BF884E553733770438775B3";

	internal const long AbsenseLength = 9175040L;

	internal static bool IsAbsenseInjectorPattern(IEnumerable<string> assemblyNames, IEnumerable<string> importNames)
	{
		HashSet<string> hashSet = new HashSet<string>(assemblyNames, StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(importNames, StringComparer.Ordinal);
		if (hashSet.Contains("Absense.Injector") && hashSet.Contains("Absense.Memory") && hashSet.Contains("Absense.Native") && hashSet2.Contains("WriteProcessMemory") && hashSet2.Contains("VirtualAllocEx"))
		{
			return hashSet2.Contains("CreateRemoteThread");
		}
		return false;
	}

	internal static bool ShouldQuarantineAbsensePayload(bool shaMatches, bool lengthMatches, bool previouslyApproved)
	{
		if (shaMatches && lengthMatches)
		{
			return !previouslyApproved;
		}
		return false;
	}
}
