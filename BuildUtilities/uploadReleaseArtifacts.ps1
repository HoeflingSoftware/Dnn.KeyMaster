[cmdletbinding()]
param
(
    [parameter(mandatory)][string]$organization,
    [parameter(mandatory)][string]$repo,
    [parameter(mandatory)][string]$token,
    [parameter(mandatory)][string]$artifact,
    [parameter(mandatory)][string]$name
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$headers = @{"Authorization"="token $token"; "Accept"="application/vnd.github.v3+json"};
$response = Invoke-Webrequest "https://api.github.com/repos/$organization/$repo/releases" -Headers @{"Authorization"="token $token"; "Accept"="application/vnd.github.v3+json"} 
$content = $response.Content | ConvertFrom-Json
$uploadUrl = $content[0].upload_url
$uploadUrl = $uploadUrl.Split("{")[0]
$uploadUrl = $uploadUrl + "?name=" + $name

Write-Host $uploadUrl

$response = Invoke-WebRequest $uploadUrl -Method POST -Body $artifact -Headers @{"Authorization"="token $token"; "Accept"="application/vnd.github.v3+json"; "Content-Type"="application/zip"}
Write-Host $response | ConvertFrom-Json

Write-Host "Upload completed successfully"
exit 0;