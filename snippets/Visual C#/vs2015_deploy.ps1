$srcPath = Split-Path -parent $PSCommandPath
$destPath = Join-Path (gc env:userprofile) "\Documents\Visual Studio 2015\Code Snippets\Visual C#\My Code Snippets\"
gci $srcPath | ? {$_.Name -like "*.snippet" } | cpi -Destination $destPath