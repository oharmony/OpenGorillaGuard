using System;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Launcher;

internal sealed class VrProcessContinuity
{
	private VrProcessIdentity? _bound;

	private string? _failure;

	internal VrProcessIdentity? Bound => _bound;

	internal string? A(IReadOnlyList<VrProcessIdentity> P_0)
	{
		if (_failure != null)
		{
			return _failure;
		}
		if (P_0.Count > 1)
		{
			return _failure = _"Multiple SteamVR server processes; restart protected play.";
		}
		if (P_0.Count == 0)
		{
			if (!(_bound == null))
			{
				return _failure = _"The protected SteamVR server exited; restart protected play.";
			}
			return null;
		}
		VrProcessIdentity vrProcessIdentity = P_0[0];
		if (vrProcessIdentity.Pid <= 0 || vrProcessIdentity.StartedUtcTicks <= 0 || !Path.IsPathFullyQualified(vrProcessIdentity.Executable))
		{
			return _failure = _"SteamVR process identity could not be established.";
		}
		if (_bound == null)
		{
			_bound = vrProcessIdentity;
			return null;
		}
		if (_bound.Pid != vrProcessIdentity.Pid || _bound.StartedUtcTicks != vrProcessIdentity.StartedUtcTicks || !string.Equals(_bound.Executable, vrProcessIdentity.Executable, StringComparison.OrdinalIgnoreCase))
		{
			return _failure = _"The protected SteamVR server was replaced; restart protected play.";
		}
		return null;
	}
}
