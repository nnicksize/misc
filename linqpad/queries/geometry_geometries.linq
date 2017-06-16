<Query Kind="SQL">
  <Connection>
    <ID>c6a60f90-584f-46bd-ab2b-e2fae4717c41</ID>
    <Persist>true</Persist>
    <Server>localhost</Server>
    <Database>Playground</Database>
    <ShowServer>true</ShowServer>
  </Connection>
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

ALTER FUNCTION [dbo].[Geometries]
(
	@geography GEOGRAPHY
)
RETURNS 
@geometries TABLE 
(
	GEOGRAPHY GEOGRAPHY NOT NULL
)
AS
BEGIN
	DECLARE @i int = 0
	WHILE @i < @geography.STNumGeometries()
	BEGIN
		SET @i = @i + 1
		INSERT INTO @geometries (GEOGRAPHY) VALUES (@geography.STGeometryN(@i))
	END
	RETURN 
END
