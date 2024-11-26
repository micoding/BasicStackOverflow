using System.Security.Claims;
using BasicStackOverflow.Entities;
using Microsoft.AspNetCore.Authorization;

namespace BasicStackOverflow.Authorization;

public class PostOperationRequirementHandler : AuthorizationHandler<PostOperationRequirement, Post>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PostOperationRequirement requirement,
        Post post)
    {
        var userId = context.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (post.AuthorId.ToString() == userId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}