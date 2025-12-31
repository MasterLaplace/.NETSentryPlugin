param(
    [string]$GitlabUrl = "https://git.6tm.eu/",
    [string]$Token = "",
    [string]$Description = "local-runner",
    [string]$Executor = "docker",
    [string]$DockerImage = "mcr.microsoft.com/dotnet/sdk:8.0",
    [string]$Tags = "docker,dotnet"
)

if (-not $Token) {
    Write-Host "Please enter registration token:"
    $Token = Read-Host -AsSecureString | ConvertFrom-SecureString
}

# This script assumes gitlab-runner.exe is in PATH
& gitlab-runner register --non-interactive --url $GitlabUrl --registration-token $Token --executor $Executor --description $Description --docker-image $DockerImage --tag-list $Tags --run-untagged="true" --locked="false"

Write-Host "Runner registration command executed. Verify in GitLab Runners UI."
