$srcPath = Split-Path -parent $PSCommandPath
$destPath = Join-Path (gc env:userprofile) "\Documents\Visual Studio 2015\Code Snippets\Visual C#\My Code Snippets\"
Get-ChildItem $srcPath -Filter "*.snippet" | Copy-Item -Destination $destPath