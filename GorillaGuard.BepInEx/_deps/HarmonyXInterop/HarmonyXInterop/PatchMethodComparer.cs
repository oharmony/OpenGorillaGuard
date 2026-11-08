using System.Collections.Generic;

namespace HarmonyXInterop;

public class PatchMethodComparer : IEqualityComparer<PatchMethod>
{
	public static PatchMethodComparer Instance { get; } = new PatchMethodComparer();

	public bool Equals(PatchMethod x, PatchMethod y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (object.Equals(x.method, y.method))
		{
			return x.owner == y.owner;
		}
		return false;
	}

	public int GetHashCode(PatchMethod obj)
	{
		return (((obj.method != null) ? obj.method.GetHashCode() : 0) * 397) ^ ((obj.owner != null) ? obj.owner.GetHashCode() : 0);
	}
}
