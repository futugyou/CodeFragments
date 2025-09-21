using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace KaleidoCode.Auth;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement, IResolverContext>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement,
        IResolverContext resolverContext)
    {
        // Omitted code for brevity
        return Task.CompletedTask;
    }
}

public class MinimumAgeRequirement : IAuthorizationRequirement
{
    public MinimumAgeRequirement(int minimumAge) =>
        MinimumAge = minimumAge;

    public int MinimumAge { get; }
}