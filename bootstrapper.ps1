$aup = [System.Environment]::ExpandEnvironmentVariables("%allusersprofile%")
$arguments = "& '" + $myinvocation.mycommand.definition + "'"
function ensureAdministrativeRights(){
    # run as administrator
    if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
    {   
        Start-Process powershell -Verb runAs -ArgumentList $arguments
        Break
    }
}
# ensure chocolatey is installed
$chocolateyInstalled = Test-Path "$aup\chocolatey" -pathType container
if($chocolateyInstalled -eq $false){
    $policy = Get-ExecutionPolicy
    if($policy -ne "Unrestricted" -and $policy -ne "RemoteSigned"){
        throw "Please set your executionpolicy to RemoteSigned or Unrestricted"
    }    
    ensureAdministrativeRights
    iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex

}
# ensure psake is installed
$psakeInstalled = Test-Path "$aup\chocolatey\lib\psake" -pathType container
if($psakeInstalled -eq $false){
    ensureAdministrativeRights
    choco install psake

    # after installing psake - we need to restart the console in order to have "psake" available as command (Path variable is loaded)
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    
    #Set-Alias psake "$aup\chocolatey\lib\psake\tools\psake.ps1" -Scope Global -Verbose
}

# run the default build
psake

# wait for any user input
Read-Host