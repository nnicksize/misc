<Query Kind="Program" />

void Main()
{
	var dir = new DirectoryInfo(@"C:\Users\dominik.hirzel\AppData\Local\Temp");
	foreach (var d in dir.EnumerateDirectories("ZAP*.tmp",SearchOption.TopDirectoryOnly))
	{
		Directory.Delete(d.FullName, true);
		Console.WriteLine($"deleted {d.FullName}");
	}
}