param (
	[string]$branch = '',

	[Parameter(Mandatory=$true)]
	[ValidateSet("dev1", "dev2", "test")]
    [string]$instance
)


# EXTERNAL SETUP REQUIRED
# The Path variable on the host machine must be setup to include the path to MSBuild.exe"
$msbuild = "MSBuild.exe"

# The file is run from the Deploy folder so the solution file is
$solutionPath =  "Qbicles.sln"

# Set the build configuration for checking the view (cshtml) compilation
$buildConfigurationView = "ViewCompilation"

# Set the build configuration for checking the view (cshtml) compilation
$buildConfigurationRelease = "Release"

# Blank line
$blankLine  = ".                                                                  ."

# Possible connection strings for checking the database
$connectionStringDev1 = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.dev1;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"
$connectionStringDev2 = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.dev2;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"
$connectionStringTest = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.test;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"


# Sort sort out correct branch of supplised
if ($branch -ne ''){
	try {git reset --hard} catch {}
	try {git checkout $branch} 
	catch {
		$errorMessage = $_.Exception.Message
		Write-Error $errorMessage
	}
	try {git pull} catch {}

	
}

Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
Write-Host "Current commit:" -BackgroundColor DarkBlue -ForegroundColor White
git rev-parse --verify HEAD
Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue

# Sort out the connection string for checking the database
$connectionStringDev1 = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.dev1;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"
$connectionStringDev2 = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.dev2;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"
$connectionStringTest = "Server=aurora-dev-test.c9gjkyawmmhj.eu-west-1.rds.amazonaws.com;Port=3306;Database=qbicles.test;Uid=qbicles;Pwd=@qbiclesqaywsx;AllowUserVariables=true;Convert Zero Datetime=true;Charset=utf8mb4;"

$connectionString =$connectionStringDev1;
 if ($instance -eq "dev1"){
    $connectionString = $connectionStringDev1
} elseif ($instance -eq "dev2") {
    $connectionString = $connectionStringDev2
} elseif ($instance -eq "test"){
    $connectionString = $connectionStringTest
}
#=========================================================================================================


# Set up an error function
# Function to log errors to a file
function Log-Error {
    param (
        [string]$message
    )

    $logFile = "..\BuildErrorsDev1.log"

    $logMessage = "{0}: {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $message
    Add-Content -Path $logFile -Value $logMessage
}

# Set error handling for the script
$ErrorActionPreference = "Stop"
$ErrorVariable = "ScriptErrors"

#=========================================================================================================






try {

	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	Write-Host "Checking Qbicles for deployment" -BackgroundColor DarkBlue -ForegroundColor White
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue

	Write-Host "Checking for MSBuild" -BackgroundColor DarkBlue -ForegroundColor White
    & $msbuild /version 
	Write-Host "MSBuild is available" -BackgroundColor DarkGreen -ForegroundColor White
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue

	# Check the views
	Write-Host "Checking the release compilation" -BackgroundColor DarkBlue -ForegroundColor White
    & $msbuild $solutionPath /t:Clean /t:Rebuild /m /p:Configuration=$buildConfigurationRelease /clp:ErrorsOnly
	if ($LASTEXITCODE -ne 0) {
	  Write-Host "Release compilation failed"
	  exit $LASTEXITCODE
	}
	Write-Host "Release compilation checked" -BackgroundColor DarkGreen -ForegroundColor White
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	
	
	# Generate migration script
	Write-Host "Executing update-database -script" -BackgroundColor DarkBlue -ForegroundColor White
	Update-Database -Verbose -Script -ConnectionString $connectionString -ConnectionProviderName "MySql.Data.MySqlClient" -StartupProjectName "Qbicles.Web" -ProjectName "Qbicles.Web" -ConfigurationTypeName "Qbicles.Web.Migrations.Configuration" -TargetMigration $null
	Write-Host "update-database - executed" -BackgroundColor DarkGreen -ForegroundColor White
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	
	

	# Check the views
	Write-Host "Checking the view (cshtml) compilation" -BackgroundColor DarkBlue -ForegroundColor White
    & $msbuild $solutionPath /t:Clean /t:Rebuild /m /p:Configuration=$buildConfigurationView /clp:ErrorsOnly
	if ($LASTEXITCODE -ne 0) {
	  Write-Host "View (cshtml) compilation failed"
	  exit $LASTEXITCODE
	}
	Write-Host "View (cshtml) compilation checked" -BackgroundColor DarkGreen -ForegroundColor White
	Write-Host $blankLine -BackgroundColor DarkBlue -ForegroundColor DarkBlue
	


} catch {
    $errorMessage = $_.Exception.Message
    Log-Error -message $errorMessage
    Write-Error $errorMessage
}

# Check if any errors occurred during script execution
if ($ScriptErrors) {
    $errorMessages = $ScriptErrors.Exception.Message | Select-Object -Unique
    $errorCount = $errorMessages.Count
    $errorMessage = "Script execution completed with $errorCount error(s):`r`n$errorMessages"
    Log-Error -message $errorMessage
    Write-Error $errorMessage
}
Else {
    Write-Output "Script execution completed successfully."
}