using API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;

namespace API.Authorization
{
    public class HierarchicalRolesAuthorizationHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesAuthorizationRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            var levels = UserRoles.All.ToDictionary(
                role => role.RoleName,
                role => role.Level,
                StringComparer.OrdinalIgnoreCase);

            var userRole = context.User.FindFirstValue(ClaimTypes.Role);
            if(string.IsNullOrEmpty(userRole))
            {
                return Task.CompletedTask;
            }

            foreach (var requiredRole in requirement.AllowedRoles)
            {
                if (string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                if (!levels.TryGetValue(requiredRole, out var requiredLevel))
                {
                    continue;
                }

                var userHasEqualOrHigherRole = levels.TryGetValue(userRole, out var userLevel) && userLevel >= requiredLevel;

                if (userHasEqualOrHigherRole)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
