Log "Start Build Variables"

# Get commit tag if exists
$tag = (git tag -l --points-at HEAD)
if ($tag -ne '') {
	Write-Host "##vso[task.setvariable variable=tag]$($tag)"
}