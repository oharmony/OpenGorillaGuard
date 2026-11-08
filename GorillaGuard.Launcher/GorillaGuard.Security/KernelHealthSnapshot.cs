using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class KernelHealthSnapshot(bool DevelopmentOnly, ulong FilteredOperations) : IEquatable<KernelHealthSnapshot>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(KernelHealthSnapshot);
		}
	}

	public bool DevelopmentOnly { get; init; }

	public ulong FilteredOperations { get; init; }

	public KernelHealthSnapshot(bool DevelopmentOnly, ulong FilteredOperations)
	{
		this.DevelopmentOnly = DevelopmentOnly;
		this.FilteredOperations = FilteredOperations;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"KernelHealthSnapshot");
		stringBuilder.Append(_" { ");
		if (A(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

		private bool A(StringBuilder P_0)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		P_0.Append(_"DevelopmentOnly = ");
		P_0.Append(DevelopmentOnly.ToString());
		P_0.Append(_", FilteredOperations = ");
		P_0.Append(FilteredOperations.ToString());
		return true;
	}

		public static bool operator !=(KernelHealthSnapshot? left, KernelHealthSnapshot? right)
	{
		return !(left == right);
	}

		public static bool operator ==(KernelHealthSnapshot? left, KernelHealthSnapshot? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(DevelopmentOnly)) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(FilteredOperations);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as KernelHealthSnapshot);
	}

		public bool Equals(KernelHealthSnapshot? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<bool>.Default.Equals(DevelopmentOnly, other.DevelopmentOnly))
			{
				return EqualityComparer<ulong>.Default.Equals(FilteredOperations, other.FilteredOperations);
			}
			return false;
		}
		return true;
	}

		private KernelHealthSnapshot(KernelHealthSnapshot original)
	{
		DevelopmentOnly = original.DevelopmentOnly;
		FilteredOperations = original.FilteredOperations;
	}

		public void Deconstruct(out bool DevelopmentOnly, out ulong FilteredOperations)
	{
		DevelopmentOnly = this.DevelopmentOnly;
		FilteredOperations = this.FilteredOperations;
	}
}
