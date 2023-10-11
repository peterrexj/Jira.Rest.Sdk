
Set-ExecutionPolicy Bypass

$pjOutput = [IO.Path]::Combine($PSScriptRoot, '..\Output\')
$jira = [IO.Path]::Combine($PSScriptRoot, '..\Jira.Rest.Sdk\Jira.Rest.Sdk.csproj')

dotnet pack $jira --output $pjOutput