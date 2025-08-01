using Sonic.Models;

namespace Sonic.API.Services
{
    public interface IResourcePermissionService
    {
        /// <summary>
        /// Checks if the current user has the required resource membership permissions
        /// </summary>
        Task<bool> CheckResourcePermissionAsync(
            HttpContext context,
            ResourceType resourceType,
            int resourceId,
            MembershipType[] requiredMemberships);

        /// <summary>
        /// Gets the user ID from the HTTP context claims
        /// </summary>
        int? GetUserIdFromContext(HttpContext context);

        /// <summary>
        /// Parses comma-separated include parameters from query string
        /// </summary>
        string[]? ParseIncludes(string? include);

        /// <summary>
        /// Parses pagination parameters from query string with validation
        /// </summary>
        (int Skip, int Take) ParsePagination(IQueryCollection query);
    }
}
