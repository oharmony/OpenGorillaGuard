using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace GorillaGuard.Security;

internal static class KernelHealthProtocol
{
	internal const uint Version = 1u;

	internal const int QuerySize = 16;

	internal const int StatusSize = 96;

	internal const uint QueryCode = 2252800u;

	internal const uint DevelopmentFlag = 2147483648u;

	internal static byte[] A(ulong P_0)
	{
		byte[] array = new byte[16];
		BinaryPrimitives.WriteUInt32LittleEndian(array, 16u);
		BinaryPrimitives.WriteUInt32LittleEndian(array.AsSpan(4), 1u);
		BinaryPrimitives.WriteUInt64LittleEndian(array.AsSpan(8), P_0);
		return array;
	}

	internal static KernelHealthSnapshot A(ReadOnlySpan<byte> bytes, ulong P_1, KernelIdentity P_2, IReadOnlyList<KernelIdentity?> targets, bool P_4 = false)
	{
		if (bytes.Length != 96 || BinaryPrimitives.ReadUInt32LittleEndian(bytes) != 96 || BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(4)) != 1)
		{
			throw new InvalidDataException(_"Kernel status size or version mismatch.");
		}
		uint num = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(8));
		if ((num & 3) != 3 || (num & 0x7FFFFFFC) != 0 || BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(12)) != 0)
		{
			throw new InvalidDataException(_"Kernel protection callbacks or launcher pin are not verified.");
		}
		bool flag = (num & 0x80000000u) != 0;
		if (flag && !P_4)
		{
			throw new InvalidDataException(_"Development kernel drivers cannot authorize a production session.");
		}
		if (P_1 == 0L || BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16)) != P_1)
		{
			throw new InvalidDataException(_"Kernel status challenge mismatch.");
		}
		A(bytes.Slice(24), P_2);
		if (targets.Count != 3 || targets[0] == null)
		{
			throw new InvalidDataException(_"Expected game and VR process identities are required.");
		}
		for (int i = 0; i < targets.Count; i++)
		{
			ReadOnlySpan<byte> readOnlySpan = bytes.Slice(40 + i * 16, 16);
			KernelIdentity kernelIdentity = targets[i];
			if ((object)kernelIdentity != null)
			{
				A(readOnlySpan, kernelIdentity);
			}
			else if (BinaryPrimitives.ReadUInt64LittleEndian(readOnlySpan) != 0L || BinaryPrimitives.ReadUInt64LittleEndian(readOnlySpan.Slice(8)) != 0L)
			{
				throw new InvalidDataException(_"Kernel driver reported an unexpected protected process.");
			}
		}
		return new KernelHealthSnapshot(flag, BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(88)));
	}

	private static void A(ReadOnlySpan<byte> P_0, KernelIdentity expected)
	{
		if (expected.Pid == 0L || expected.StartedFileTime == 0L || BinaryPrimitives.ReadUInt64LittleEndian(P_0) != expected.Pid || BinaryPrimitives.ReadUInt64LittleEndian(P_0.Slice(8)) != expected.StartedFileTime)
		{
			throw new InvalidDataException(_"Kernel process identity or creation time mismatch.");
		}
	}
}
