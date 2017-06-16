<Query Kind="FSharpProgram">
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

let calculate number = number * 12.0 + 431.13
let calculateAll numbers = Seq.map calculate numbers

let sw = Stopwatch.StartNew()
let li = [1.0..10000000.0]
printf "list created: %s" (sw.Elapsed.ToString())
calculateAll li |> ignore
printf "list calced : %s" (sw.Elapsed.ToString())

sw.Restart()
let li2 = {1.0..10000000.0}
printf "seq created: %s" (sw.Elapsed.ToString())
calculateAll li2 |> ignore
printf "seq calced : %s" (sw.Elapsed.ToString())

sw.Restart()
let li3 = [|1.0..10000000.0|]
printf "arr created: %s" (sw.Elapsed.ToString())
calculateAll li3 |> ignore
printf "arr calced : %s" (sw.Elapsed.ToString())