using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace BepInEx;

public static class ThreadingExtensions
{

	public static IEnumerable<TOut> RunParallel<TIn, TOut>(this IEnumerable<TIn> data, Func<TIn, TOut> work, int workerCount = -1)
	{
		foreach (TOut item in data.ToList().RunParallel(work))
		{
			yield return item;
		}
	}

	public static IEnumerable<TOut> RunParallel<TIn, TOut>(this IList<TIn> data, Func<TIn, TOut> work, int workerCount = -1)
	{
		if (workerCount < 0)
		{
			workerCount = Mathf.Max(2, Environment.ProcessorCount);
		}
		else if (workerCount == 0)
		{
			throw new ArgumentException("Need at least 1 worker", "workerCount");
		}
		int perThreadCount = Mathf.CeilToInt((float)data.Count / (float)workerCount);
		int doneCount = 0;
		object lockObj = new object();
		ManualResetEvent are = new ManualResetEvent(initialState: false);
		IEnumerable<TOut> doneItems = null;
		Exception exceptionThrown = null;
		for (int i = 0; i < workerCount; i++)
		{
			int first = i * perThreadCount;
			int last = Mathf.Min(first + perThreadCount, data.Count);
			ThreadPool.QueueUserWorkItem(delegate
			{
				List<TOut> list = new List<TOut>(perThreadCount);
				try
				{
					for (int j = first; j < last; j++)
					{
						if (exceptionThrown != null)
						{
							break;
						}
						list.Add(work(data[j]));
					}
				}
				catch (Exception ex)
				{
					exceptionThrown = ex;
				}
				lock (lockObj)
				{
					IEnumerable<TOut> enumerable;
					if (doneItems != null)
					{
						enumerable = list.Concat(doneItems);
					}
					else
					{
						IEnumerable<TOut> enumerable2 = list;
						enumerable = enumerable2;
					}
					doneItems = enumerable;
					int num = doneCount;
					doneCount = num + 1;
					are.Set();
				}
			});
		}
		bool isDone;
		do
		{
			are.WaitOne();
			IEnumerable<TOut> enumerable3;
			lock (lockObj)
			{
				enumerable3 = doneItems;
				doneItems = null;
				isDone = doneCount == workerCount;
			}
			if (enumerable3 == null)
			{
				continue;
			}
			foreach (TOut item in enumerable3)
			{
				yield return item;
			}
		}
		while (!isDone);
		if (exceptionThrown != null)
		{
			throw new TargetInvocationException("An exception was thrown inside one of the threads", exceptionThrown);
		}
	}
}
