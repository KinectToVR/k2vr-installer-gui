param (
  [string]$path = "",
  [string]$outFile = (Get-Location).Path + "\fileList.xml"
)

# https://petri.com/creating-custom-xml-net-powershell

$doc = New-Object System.Xml.XmlDocument
$doc.AppendChild($doc.CreateXmlDeclaration("1.0","UTF-8",$null))

$root = $doc.CreateElement("FileList", $null);

$fileNode = $doc.CreateElement("Files", $null);
Get-ChildItem -Recurse -File $path | ForEach-Object {
  $strNode = $doc.CreateElement("string", $null)
  $strNode.InnerText = $_.FullName.Substring($path.Length)
  $fileNode.AppendChild($strNode)
}

$folderNode = $doc.CreateElement("Folders", $null);
Get-ChildItem -Recurse -Directory $path | ForEach-Object {
  $strNode = $doc.CreateElement("string", $null)
  $strNode.InnerText = $_.FullName.Substring($path.Length)
  $folderNode.AppendChild($strNode)
}

$root.AppendChild($fileNode)
$root.AppendChild($folderNode)
$doc.AppendChild($root)

# https://stackoverflow.com/a/52157943/
$doc.Save($ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($outFile))