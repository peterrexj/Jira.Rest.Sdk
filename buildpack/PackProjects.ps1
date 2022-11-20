
Set-ExecutionPolicy Bypass

$zephyr = [IO.Path]::Combine($PSScriptRoot, '..\Jira.Rest.Sdk\Jira.Rest.Sdk.csproj')

dotnet pack $zephyr