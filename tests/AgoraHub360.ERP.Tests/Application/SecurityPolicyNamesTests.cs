namespace AgoraHub360.ERP.Tests.Application;

using AgoraHub360.ERP.Shared.Constants;

public class SecurityPolicyNamesTests
{
    [Fact]
    public void AllPolicyNames_ShouldBeDefined()
    {
        var policies = new[]
        {
            PolicyNames.RequireAuthenticated,
            PolicyNames.RequirePlatformSuperAdmin,
            PolicyNames.RequirePlatformAdmin,
            PolicyNames.RequireTenantSelected,
            PolicyNames.RequireTenantMembership,
            PolicyNames.RequireTenantAdmin,
            PolicyNames.RequireTenantSupervisor,
            PolicyNames.RequireTenantOperator,
            PolicyNames.RequireBranchAccessRead,
            PolicyNames.RequireBranchAccessOperate,
            PolicyNames.RequireAuditGlobalRead
        };

        Assert.All(policies, p => Assert.False(string.IsNullOrWhiteSpace(p)));
    }

    [Fact]
    public void AllPolicyNames_ShouldBeUnique()
    {
        var policies = new[]
        {
            PolicyNames.RequireAuthenticated,
            PolicyNames.RequirePlatformSuperAdmin,
            PolicyNames.RequirePlatformAdmin,
            PolicyNames.RequireTenantSelected,
            PolicyNames.RequireTenantMembership,
            PolicyNames.RequireTenantAdmin,
            PolicyNames.RequireTenantSupervisor,
            PolicyNames.RequireTenantOperator,
            PolicyNames.RequireBranchAccessRead,
            PolicyNames.RequireBranchAccessOperate,
            PolicyNames.RequireAuditGlobalRead
        };

        Assert.Equal(policies.Length, policies.Distinct(StringComparer.Ordinal).Count());
    }
}
