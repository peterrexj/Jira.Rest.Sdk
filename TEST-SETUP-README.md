# Jira.Rest.Sdk Integration Tests Setup

This document outlines the prerequisite environment variables and setup required to run the integration tests for the Jira.Rest.Sdk project.

## Prerequisites

Before running the integration tests, you must configure the following environment variables. These variables are used by the test suite to connect to your Jira instance and perform various operations.

## Required Environment Variables

### Authentication Variables

| Variable Name | Required | Description | Example Value |
|---------------|----------|-------------|---------------|
| `JiraServerUrl` | **Yes** | The base URL of your Jira instance | `https://yourcompany.atlassian.net` or `https://jira.yourcompany.com` |
| `JiraUsername` | **Yes** | Username for Jira authentication | `your.email@company.com` |
| `JiraPassword` | **Yes** | Password or API token for Jira authentication | `your-api-token` or `your-password` |
| `JiraAuthToken` | **Yes** | Bearer token for authentication (alternative to username/password) | `your-bearer-token` |

### Project and Issue Variables

| Variable Name | Required | Description | Example Value |
|---------------|----------|-------------|---------------|
| `ProjectKey` | **Yes** | The key of the Jira project to use for testing | `PROJ`, `TEST`, `POC` |
| `IssueKey` | **Yes** | A valid existing issue key for testing operations | `PROJ-123`, `TEST-456` |
| `IssueFilter` | **Yes** | JQL filter to find issues for testing | `project = PROJ AND status != Done` |
| `IssueNamePrefix` | **Yes** | Prefix for test issues created during testing | `TEST_AUTOMATION_` |
| `IssueCleanFilter` | **Yes** | JQL filter to find and clean up test issues | `project = PROJ AND summary ~ "TEST_AUTOMATION_"` |

### Optional Project Metadata Variables

| Variable Name | Required | Description | Example Value |
|---------------|----------|-------------|---------------|
| `FixVersion` | No | Comma-separated list of fix versions for testing | `1.0.0,1.1.0,2.0.0` |
| `Version` | No | Comma-separated list of affected versions for testing | `0.9.0,1.0.0` |
| `Component` | No | Comma-separated list of components for testing | `Backend,Frontend,API` |
| `Label` | No | Comma-separated list of labels for testing | `automation,test,integration` |

## Setting Up Environment Variables

### Windows (Command Prompt)
```cmd
set JiraServerUrl=https://yourcompany.atlassian.net
set JiraUsername=your.email@company.com
set JiraPassword=your-api-token
set JiraAuthToken=your-bearer-token
set ProjectKey=PROJ
set IssueKey=PROJ-123
set IssueFilter=project = PROJ AND status != Done
set IssueNamePrefix=TEST_AUTOMATION_
set IssueCleanFilter=project = PROJ AND summary ~ "TEST_AUTOMATION_"
set FixVersion=1.0.0,1.1.0
set Version=0.9.0,1.0.0
set Component=Backend,Frontend
set Label=automation,test
```

### Windows (PowerShell)
```powershell
$env:JiraServerUrl="https://yourcompany.atlassian.net"
$env:JiraUsername="your.email@company.com"
$env:JiraPassword="your-api-token"
$env:JiraAuthToken="your-bearer-token"
$env:ProjectKey="PROJ"
$env:IssueKey="PROJ-123"
$env:IssueFilter="project = PROJ AND status != Done"
$env:IssueNamePrefix="TEST_AUTOMATION_"
$env:IssueCleanFilter="project = PROJ AND summary ~ `"TEST_AUTOMATION_`""
$env:FixVersion="1.0.0,1.1.0"
$env:Version="0.9.0,1.0.0"
$env:Component="Backend,Frontend"
$env:Label="automation,test"
```

### Linux/macOS (Bash)
```bash
export JiraServerUrl="https://yourcompany.atlassian.net"
export JiraUsername="your.email@company.com"
export JiraPassword="your-api-token"
export JiraAuthToken="your-bearer-token"
export ProjectKey="PROJ"
export IssueKey="PROJ-123"
export IssueFilter="project = PROJ AND status != Done"
export IssueNamePrefix="TEST_AUTOMATION_"
export IssueCleanFilter="project = PROJ AND summary ~ \"TEST_AUTOMATION_\""
export FixVersion="1.0.0,1.1.0"
export Version="0.9.0,1.0.0"
export Component="Backend,Frontend"
export Label="automation,test"
```

## Authentication Methods

The tests support multiple authentication methods:

### 1. Username and Password/API Token
- Set `JiraUsername` and `JiraPassword`
- For Jira Cloud, use an API token instead of your actual password
- Generate API tokens at: https://id.atlassian.com/manage-profile/security/api-tokens

### 2. Bearer Token Authentication
- Set `JiraAuthToken` with your bearer token
- Can be used with or without username/password
- Useful for OAuth or other token-based authentication

## Test Categories and Requirements

### Connection Tests
- **Required**: `JiraServerUrl`, `JiraUsername`, `JiraPassword`, `JiraAuthToken`
- Tests basic connectivity to Jira instance

### Project Tests
- **Required**: `ProjectKey`
- Tests project retrieval and filtering

### Issue Tests
- **Required**: `ProjectKey`, `IssueKey`, `IssueFilter`, `IssueNamePrefix`, `IssueCleanFilter`
- Tests issue creation, retrieval, searching, and deletion
- **Note**: Some tests require admin rights on the Jira project

### Issue Fields Tests
- **Required**: `IssueKey`, `ProjectKey`
- **Optional**: `FixVersion`, `Version`, `Component`, `Label`
- Tests updating various issue fields like versions, components, and labels

### User Tests
- **Required**: `IssueKey`
- Tests user account operations and issue assignment

## Important Notes

1. **Permissions**: Ensure the authenticated user has appropriate permissions:
   - Create, read, update, and delete issues in the specified project
   - Admin rights may be required for some metadata operations
   - Ability to assign issues and modify issue fields

2. **Test Data**: 
   - The `IssueKey` should point to an existing issue that can be modified during testing
   - The `IssueFilter` should return at least one issue for search tests
   - The `IssueCleanFilter` is used to clean up test issues created during testing

3. **Jira Cloud vs Server**:
   - For Jira Cloud, set `isCloudVersion: true` in the service initialization
   - For Jira Server, set `isCloudVersion: false`
   - API token authentication is recommended for Jira Cloud

4. **Test Cleanup**:
   - Tests may create issues with the specified `IssueNamePrefix`
   - The cleanup test uses `IssueCleanFilter` to remove test issues
   - Ensure the filter is specific enough to avoid deleting real issues

## Running the Tests

After setting up the environment variables, you can run the tests using:

```bash
dotnet test Jira.IntegrationTests/Jira.IntegrationTests.csproj
```

Or run specific test categories:

```bash
# Run only connection tests
dotnet test --filter "FullyQualifiedName~ConnectionTests"

# Run only issue tests
dotnet test --filter "FullyQualifiedName~IssueTests"
```

## Troubleshooting

1. **Authentication Failures**: Verify your credentials and API tokens
2. **Permission Errors**: Ensure the user has sufficient permissions in the Jira project
3. **Issue Not Found**: Verify the `IssueKey` exists and is accessible
4. **Filter Returns No Results**: Check your JQL syntax in `IssueFilter` and `IssueCleanFilter`
5. **Network Issues**: Configure retry settings in the JiraService constructor if needed

For more information about the SDK usage, refer to the main [README.md](README.md) file.