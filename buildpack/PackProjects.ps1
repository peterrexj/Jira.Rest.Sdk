
Set-ExecutionPolicy Bypass

$jira = [IO.Path]::Combine($PSScriptRoot, '..\Jira.Rest.Sdk\Jira.Rest.Sdk.csproj')

dotnet pack $jira