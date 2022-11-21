Set-ExecutionPolicy Bypass

$apiKey = [System.Environment]::GetEnvironmentVariable('NugetApiKey', 'User')
$packageVersion = '.1.0.1.6.nupkg'

$jira = [IO.Path]::Combine($PSScriptRoot, '..\Jira.Rest.Sdk\bin\Debug\Jira.Rest.Sdk' + $packageVersion)

Get-ChildItem -Path $jira -ErrorAction Stop

dotnet nuget push $jira --api-key $apiKey --source https://api.nuget.org/v3/index.json