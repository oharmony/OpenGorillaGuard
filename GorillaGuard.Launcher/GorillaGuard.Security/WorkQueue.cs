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
			throw new ArgumentOutOfRangeException(_"capacity");
		}
		this.capacity = capacity;
	}

	internal void A(T P_0)
	{
		lock (items)
		{
			if (items.Count >= capacity)
			{
				dropped++;
			}
			else
			{
				items.Enqueue(P_0);
			}
		}
	}

	internal bool A(out T P_0)
	{
		lock (items)
		{
			if (items.Count == 0)
			{
				P_0 = default(T);
				return false;
			}
			P_0 = items.Dequeue();
			return true;
		}
	}
}
