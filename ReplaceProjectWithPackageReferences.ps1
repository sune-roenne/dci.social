param(
    [string] $SearchString,
    $Version = $env:bamboo_Version,
    [switch] $PackageFeatureBranches,
    $ScriptsDir = ".\Scripts"
)
. "$ScriptsDir\CommonFunctions.ps1"
trap { Handle-Trap-Error }
Print-Parameter-Values $PSCommandPath

Write-Host "Will now replace ${SearchString} ProjectReferences with PackageReferences (using version ${version} in .csproj files that do not contain the search-string ${SearchString} in their name"

# Get version including branch-name if branch packaging is enabled, else version is unchanged
$version = & Call-Inner-Script "$ScriptsDir\GetBranchBuildVersion.ps1" -Version $Version -PackageFeatureBranches $PackageFeatureBranches

# Find csproj files:
$csprojFiles = Get-ChildItem -Filter "*.csproj" -Recurse | Where-Object { -Not $_.FullName.Contains($SearchString) } | % { $_.FullName }

foreach ($csprojFile in $csprojFiles) {
    Write-Host "Now working on $csprojFile"

    $csprojXml = [xml](Get-Content $csprojFile)

    # TODO KRU: Insert into XML below
    $projectReferenceNodes = $csprojXml.Project.ItemGroup.ProjectReference | Where-Object { $_.Include -like "*${SearchString}*" }

    # Find PackageReference ParentNode
    $packageReferenceNode = $csprojXml.Project.ItemGroup.PackageReference | Select-Object -First 1
    $packageReferenceParentNode = $packageReferenceNode.ParentNode

    if (-Not $packageReferenceParentNode) {
        Write-Host "Could not find ItemGroup XML node containing one or more PackageReferences. Skipping..."
        continue
    }

    # Add PackageReference projectReferenceNodes
    foreach($node in $projectReferenceNodes) {
        $projectReferenceCsprojPath = $node.Include
        $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension("$projectReferenceCsprojPath")
        Write-Host "File-name without extension: $fileNameWithoutExtension"

        # Add PackageReference
        $newPackageReferenceNode = $csprojXml.CreateElement("PackageReference")
        $newPackageReferenceNode.SetAttribute("Include", "$fileNameWithoutExtension")
        $newPackageReferenceNode.SetAttribute("Version", "$version")

        $packageReferenceParentNode.AppendChild($newPackageReferenceNode)
    }

    # Remove the projectReferenceNodes that have now been added as PackageReferences
    foreach($node in $projectReferenceNodes) {
        $nodeParent = $node.ParentNode
        $nodeParent.RemoveChild($node)
        # Check if ParentNode is now empty, and if so, remove it as well
        $children = $nodeParent.ChildNodes
        if (-Not $children) {
            $grandParent = $nodeParent.ParentNode
            $grandParent.RemoveChild($nodeParent)
        }
    }

    $csprojXml.Save($csprojFile)
}
