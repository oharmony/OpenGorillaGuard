using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class HostQuarantineMove(string Source, string Destination, HostRecoveryKind Kind) : IEquatable<HostQuarantineMove>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(HostQuarantineMove);
		}
	}

	public string Source { get; init; }

	public string Destination { get; init; }

	public HostRecoveryKind Kind { get; init; }

	public HostQuarantineMove(string Source, string Destination, HostRecoveryKind Kind)
	{
		this.Source = Source;
		this.Destination = Destination;
		this.Kind = Kind;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"HostQuarantineMove");
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
		P_0.Append(_"Source = ");
		P_0.Append((object?)Source);
		P_0.Append(_", Destination = ");
		P_0.Append((object?)Destination);
		P_0.Append(_", Kind = ");
		P_0.Append(Kind.ToString());
		return true;
	}

		public static bool operator !=(HostQuarantineMove? left, HostQuarantineMove? right)
	{
		return !(left == right);
	}

		public static bool operator ==(HostQuarantineMove? left, HostQuarantineMove? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Destination)) * -1521134295 + EqualityComparer<HostRecoveryKind>.Default.GetHashCode(Kind);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as HostQuarantineMove);
	}

		public bool Equals(HostQuarantineMove? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Source, other.Source) && EqualityComparer<string>.Default.Equals(Destination, other.Destination))
			{
				return EqualityComparer<HostRecoveryKind>.Default.Equals(Kind, other.Kind);
			}
			return false;
		}
		return true;
	}

		private HostQuarantineMove(HostQuarantineMove original)
	{
		Source = original.Source;
		Destination = original.Destination;
		Kind = original.Kind;
	}

		public void Deconstruct(out string Source, out string Destination, out HostRecoveryKind Kind)
	{
		Source = this.Source;
		Destination = this.Destination;
		Kind = this.Kind;
	}
}
