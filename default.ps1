#########################
###### PSAKE BUILD ######
#########################

# psake build definition
properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir"
    $nuget_packages_dir = "$up\.nuget\packages"
	$global:config = "debug"
}

$up = [System.Environment]::ExpandEnvironmentVariables("%UserProfile%")
$msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe"

function getNunit3TestRunner(){
    $testRunners = @(gci $nuget_packages_dir -rec -filter nunit3-console.exe)
    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 nunit3-console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName
    $testRunner
}

task default -depends local
task local -depends compile
task reset {
    get-childitem -include bin,obj,project.lock.json -recurse -force |remove-item -recurse -force
}
task clean {
    rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

# SVG BUILD
task configrelease {
    $global:config = "release"
}
task installNuget {
    $targetNugetExe = "$base_dir\Nuget\nuget.exe"

    if(-not (Test-Path $targetNugetExe)){
        $sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
        Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
    }
    Set-Alias nuget $targetNugetExe -Scope Global -Verbose  
}
task compileSvg -depends clean, installNuget {
    exec { &.\Nuget\nuget.exe restore $source_dir\svg.sln}
    #exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.sln }
}
task compileEditor -depends clean, installNuget {
     .\Nuget\nuget.exe restore svg.sln
	#exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.editor.sln }
}

task compileSvgRelease -depends clean, configrelease, installNuget {
    exec { &.\Nuget\nuget.exe restore $source_dir\svg.sln}
	#exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.sln }
}
task compileEditorRelease -depends clean, configrelease, installNuget {
    exec { &.\Nuget\nuget.exe restore $source_dir\svg.sln}
	#exec { & $msbuild /t:Clean /t:Build /p:Configuration=$config /v:q /p:NoWarn=1591 /nologo $source_dir\svg.editor.sln }
}
task packageSvg -depends compileSvg, compileSvgRelease {
	get-childitem -include obj -recurse -force | remove-item -recurse -force
	nuget pack Svg.symbols.nuspec -symbols
	nuget pack Svg.nuspec
}
task packageEditor -depends compileEditor, compileEditorRelease {
	get-childitem -include obj -recurse -force | remove-item -recurse -force
	nuget pack Svg.Editor.symbols.nuspec -symbols
	nuget pack Svg.Editor.nuspec
	nuget pack Svg.Editor.Forms.symbols.nuspec -symbols
	nuget pack Svg.Editor.Forms.nuspec
	nuget pack Svg.Editor.Views.symbols.nuspec -symbols
	nuget pack Svg.Editor.Views.nuspec
}

# BUILD
task compile -depends compileEditor
task package -depends packageSvg, packageEditor