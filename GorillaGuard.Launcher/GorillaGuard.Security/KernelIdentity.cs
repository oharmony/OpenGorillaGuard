using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Security;

internal sealed class KernelIdentity(ulong Pid, ulong StartedFileTime) : IEquatable<KernelIdentity>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(KernelIdentity);
		}
	}

	public ulong Pid { get; init; }

	public ulong StartedFileTime { get; init; }

	public KernelIdentity(ulong Pid, ulong StartedFileTime)
	{
		this.Pid = Pid;
		this.StartedFileTime = StartedFileTime;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"KernelIdentity");
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
		P_0.Append(_", StartedFileTime = ");
		P_0.Append(StartedFileTime.ToString());
		return true;
	}

		public static bool operator !=(KernelIdentity? left, KernelIdentity? right)
	{
		return !(left == right);
	}

		public static bool operator ==(KernelIdentity? left, KernelIdentity? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(Pid)) * -1521134295 + EqualityComparer<ulong>.Default.GetHashCode(StartedFileTime);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as KernelIdentity);
	}

		public bool Equals(KernelIdentity? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<ulong>.Default.Equals(Pid, other.Pid))
			{
				return EqualityComparer<ulong>.Default.Equals(StartedFileTime, other.StartedFileTime);
			}
			return false;
		}
		return true;
	}

		private KernelIdentity(KernelIdentity original)
	{
		Pid = original.Pid;
		StartedFileTime = original.StartedFileTime;
	}

		public void Deconstruct(out ulong Pid, out ulong StartedFileTime)
	{
		Pid = this.Pid;
		StartedFileTime = this.StartedFileTime;
	}
}
