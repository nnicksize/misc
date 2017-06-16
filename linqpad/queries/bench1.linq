<Query Kind="Program">
  <Output>DataGrids</Output>
  <GACReference>System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
  <GACReference>System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</GACReference>
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

// tis a bench
void Main()
{
	var data = Data(1_000_000, 2);
	var data0 = data[0];
	var data1 = data[1];

	Console.WriteLine("First");
	First(data0, data1, 1000);
	Console.WriteLine("Skip");
	Skip(data0, data1, 1000, 234_645);
}

public static List<List<int>> Data(int size, int number)
{
	var r = new Random();
	return Enumerable.Range(0, number)
		.Select(i => Enumerable.Range(0, size)
			.Select(j => r.Next()).ToList())
		.ToList();
}

static void Skip(List<int> data0, List<int> data1, int benchSize, int skip)
{
	var watch = Stopwatch.StartNew();
	int k = 0;
	for (int i = 0; i < benchSize; i++)
	{
		var first = data0.Skip(skip).First();
		k = first;
	}
	watch.Stop();
	var a = watch.Elapsed;
	Console.WriteLine($"linq: {a}");
	watch.Restart();
	for (int i = 0; i < benchSize; i++)
	{
		var first = data1[skip];
		k = first;
	}
	watch.Stop();
	Console.WriteLine($"not linq: {watch.Elapsed}");
	Console.WriteLine($"faster by: {a - watch.Elapsed}");
}

static void First(List<int> data0, List<int> data1, int benchSize)
{
	var watch = Stopwatch.StartNew();
	int k = 0;
	for (int i = 0; i < benchSize; i++)
	{
		var first = data0.First();
		k = first;
	}
	watch.Stop();
	var a = watch.Elapsed;
	Console.WriteLine($"linq: {watch.Elapsed}");
	watch.Restart();
	for (int i = 0; i < benchSize; i++)
	{
		var first = data1[0];
		k = first;
	}
	watch.Stop();
	Console.WriteLine($"not linq: {watch.Elapsed}");
	Console.WriteLine($"faster by: {a - watch.Elapsed}");
}
