[cmdletbinding()]
param
(
    [parameter(mandatory)][string]$version
)

$path = "$((Get-Item -Path ".\").FullName)\Dnn.KeyMaster.dnn"
$xml = New-Object XML
$xml.Load($path)

$nodes = $xml.SelectNodes("/dotnetnuke/packages/package")
$nodes.SetAttribute("version", $version)

$xml.Save($path)

Write-Host "Updated manifest file at $path to have version: $version"