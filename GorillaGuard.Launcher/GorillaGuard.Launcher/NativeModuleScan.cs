using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GorillaGuard.Launcher;

internal sealed class NativeModuleScan(IReadOnlyList<string> SuspiciousPaths, LoaderFinding? Loader, IReadOnlyList<string> Errors) : IEquatable<NativeModuleScan>
{
		private Type EqualityContract
	{
				get
		{
			return typeof(NativeModuleScan);
		}
	}

	public IReadOnlyList<string> SuspiciousPaths { get; init; }

	public LoaderFinding? Loader { get; init; }

	public IReadOnlyList<string> Errors { get; init; }

	public NativeModuleScan(IReadOnlyList<string> SuspiciousPaths, LoaderFinding? Loader, IReadOnlyList<string> Errors)
	{
		this.SuspiciousPaths = SuspiciousPaths;
		this.Loader = Loader;
		this.Errors = Errors;
		base._002Ector();
	}

		public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(_"NativeModuleScan");
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
		P_0.Append(_"SuspiciousPaths = ");
		P_0.Append(SuspiciousPaths);
		P_0.Append(_", Loader = ");
		P_0.Append(Loader);
		P_0.Append(_", Errors = ");
		P_0.Append(Errors);
		return true;
	}

		public static bool operator !=(NativeModuleScan? left, NativeModuleScan? right)
	{
		return !(left == right);
	}

		public static bool operator ==(NativeModuleScan? left, NativeModuleScan? right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

		public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(SuspiciousPaths)) * -1521134295 + EqualityComparer<LoaderFinding>.Default.GetHashCode(Loader)) * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(Errors);
	}

		public override bool Equals(object? obj)
	{
		return Equals(obj as NativeModuleScan);
	}

		public bool Equals(NativeModuleScan? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<IReadOnlyList<string>>.Default.Equals(SuspiciousPaths, other.SuspiciousPaths) && EqualityComparer<LoaderFinding>.Default.Equals(Loader, other.Loader))
			{
				return EqualityComparer<IReadOnlyList<string>>.Default.Equals(Errors, other.Errors);
			}
			return false;
		}
		return true;
	}

		private NativeModuleScan(NativeModuleScan original)
	{
		SuspiciousPaths = original.SuspiciousPaths;
		Loader = original.Loader;
		Errors = original.Errors;
	}

		public void Deconstruct(out IReadOnlyList<string> SuspiciousPaths, out LoaderFinding? Loader, out IReadOnlyList<string> Errors)
	{
		SuspiciousPaths = this.SuspiciousPaths;
		Loader = this.Loader;
		Errors = this.Errors;
	}
}
