using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class VrProcessIdentity(int Pid, long StartedUtcTicks, string Executable) : IEquatable<VrProcessIdentity>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(VrProcessIdentity);
		}
	}

	public int Pid { get; init; }

	public long StartedUtcTicks { get; init; }

	public string Executable { get; init; }

	public VrProcessIdentity(int Pid, long StartedUtcTicks, string Executable)
	{
		this.Pid = Pid;
		this.StartedUtcTicks = StartedUtcTicks;
		this.Executable = Executable;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"VrProcessIdentity");
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
		P_0.Append(_"Pid = ");
		P_0.Append(Pid.ToString());
		P_0.Append(_", StartedUtcTicks = ");
		P_0.Append(StartedUtcTicks.ToString());
		P_0.Append(_", Executable = ");
		P_0.Append((object?)Executable);
		return true;
	}

		public static bool operator !=(VrProcessIdentity? left, VrProcessIdentity? right)
	{
		return !(left == right);
	}

		public static bool operator ==(VrProcessIdentity? left, VrProcessIdentity? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(Pid)) * -1521134295 + EqualityComparer<long>.Default.GetHashCode(StartedUtcTicks)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Executable);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as VrProcessIdentity);
	}

		public bool Equals(VrProcessIdentity? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<int>.Default.Equals(Pid, other.Pid) && EqualityComparer<long>.Default.Equals(StartedUtcTicks, other.StartedUtcTicks))
			{
				return EqualityComparer<string>.Default.Equals(Executable, other.Executable);
			}
			return false;
		}
		return true;
	}

		private VrProcessIdentity(VrProcessIdentity original)
	{
		Pid = original.Pid;
		StartedUtcTicks = original.StartedUtcTicks;
		Executable = original.Executable;
	}

		public void Deconstruct(out int Pid, out long StartedUtcTicks, out string Executable)
	{
		Pid = this.Pid;
		StartedUtcTicks = this.StartedUtcTicks;
		Executable = this.Executable;
	}
}
