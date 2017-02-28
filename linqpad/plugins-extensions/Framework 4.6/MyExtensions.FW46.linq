<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.IO.Compression.dll</Reference>
  <NuGetReference>System.Reactive</NuGetReference>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.Diagnostics</Namespace>
  <Namespace>System.Diagnostics.Contracts</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.IO.Compression</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Linq.Expressions</Namespace>
  <Namespace>System.Reflection</Namespace>
  <Namespace>System.Threading</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
}

public static class IntegerSequence
{
	/// <summary>
	/// Returns all positive integers from 0 to int.MaxValue inclusively.
	/// </summary>
	[Pure]
	public static IEnumerable<int> PositiveIntegers => Enumerable.Range(0, int.MaxValue);

	/// <summary>
	/// Returns all negative integers from int.MinValue to -1 inclusively.
	/// </summary>
	[Pure]
	public static IEnumerable<int> NegativeIntegers
		=> Enumerable.Range(int.MinValue, int.MaxValue).Concat(new[] { -1 });

	/// <summary>
	/// Returns all integers from int.MinValue to int.MaxValue inclusively.
	/// </summary>
	[Pure]
	public static IEnumerable<int> Integers => NegativeIntegers.Concat(PositiveIntegers);
}

public static class IEnumerableExtensions
{
	/// <summary>
	/// Enumerates <see cref="first"/>once BUT <see cref="second"/> as many times as there are elements in <see cref="first"/>.
	/// <br />
	/// Consider passing a <see cref="ICollection{T}"/> (e.g. <see cref="List{T}"/>) for parameter <see cref="second"/>.
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	/// <param name="first"></param>
	/// <param name="second"></param>
	/// <returns>The cross product of two IEnumerables.</returns>
	public static IEnumerable<Tuple<T1, T2>> CrossProduct<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
	{
		return first.SelectMany(i1 => second, Tuple.Create);
	}

	public static IEnumerable<Tuple<T1, T2>> CachedCrossProduct<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
	{
		var cache = new List<T2>();
		using (var e = first.GetEnumerator())
		{
			if (e.MoveNext())
			{
				foreach (var element in second)
				{
					cache.Add(element);
					yield return Tuple.Create(e.Current, element);
				}
			}
			while (e.MoveNext())
			{
				foreach (var item2 in cache)
				{
					yield return Tuple.Create(e.Current, item2);
				}
			}
		}
	}

	public static IEnumerable<Tuple<T1, T2>> CachedCrossProduct2<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
	{
		var cache = new List<T2>();
		var firstRun = true;
		foreach (var item1 in first)
		{
			if (firstRun)
			{
				foreach (var item2 in second)
				{
					cache.Add(item2);
					yield return Tuple.Create(item1, item2);
				}
			}
			else
			{
				foreach (var item2 in cache)
				{
					yield return Tuple.Create(item1, item2);
				}
			}
			firstRun = false;
		}
	}


	/// <summary>
	/// Returns all elements in the initial IEnumerable. After all elements have been returned once, it starts over again at the first element. Forever.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="thing"></param>
	/// <returns>An infinity sequence of elements.</returns>
	public static IEnumerable<T> Infiniloop<T>(this IEnumerable<T> source)
	{
		while (true)
		{
			foreach (var element in source)
			{
				yield return element;
			}
		}
	}

	/// <summary>
	/// Returns all elements in the initial IEnumerable. After all elements have been returned once, it starts over again at the first element. Forever.
	/// This also caches all elements in a <see cref="List{T}" /> internally. Use only when small sequences with expensive to create elements are used.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="thing"></param>
	/// <returns>An infinity sequence of elements.</returns>
	public static IEnumerable<T> CachedInfiniloop<T>(this IEnumerable<T> source)
	{
		var cache = new List<T>();
		foreach (var element in source)
		{
			cache.Add(element);
			yield return element;
		}

		while (true)
		{
			foreach (var element in cache)
			{
				yield return element;
			}
		}
	}
}

public static class IOExtensions
{
	/// <summary>
	/// Returns a new MemoryStream containing the compressed source Stream using <see cref="GZipStream" />.
	/// </summary>
	public static async Task<MemoryStream> Compress(this Stream source)
	{
		var mem = new MemoryStream();
		await source.CompressTo(mem);
		return mem;
	}

	/// <summary>
	/// Returns a new MemoryStream containing the decompressed source Stream using <see cref="GZipStream" />.
	/// </summary>
	public static async Task<MemoryStream> Decompress(this Stream source)
	{
		var mem = new MemoryStream();
		await source.DecompressTo(mem);
		return mem;
	}

	/// <summary>
	/// Uses <see cref="GZipStream" /> to write the compressed <see cref="source" /> <see cref="Stream" /> to the <see cref="destination" /> <see cref="Stream" />.
	/// </summary>
	public static async Task CompressTo(this Stream source, Stream destination)
	{
		using (var gzip = new GZipStream(destination, CompressionMode.Compress, true))
		{
			await source.CopyToAsync(gzip);
		}
	}

	/// <summary>
	/// Uses <see cref="GZipStream" /> to write the decompressed <see cref="source" /> <see cref="Stream" /> to the <see cref="destination" /> <see cref="Stream" />.
	/// </summary>
	public static async Task DecompressTo(this Stream source, Stream destination)
	{
		using (var gzip = new GZipStream(source, CompressionMode.Decompress, true))
		{
			await gzip.CopyToAsync(destination);
		}
	}

	/// <summary>
	/// Reads a <see cref="Stream" /> to the end and returns the data in a byte array.
	/// </summary>
	public static byte[] ReadToEnd(this Stream source, int blockSize = 1024)
	{
		byte[] result = new byte[((source.Length / blockSize) + 1) * blockSize];
		long position = 0;
		foreach (var block in source.ReadAllBlocks(blockSize))
		{
			Array.Copy(block, 0, result, position, block.Length);
			position += blockSize;
		}
		return result;
	}

	///
	/// how to return the correctly sized array instead of one that is "blockSize" by default? :(
	///
	public static IEnumerable<byte[]> ReadAllBlocks(this Stream source, int blockSize = 1024)
	{
		source.Position = 0;
		byte[] buffer = new byte[blockSize];
		while (source.Read(buffer, 0, blockSize) > 0)
		{
			yield return buffer;
		}
	}
}