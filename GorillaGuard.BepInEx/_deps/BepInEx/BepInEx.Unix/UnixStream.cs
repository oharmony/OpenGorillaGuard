using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BepInEx.Unix;

internal class UnixStream : Stream
{
	public override bool CanRead
	{
		get
		{
			if (Access != FileAccess.Read)
			{
				return Access == FileAccess.ReadWrite;
			}
			return true;
		}
	}

	public override bool CanSeek => false;

	public override bool CanWrite
	{
		get
		{
			if (Access != FileAccess.Write)
			{
				return Access == FileAccess.ReadWrite;
			}
			return true;
		}
	}

	public override long Length
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override long Position
	{
		get
		{
			throw new InvalidOperationException();
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public FileAccess Access { get; }

	public IntPtr FileHandle { get; }

	public UnixStream(int fileDescriptor, FileAccess access)
	{
		Access = access;
		int fd = UnixStreamHelper.dup(fileDescriptor);
		FileHandle = UnixStreamHelper.fdopen(fd, (access == FileAccess.Write) ? "w" : "r");
	}

	public override void Flush()
	{
		UnixStreamHelper.fflush(FileHandle);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new InvalidOperationException();
	}

	public override void SetLength(long value)
	{
		throw new InvalidOperationException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		GCHandle gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		IntPtr intPtr = UnixStreamHelper.fread(new IntPtr(gCHandle.AddrOfPinnedObject().ToInt64() + offset), (IntPtr)count, (IntPtr)1, FileHandle);
		gCHandle.Free();
		return intPtr.ToInt32();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		GCHandle gCHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		UnixStreamHelper.fwrite(new IntPtr(gCHandle.AddrOfPinnedObject().ToInt64() + offset), (IntPtr)count, (IntPtr)1, FileHandle);
		gCHandle.Free();
	}

	private void ReleaseUnmanagedResources()
	{
		UnixStreamHelper.fclose(FileHandle);
	}

	protected override void Dispose(bool disposing)
	{
		ReleaseUnmanagedResources();
		base.Dispose(disposing);
	}

	~UnixStream()
	{
		Dispose(disposing: false);
	}
}
