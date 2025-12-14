
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        var valid = context.User.HasClaim(c =>
            c.Type == "age" &&
            int.TryParse(c.Value, out var age) &&
            age >= requirement.MinimumAge);

        // Omitted code for brevity
        return Task.CompletedTask;
    }
}

public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}