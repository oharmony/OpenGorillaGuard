using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class EvasionFinding(string Kind, string Evidence, int ProcessId = 0, string Path = "") : IEquatable<EvasionFinding>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(EvasionFinding);
		}
	}

	public string Kind { get; init; }

	public string Evidence { get; init; }

	public int ProcessId { get; init; }

	public string Path { get; init; }

	public EvasionFinding(string Kind, string Evidence, int ProcessId = 0, string Path = "")
	{
		this.Kind = Kind;
		this.Evidence = Evidence;
		this.ProcessId = ProcessId;
		this.Path = Path;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"EvasionFinding");
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
		P_0.Append(_"Kind = ");
		P_0.Append((object?)Kind);
		P_0.Append(_", Evidence = ");
		P_0.Append((object?)Evidence);
		P_0.Append(_", ProcessId = ");
		P_0.Append(ProcessId.ToString());
		P_0.Append(_", Path = ");
		P_0.Append((object?)Path);
		return true;
	}

		public static bool operator !=(EvasionFinding? left, EvasionFinding? right)
	{
		return !(left == right);
	}

		public static bool operator ==(EvasionFinding? left, EvasionFinding? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Kind)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Evidence)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(ProcessId)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as EvasionFinding);
	}

		public bool Equals(EvasionFinding? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Kind, other.Kind) && EqualityComparer<string>.Default.Equals(Evidence, other.Evidence) && EqualityComparer<int>.Default.Equals(ProcessId, other.ProcessId))
			{
				return EqualityComparer<string>.Default.Equals(Path, other.Path);
			}
			return false;
		}
		return true;
	}

		private EvasionFinding(EvasionFinding original)
	{
		Kind = original.Kind;
		Evidence = original.Evidence;
		ProcessId = original.ProcessId;
		Path = original.Path;
	}

		public void Deconstruct(out string Kind, out string Evidence, out int ProcessId, out string Path)
	{
		Kind = this.Kind;
		Evidence = this.Evidence;
		ProcessId = this.ProcessId;
		Path = this.Path;
	}
}
