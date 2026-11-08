using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class HostRecoveryPlan
{
	internal string ExpectedBuildId { get; init; } = _"";

	internal string InstalledBuildId { get; init; } = _"";

	internal bool ManagedInventoryComplete { get; init; }

	internal bool NativeInventoryComplete { get; init; }

	internal List<HostRecoveryItem> Items { get; } = new List<HostRecoveryItem>();

	internal bool HasCompleteInventory
	{
		get
		{
			if (ManagedInventoryComplete)
			{
				return NativeInventoryComplete;
			}
			return false;
		}
	}

	internal bool BuildMatches
	{
		get
		{
			if (ExpectedBuildId.Length > 0)
			{
				return ExpectedBuildId.Equals(InstalledBuildId, StringComparison.Ordinal);
			}
			return false;
		}
	}

	internal bool CanQuarantine
	{
		get
		{
			if (HasCompleteInventory)
			{
				return BuildMatches;
			}
			return false;
		}
	}
}
