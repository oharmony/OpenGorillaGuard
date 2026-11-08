using System.Collections.Generic;

namespace BepInEx.Bootstrap;

public class CachedAssembly<T> where T : ICacheable
{

	public List<T> CacheItems { get; set; }

	public long Timestamp { get; set; }
}
