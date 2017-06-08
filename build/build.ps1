$ErrorActionPreference = 'Stop'

$isAppveyor = [bool]$env:APPVEYOR
$configuration = if ($CONFIGURATION -ne $null) { $CONFIGURATION  } else { 'Debug' }
$platform = if ($PLATFORM -ne $null) { $PLATFORM } else { 'Any CPU' }
$projectFolder = if ($ENV:APPVEYOR_BUILD_FOLDER -ne $null) { "$ENV:APPVEYOR_BUILD_FOLDER" } else { $(Get-Location).path }
$buildFolder = $projectFolder + '\build\'
$runtime = if ($ENV:DotNetRunTime -ne $null) { $ENV:DotNetRunTime } else { 'netcoreapp1.0' }
$artifactsFolder = $buildFolder + 'artifacts\'
$appProject = $projectFolder + '\src\\Microsoft.Azure.ServiceBus.KeyVault\Microsoft.Azure.ServiceBus.KeyVault.csproj'
$testProject = $projectFolder + '\test\Microsoft.Azure.ServiceBus.KeyVault.Test\Microsoft.Azure.ServiceBus.KeyVault.Test.csproj'
$coverageFile = $buildFolder + 'coverage.xml'
$appNamespace = 'Microsoft.Azure.ServiceBus.KeyVault'
$testNamespace = 'Microsoft.Azure.ServiceBus.KeyVault.Test'

# Environment variables
$skipCodeCoverage = if ([bool][Environment]::GetEnvironmentVariable('azure-service-bus-dotnet/SkipCodeCoverage')) { $true } else { $false }

function Build-Solution
{
    Write-Host "Building projects"

    # Restore solution files
    MSBuild.exe Microsoft.Azure.ServiceBus.Plugins.sln /t:restore /p:Configuration=$configuration /p:Platform=$platform /verbosity:minimal

    # $? Returns True or False value indicating whether previous command ended with an error.
    # This is used to throw an error that will cause the AppVeyor process to fail as expected.
    if (-not $?)
    {
        throw "Package restore failed."
    }

    # Build solution
    MSBuild.exe Microsoft.Azure.ServiceBus.Plugins.sln /p:Configuration=$configuration /p:Platform=$platform /verbosity:minimal

    if (-not $?)
    {
        throw "Build failed."
    }
    else
    {
        Write-Host "Building complete."   
    }
}

function Run-UnitTests
{
    Write-Host "Running unit tests."

    if ($skipCodeCoverage)
    {
        foreach ($test in ls ($projectFolder + '\test\**\*.Test.csproj')) 
        {
            Write-Host "Testing $test"
            dotnet test $test -f $runtime
            if (-not $?)
            {
                throw "Unit tests failed."
            }
        }
        return;
    }
    
    if (-Not (Test-Path .\nuget.exe))
    {
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -outfile $buildFolder\nuget.exe
    }

    $openCoverVersion = '4.6.712'
    $openCoverNuPkgOutFile = $buildFolder + 'OpenCover.' + $openCoverVersion + '.nupkg'

    # Using a temporary version of OpenCover until a NuGet is published. https://github.com/OpenCover/opencover/issues/669
    # Once there is a new NuGet package, this if statement can be removed, and so can '-source $buildFolder' in the line after
    if (-Not (Test-Path $openCoverNuPkgOutFile)) {
        Invoke-WebRequest `
            -Uri "https://ci.appveyor.com/api/buildjobs/upad53qdyo1iv382/artifacts/main%2Fbin%2Fpackages%2Fnuget%2Fopencover%2FOpenCover.4.6.712.nupkg" `
            -OutFile $openCoverNuPkgOutFile
    }

    & $buildFolder\nuget.exe install OpenCover -version $openCoverVersion -SolutionDirectory $buildFolder -source $buildFolder

    $openCoverConsole = $buildFolder + 'packages/' + 'OpenCover.' + $openCoverVersion + '\tools\OpenCover.Console.exe'
    $target = '-target:C:\Program Files\dotnet\dotnet.exe'
    $targetArgs = '-targetargs: test ' + $testProject + ' -f ' + $runtime
    $filter = '-filter:+[' + $appNamespace + '*]* -[' + $testNamespace + ']*'
    $output = '-output:' + $coverageFile

    & $openCoverConsole $target $targetArgs $filter $output '-register:user' '-oldStyle'

    if (-not $?)
    {
        throw "Unit tests failed."
    }

    if (-Not (Test-Path $coverageFile))
    {
        return
    }
    if ($isAppveyor)
    {
        $ENV:PATH = 'C:\\Python34;C:\\Python34\\Scripts;' + $ENV:PATH
        python -m pip install --upgrade pip
        pip install git+git://github.com/codecov/codecov-python.git
        codecov -f $coverageFile -X gcov   
    }
    else
    {
        $reportGeneratorVersion = '2.5.7'
        & $buildFolder\nuget.exe install ReportGenerator -version $reportGeneratorVersion -SolutionDirectory $buildFolder
        $reportGenerator = $buildFolder + 'packages\' + 'ReportGenerator.' + $reportGeneratorVersion + '\tools\ReportGenerator.exe'
        $targetDirectory = $buildFolder + 'OpenCoverReport\'
        & $reportGenerator -reports:$coverageFile -targetdir:$targetDirectory
    }
}

function CopyArtifacts
{
    if (-Not $isAppveyor)
    {
        return
    }
    New-Item -ItemType Directory -Force -Path $artifactsFolder | Out-Null
    MSBuild.exe $appProject /t:pack /p:Configuration=$configuration /p:Platform=$platform /p:PackageOutputPath=$artifactsFolder /verbosity:minimal
    if (Test-Path $coverageFile)
    {
        Copy-Item $coverageFile $artifactsFolder
    }
}

# Run the functions

Build-Solution
Run-UnitTests
CopyArtifacts