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

void Main()
{
	var str = @"C:\Users\dominik.hirzel\Downloads";
	var dir = new DirectoryInfo(str);
	if (dir.Exists)
	{
		var biggest = dir.GetFiles().OrderByDescending(d => d.Length).FirstOrDefault();
		if (biggest == null)
			return;
		var zipped = new FileInfo(Path.Combine(str, "zipped.zip"));
		using (var reader = biggest.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
		using (var writer = zipped.Create()) // overwrites an existing file
		using(var zip = new GZipStream(writer,CompressionMode.Compress))
		{
			reader.CopyToAsync(zip).Wait();
		}

		Console.WriteLine("Finished zipping. Please have a look and hit ENTER.");
		Console.ReadLine();

		var unzipped = new FileInfo(Path.Combine(str, "unzipped" + biggest.Extension));
		using (var reader = zipped.OpenRead())
		using (var writer = unzipped.Create())
		{
			reader.DecompressTo(writer).Wait();
		}
		Console.WriteLine("Finished unzipping. Please have a look and hit ENTER.");
		Console.ReadLine();
	}
}
