param(
    $nuget = $env:bamboo_capability_system_builder_command_Nuget,
    [string] $id = $env:bamboo_planRepository_name,
    $version = $env:bamboo_Version,
    $dockerfilePath = "",
    $outputDir = ".\Output",   
    $scriptsDir = ".\Scripts",
    $sourceDir = ".\Source",
    $configDir = "config",
    $branchId = ""
)

# Traps errors and writes the error-message before exiting with exit code 1
# since throw error does not set exit code (%ERRORLEVEL%) in some versions of PowerShell.
trap
{
	$exception = $_.Exception
	$scriptStackTrace = $Error[0].ScriptStackTrace
	Write-Error "Unhandled exception: { $exception }`nScriptStackTrace: { $scriptStackTrace }"
	[Environment]::Exit(1)
}
$ErrorActionPreference = "Stop"

if (-Not $dockerfilePath) {
    throw "dockerfilePath argument not set."
}

# TODO KRU: Logic for when to choose snapshot vs release (just based on "mainBranchName" (default = "master")?)
$isSnapshotPackage = $env:bamboo_repository_branch_name -ne "master"

$REGISTRY_USERNAME = 'SOL_BUILD'
$REGISTRY_USERPASS = $env:bamboo_Sol_Build_Password

# TODO KRU: Is it important that the config file be suffixed with "-SNAPSHOT"?
# TODO KRU: Call script to create branch version i.e. 0.0.1-WAX-153
#write-host "TEST: Changing version from $version to 0.0.1-WAX-153-SNAPSHOT."
#$version = "0.0.1-WAX-153-SNAPSHOT"


# ---------------------------------------------------------------------
# Push "config" zip-artifact
# ---------------------------------------------------------------------

$configFolder = "$sourceDir\$configDir"
if (-Not (Test-Path $configFolder)) {
    throw "Could not find the required config folder, used to configure Kubernetes settings. See TODO LINK" # TODO LINK *************************************************
}

if ($DOCKER_REGISTRY_URL -eq $SNAPSHOT_DOCKER_REGISTRY) {
    $nexusConfigRepository = "config_snapshot"
} else {
    $nexusConfigRepository = "config_release"
}
$id = $id.ToLower()
$configFilePath = "config-$version.zip"
$CONFIG_REGISTRY_URL = "http://maven/nexus/repository/${nexusConfigRepository}/${id}"

Compress-Archive -Path "$configFolder" -DestinationPath "$configFilePath" -force

$pair = "$($REGISTRY_USERNAME):$($REGISTRY_USERPASS)"
$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
$basicAuthValue = "Basic $encodedCreds"
$Headers = @{
    Authorization = $basicAuthValue
}

# Direct "RAW" upload:
write-host "Upload Config: $configFilePath -> ${CONFIG_REGISTRY_URL}/${version}.zip"
Invoke-WebRequest -Uri "${CONFIG_REGISTRY_URL}/${version}.zip" -Method Put -Infile "$configFilePath" -ContentType 'application/zip' -Headers $Headers -UseBasicParsing

if ($LASTEXITCODE) {
    throw "Call failed."
}

# ---------------------------------------------------------------------
# Docker image push
# ---------------------------------------------------------------------

$PULL_DOCKER_REGISTRY = 'docker.tools.nykredit.it'
$SNAPSHOT_DOCKER_REGISTRY = 'snapshots-docker.tools.nykredit.it'
$RELEASE_DOCKER_REGISTRY = 'releases-docker.tools.nykredit.it'

if ($isSnapshotPackage) {
    $DOCKER_REGISTRY_URL = $SNAPSHOT_DOCKER_REGISTRY
} else { # Release-package
    $DOCKER_REGISTRY_URL = $RELEASE_DOCKER_REGISTRY
}

$tag = "$version"

write-host "CALL DOCKER"

# Docker install information
# docker version
#docker info
# Write-Host "Container OS mode: " -NoNewline; docker info --format '{{.OSType}}';
# write-host "Docker service info:" Get-Service -Name "*docker*"

# Try to find Docker CLI .exe!
# $dockerCli = Get-ChildItem "C:\Program Files\Docker\" -Recurse -Include  "DockerCli.exe"
# Write-Host "Here be Docker CLI .exe: $dockerCli"

# Write-Host "Before trying to build a new docker image, performing cleanup of Docker-related artifacts..."
# docker system prune --all --force --volumes

$dockerImageIdTag = "${id}:${tag}"
$dockerImageUrl = "${DOCKER_REGISTRY_URL}/nykredit/${dockerImageIdTag}"

write-host "DO: docker login ${DOCKER_REGISTRY_URL}"
docker login -u ${REGISTRY_USERNAME} -p ${REGISTRY_USERPASS} ${DOCKER_REGISTRY_URL}
if ($LASTEXITCODE) {
    Exit $LASTEXITCODE
}

# TODO KRU: Create logic to recursively find all Dockerfiles and run the build,tag,push flow for each
write-host "DO: docker image build -t ${dockerImageIdTag} ${dockerfilePath}"
docker image build -t ${dockerImageIdTag} -f ${dockerfilePath} .
if ($LASTEXITCODE) {
    Exit $LASTEXITCODE
}

write-host "DO: docker image tag ${id}:${tag} ${dockerImageUrl}"
docker image tag "${id}:${tag}" "${dockerImageUrl}"
if ($LASTEXITCODE) {
    Exit $LASTEXITCODE
}

write-host "DO: docker image push ${dockerImageUrl}"
docker image push "${dockerImageUrl}"
if ($LASTEXITCODE) {
    Exit $LASTEXITCODE
}

docker images