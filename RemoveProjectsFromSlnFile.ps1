param(
    [string] $SearchString,
    $ScriptsDir = ".\Scripts"
)
. "$ScriptsDir\CommonFunctions.ps1"
trap { Handle-Trap-Error }
Print-Parameter-Values $PSCommandPath

$ErrorActionPreference = "Stop"

$slnFile = Get-ChildItem -Filter "*.sln" -Recurse | ForEach-Object { $_.FullName }

$slnContent = Get-Content "$slnFile"

$guidsToAlsoSearchFor = @()
$linesToKeep = @()
$deleteFollowupLine = $false

foreach ($line in $slnContent) {

    if ($deleteFollowupLine) {
        $deleteFollowupLine = $false
        continue # Don't add to $linesToKeep
    }

    if ($line -like "*${SearchString}*") {
        # Find all GUIDs
        $guids = ([regex]"([A-Z]|[0-9])+-([A-Z]|[0-9])+-([A-Z]|[0-9])+-([A-Z]|[0-9])+-([A-Z]|[0-9])+").Matches($line);
        
        Write-Host "Printing all found Guids on line"
        foreach ($guid in $guids) {
            Write-Host "$guid"
        }
        
        $projectGuid = $guids[1] # Last found GUID

        Write-Host "Adding projectGuid to be searched for and remove lines for: $projectGuid"

        $guidsToAlsoSearchFor += $projectGuid

        # Remember to delete the following line ("EndProject" line)
        $deleteFollowupLine = $true
    } else {
        # Check if line contains one of the project GUIDs
        $foundProjectGuid = $false
        foreach ($projectGuid in $guidsToAlsoSearchFor) {
            if ($line -like "*${projectGuid}*") {
                $foundProjectGuid = $true
            }
        }
        
        if (-Not $foundProjectGuid) {
            # Only keep lines that do not mention SearchString or the projectGuid of the project found using the SearchString
            $linesToKeep += $line
        }
    }
}

# Finally, overwrite the file's contents
$linesToKeep | Out-File $slnFile