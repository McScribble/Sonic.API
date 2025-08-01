using Sonic.Models;
using Sonic.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Serilog;

namespace Sonic.API.Services
{
    public class ResourcePermissionService : IResourcePermissionService
    {
        private readonly SonicDbContext _dbContext;

        public ResourcePermissionService(SonicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Checks if the current user has the required resource membership permissions
        /// </summary>
        public async Task<bool> CheckResourcePermissionAsync(
            HttpContext context,
            ResourceType resourceType,
            int resourceId,
            MembershipType[] requiredMemberships)
        {
            // Get user ID from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Log.Warning("Resource permission check failed: No valid user ID found in claims");
                return false;
            }

            // Single query to check both admin status and resource memberships to prevent race conditions
            var userWithMemberships = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    IsAdmin = u.IsAdmin,
                    HasRequiredMembership = u.ResourceMemberships
                        .Any(rm => rm.ResourceType == resourceType
                                && rm.ResourceId == resourceId
                                && rm.Roles.Any(role => requiredMemberships.Contains(role)))
                })
                .FirstOrDefaultAsync();

            if (userWithMemberships == null)
            {
                Log.Warning("Resource permission check failed: User {UserId} not found", userId);
                return false;
            }

            // Check if user is admin - admins have platform-wide privileges
            if (userWithMemberships.IsAdmin)
            {
                Log.Information($"Admin override: User {userId} granted access to {resourceType} {resourceId} due to admin privileges");
                return true;
            }

            // Check if user has required membership
            if (!userWithMemberships.HasRequiredMembership)
            {
                Log.Warning($"Resource permission check failed: User {userId} lacks required membership {string.Join(",", requiredMemberships)} for {resourceType} {resourceId}");
            }

            return userWithMemberships.HasRequiredMembership;
        }

        /// <summary>
        /// Gets the user ID from the HTTP context claims
        /// </summary>
        public int? GetUserIdFromContext(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }

        /// <summary>
        /// Parses comma-separated include parameters from query string
        /// </summary>
        public string[]? ParseIncludes(string? include)
        {
            if (string.IsNullOrWhiteSpace(include))
                return null;

            return include
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToArray();
        }

        /// <summary>
        /// Parses pagination parameters from query string with validation
        /// </summary>
        public (int Skip, int Take) ParsePagination(IQueryCollection query)
        {
            const int DefaultTake = 50; // Default page size
            const int MaxTake = 50;     // Maximum page size to prevent abuse

            var skip = 0;
            var take = DefaultTake;

            // Parse skip parameter
            if (query.TryGetValue("skip", out var skipValue) && int.TryParse(skipValue, out var parsedSkip))
            {
                skip = Math.Max(0, parsedSkip); // Ensure skip is not negative
            }

            // Parse take parameter
            if (query.TryGetValue("take", out var takeValue) && int.TryParse(takeValue, out var parsedTake))
            {
                take = Math.Min(Math.Max(1, parsedTake), MaxTake); // Ensure take is between 1 and MaxTake
            }

            return (skip, take);
        }
    }
}
