using System;
using System.Collections.Generic;

namespace GorillaGuard.Security;

internal sealed class WorkQueue<T>
{
	private readonly Queue<T> items = new Queue<T>();

	private readonly int capacity;

	private long dropped;

	internal long Dropped
	{
		get
		{
			lock (items)
			{
				return dropped;
			}
		}
	}

	internal int Count
	{
		get
		{
			lock (items)
			{
				return items.Count;
			}
		}
	}

	internal bool IsEmpty => Count == 0;

	internal WorkQueue(int capacity)
	{
		if (capacity < 1)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		this.capacity = capacity;
	}

	internal void Enqueue(T item)
	{
		lock (items)
		{
			if (items.Count >= capacity)
			{
				dropped++;
			}
			else
			{
				items.Enqueue(item);
			}
		}
	}

	internal bool TryDequeue(out T item)
	{
		lock (items)
		{
			if (items.Count == 0)
			{
				item = default(T);
				return false;
			}
			item = items.Dequeue();
			return true;
		}
	}
}
