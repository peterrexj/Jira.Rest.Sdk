using NUnit.Framework;

namespace Jira.IntegrationTests;

internal class UserTests : TestBase
{
    [TestCase]
    public void Should_Get_LoggedIn_User_Account()
    {
        var userAccount = _service.LoggedInUserAccount();
        Assert.IsNotNull(userAccount);
        Assert.IsTrue(userAccount.Active);
    }

    [TestCase]
    public void Should_Get_User_Account_By_Id()
    {
        var accountId = _service.LoggedInUserAccount().AccountId;
        var userAccount = _service.UserAccountGet(accountId);
        Assert.IsNotNull(userAccount);
        Assert.AreEqual(accountId, userAccount.AccountId);
    }

    [TestCase]
    public void Should_Assign_Issue_By_AccountId()
    {
        var issueKey = EnvironmentVariables.IssueKey;
        var accountId = _service.LoggedInUserAccount().AccountId;
        _service.IssueAssigneeByAccountId(issueKey, accountId);
        var issue = _service.IssueGetById(issueKey);
        Assert.AreEqual(accountId, issue.Fields.Assignee.AccountId);
    }
}
