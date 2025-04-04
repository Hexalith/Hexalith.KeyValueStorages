#!/usr/bin/env pwsh
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$PackageName
)

Write-Output "Initializing package: $PackageName"
Write-Verbose "Replacing 'MyNewPackage' with '$PackageName' in all files..."
Write-Verbose "Also replacing 'mynewpackage' with '$($PackageName.ToLower())' in all files..."

# Function to process a file
function ProcessFile {
    param (
        [string]$FilePath
    )
    
    # Skip binary files and git files
    $extension = [System.IO.Path]::GetExtension($FilePath)
    $binaryExtensions = @('.dll', '.exe', '.pdb', '.zip', '.obj', '.bin')
    
    if ($binaryExtensions -contains $extension) {
        return
    }
    
    # Skip .git directory
    if ($FilePath -like "*\.git\*") {
        return
    }
    
    try {
        $content = Get-Content -Path $FilePath -Raw -ErrorAction SilentlyContinue
        if ($null -eq $content) {
            return
        }
        
        $hasChanges = $false
        
        if ($content -match "MyNewPackage") {
            $content = $content -replace "MyNewPackage", $PackageName
            $hasChanges = $true
        }
        
        if ($content -match "mynewpackage") {
            $content = $content -replace "mynewpackage", $PackageName.ToLower()
            $hasChanges = $true
        }
        
        if ($hasChanges) {
            Set-Content -Path $FilePath -Value $content -NoNewline
            Write-Verbose "Updated: $FilePath"
        }
    }
    catch {
        Write-Warning "Could not process file: $FilePath"
        Write-Warning $_.Exception.Message
    }
}

# Function to rename files or directories matching a pattern
function Rename-ProjectItems {
    param (
        [Parameter(Mandatory = $true)]
        [string]$SearchPattern,
        
        [Parameter(Mandatory = $true)]
        [string]$Replacement,
        
        [Parameter(Mandatory = $true)]
        [ValidateSet('Directory', 'File')]
        [string]$ItemType
    )
    
    $getItemsParams = @{
        Path    = '.'
        Recurse = $true
    }
    if ($ItemType -eq 'Directory') {
        $getItemsParams.Directory = $true
    } else {
        $getItemsParams.File = $true
    }
    
    $items = Get-ChildItem @getItemsParams | Where-Object { $_.Name -like "*$SearchPattern*" }
    
    if ($ItemType -eq 'Directory') {
        # Sort directories by path depth (descending) to rename nested items first
        $items = $items | Sort-Object -Property FullName -Descending
    }
    
    foreach ($item in $items) {
        $newName = $item.Name -replace [regex]::Escape($SearchPattern), $Replacement
        $parentPath = if ($ItemType -eq 'Directory') { $item.Parent.FullName } else { $item.DirectoryName }
        $newPath = Join-Path -Path $parentPath -ChildPath $newName
        try {
            Rename-Item -Path $item.FullName -NewName $newName -Force -ErrorAction Stop
            Write-Verbose "Renamed $($ItemType): $($item.FullName) -> $newPath"
        }
        catch {
            Write-Warning "Could not rename $($ItemType): $($item.FullName)"
            Write-Warning $_.Exception.Message
        }
    }
}

# Get all files recursively from the current directory
$files = Get-ChildItem -Path . -Recurse -File

# Process each file content
foreach ($file in $files) {
    ProcessFile -FilePath $file.FullName
}

# Rename directories and files
Rename-ProjectItems -SearchPattern "MyNewPackage" -Replacement $PackageName -ItemType Directory
Rename-ProjectItems -SearchPattern "MyNewPackage" -Replacement $PackageName -ItemType File
Rename-ProjectItems -SearchPattern "mynewpackage" -Replacement $PackageName.ToLower() -ItemType Directory
Rename-ProjectItems -SearchPattern "mynewpackage" -Replacement $PackageName.ToLower() -ItemType File

# Initialize and update Git submodules
Write-Output "`nInitializing Git submodules..."
try {
    # Check if .gitmodules file exists
    if (Test-Path ".gitmodules") {
        # Initialize submodules
        git submodule init
        Write-Verbose "Git submodules initialized"
        
        # Update submodules
        git submodule update
        Write-Verbose "Git submodules updated"
        
        # Checkout main branch for each submodule
        $submodules = git submodule foreach -q 'echo $name'
        foreach ($submodule in $submodules) {
            Write-Verbose "Checking out main branch for submodule: $submodule"
            Push-Location $submodule
            
            # Try to checkout main branch, if it fails try master
            git checkout main -q 2>$null
            if ($LASTEXITCODE -ne 0) {
                Write-Verbose "  Main branch not found, trying master..."
                git checkout master -q 2>$null
                if ($LASTEXITCODE -ne 0) {
                    Write-Warning "  Could not checkout main or master branch for $submodule"
                } else {
                    Write-Verbose "  Successfully checked out master branch for $submodule"
                }
            } else {
                Write-Verbose "  Successfully checked out main branch for $submodule"
            }
            
            Pop-Location
        }
    } else {
        Write-Verbose "No .gitmodules file found, skipping submodule initialization"
    }
}
catch {
    Write-Warning "Error during Git submodule operations"
    Write-Warning $_.Exception.Message
}

Write-Output "`nInitialization complete!"
Write-Output "Package name has been changed from 'MyNewPackage' to '$PackageName'"
Write-Output "Lowercase 'mynewpackage' has been changed to '$($PackageName.ToLower())'" 