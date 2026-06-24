namespace AgoraHub360.ERP.Api.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

public sealed class BranchAccessRequirement : IAuthorizationRequirement
{
    public BranchAccessRequirement(bool canOperate)
    {
        CanOperate = canOperate;
    }

    public bool CanOperate { get; }
}
