using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class HostRecoveryItem(string Path, string RelativePath, HostRecoveryKind Kind, string? ExpectedSha256, string? ActualSha256) : IEquatable<HostRecoveryItem>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(HostRecoveryItem);
		}
	}

	public string Path { get; init; }

	public string RelativePath { get; init; }

	public HostRecoveryKind Kind { get; init; }

	public string? ExpectedSha256 { get; init; }

	public string? ActualSha256 { get; init; }

	public HostRecoveryItem(string Path, string RelativePath, HostRecoveryKind Kind, string? ExpectedSha256, string? ActualSha256)
	{
		this.Path = Path;
		this.RelativePath = RelativePath;
		this.Kind = Kind;
		this.ExpectedSha256 = ExpectedSha256;
		this.ActualSha256 = ActualSha256;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"HostRecoveryItem");
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
		P_0.Append(_"Path = ");
		P_0.Append((object?)Path);
		P_0.Append(_", RelativePath = ");
		P_0.Append((object?)RelativePath);
		P_0.Append(_", Kind = ");
		P_0.Append(Kind.ToString());
		P_0.Append(_", ExpectedSha256 = ");
		P_0.Append((object?)ExpectedSha256);
		P_0.Append(_", ActualSha256 = ");
		P_0.Append((object?)ActualSha256);
		return true;
	}

		public static bool operator !=(HostRecoveryItem? left, HostRecoveryItem? right)
	{
		return !(left == right);
	}

		public static bool operator ==(HostRecoveryItem? left, HostRecoveryItem? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RelativePath)) * -1521134295 + EqualityComparer<HostRecoveryKind>.Default.GetHashCode(Kind)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExpectedSha256)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ActualSha256);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as HostRecoveryItem);
	}

		public bool Equals(HostRecoveryItem? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Path, other.Path) && EqualityComparer<string>.Default.Equals(RelativePath, other.RelativePath) && EqualityComparer<HostRecoveryKind>.Default.Equals(Kind, other.Kind) && EqualityComparer<string>.Default.Equals(ExpectedSha256, other.ExpectedSha256))
			{
				return EqualityComparer<string>.Default.Equals(ActualSha256, other.ActualSha256);
			}
			return false;
		}
		return true;
	}

		private HostRecoveryItem(HostRecoveryItem original)
	{
		Path = original.Path;
		RelativePath = original.RelativePath;
		Kind = original.Kind;
		ExpectedSha256 = original.ExpectedSha256;
		ActualSha256 = original.ActualSha256;
	}

		public void Deconstruct(out string Path, out string RelativePath, out HostRecoveryKind Kind, out string? ExpectedSha256, out string? ActualSha256)
	{
		Path = this.Path;
		RelativePath = this.RelativePath;
		Kind = this.Kind;
		ExpectedSha256 = this.ExpectedSha256;
		ActualSha256 = this.ActualSha256;
	}
}
