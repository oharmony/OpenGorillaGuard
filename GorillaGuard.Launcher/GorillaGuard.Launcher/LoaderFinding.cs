using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class LoaderFinding(string Evidence, string Path, int ProcessId = 0) : IEquatable<LoaderFinding>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(LoaderFinding);
		}
	}

	public string Evidence { get; init; }

	public string Path { get; init; }

	public int ProcessId { get; init; }

	public LoaderFinding(string Evidence, string Path, int ProcessId = 0)
	{
		this.Evidence = Evidence;
		this.Path = Path;
		this.ProcessId = ProcessId;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"LoaderFinding");
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
		P_0.Append(_"Evidence = ");
		P_0.Append((object?)Evidence);
		P_0.Append(_", Path = ");
		P_0.Append((object?)Path);
		P_0.Append(_", ProcessId = ");
		P_0.Append(ProcessId.ToString());
		return true;
	}

		public static bool operator !=(LoaderFinding? left, LoaderFinding? right)
	{
		return !(left == right);
	}

		public static bool operator ==(LoaderFinding? left, LoaderFinding? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Evidence)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(ProcessId);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as LoaderFinding);
	}

		public bool Equals(LoaderFinding? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(Evidence, other.Evidence) && EqualityComparer<string>.Default.Equals(Path, other.Path))
			{
				return EqualityComparer<int>.Default.Equals(ProcessId, other.ProcessId);
			}
			return false;
		}
		return true;
	}

		private LoaderFinding(LoaderFinding original)
	{
		Evidence = original.Evidence;
		Path = original.Path;
		ProcessId = original.ProcessId;
	}

		public void Deconstruct(out string Evidence, out string Path, out int ProcessId)
	{
		Evidence = this.Evidence;
		Path = this.Path;
		ProcessId = this.ProcessId;
	}
}
