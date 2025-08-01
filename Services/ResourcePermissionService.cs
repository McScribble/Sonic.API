using Sonic.Models;
using Sonic.Models.Base;
using Sonic.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Reflection;
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
        /// Checks if the current user has the required resource membership permissions
        /// with cascading ownership support
        /// </summary>
        public async Task<bool> CheckCascadingResourcePermissionAsync<TEntity>(
            HttpContext context,
            int resourceId,
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            // Get user ID from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                Log.Warning("Cascading resource permission check failed: No valid user ID found in claims");
                return false;
            }

            // Check if user is admin first - admins have platform-wide privileges
            var user = await _dbContext.Users.FindAsync(userId);
            if (user?.IsAdmin == true)
            {
                Log.Information($"Admin override: User {userId} granted access to {typeof(TEntity).Name} {resourceId} due to admin privileges");
                return true;
            }

            // Check direct ownership first
            var hasDirectOwnership = await CheckDirectOwnership<TEntity>(userId, resourceId, requiredMemberships);
            if (hasDirectOwnership)
            {
                Log.Information($"Direct ownership: User {userId} has direct access to {typeof(TEntity).Name} {resourceId}");
                return true;
            }

            // Check cascading ownership
            var hasCascadingOwnership = await CheckCascadingOwnership<TEntity>(userId, resourceId, requiredMemberships);
            if (hasCascadingOwnership)
            {
                Log.Information($"Cascading ownership: User {userId} has cascading access to {typeof(TEntity).Name} {resourceId}");
                return true;
            }

            Log.Warning($"Access denied: User {userId} lacks required permissions for {typeof(TEntity).Name} {resourceId}");
            return false;
        }

        /// <summary>
        /// Checks direct ownership through ResourceMembership records
        /// </summary>
        private async Task<bool> CheckDirectOwnership<TEntity>(int userId, int resourceId, MembershipType[] requiredMemberships)
            where TEntity : class
        {
            // Get the DirectOwnership attribute to determine the ResourceType
            var directOwnershipAttr = typeof(TEntity).GetCustomAttribute<DirectOwnershipAttribute>();
            if (directOwnershipAttr == null)
            {
                return false; // Entity doesn't support direct ownership
            }

            var hasDirectMembership = await _dbContext.ResourceMemberships
                .Where(rm => rm.User.Id == userId
                        && rm.ResourceType == directOwnershipAttr.ResourceType
                        && rm.ResourceId == resourceId
                        && rm.Roles.Any(role => requiredMemberships.Contains(role)))
                .AnyAsync();

            return hasDirectMembership;
        }

        /// <summary>
        /// Checks cascading ownership through related entities
        /// </summary>
        private async Task<bool> CheckCascadingOwnership<TEntity>(int userId, int resourceId, MembershipType[] requiredMemberships)
            where TEntity : class
        {
            // Get all CascadeOwnershipFrom attributes
            var cascadeAttributes = typeof(TEntity).GetCustomAttributes<CascadeOwnershipFromAttribute>()
                .OrderBy(attr => attr.Priority)
                .ToList();

            if (!cascadeAttributes.Any())
            {
                return false; // No cascading ownership configured
            }

            foreach (var cascadeAttr in cascadeAttributes)
            {
                var hasOwnership = await CheckOwnershipThroughRelation<TEntity>(
                    userId, resourceId, cascadeAttr, requiredMemberships);
                
                if (hasOwnership)
                {
                    return true; // Found ownership through this relation
                }
            }

            return false;
        }

        /// <summary>
        /// Checks ownership through a specific relationship
        /// </summary>
        private async Task<bool> CheckOwnershipThroughRelation<TEntity>(
            int userId, 
            int resourceId, 
            CascadeOwnershipFromAttribute cascadeAttr, 
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            try
            {
                // This is a simplified example - in practice, you'd need to build dynamic queries
                // based on the relationship type and entity structure
                
                if (typeof(TEntity) == typeof(Event) && cascadeAttr.OwningEntityType == typeof(Venue))
                {
                    return await CheckEventVenueOwnership(userId, resourceId, requiredMemberships);
                }
                
                if (typeof(TEntity) == typeof(Event) && cascadeAttr.OwningEntityType == typeof(User))
                {
                    return await CheckEventOrganizerOwnership(userId, resourceId, requiredMemberships);
                }

                // Add more relationship checks as needed
                Log.Warning($"Unhandled cascading ownership relationship: {typeof(TEntity).Name} -> {cascadeAttr.OwningEntityType.Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error checking cascading ownership for {typeof(TEntity).Name} {resourceId}");
                return false;
            }
        }

        /// <summary>
        /// Checks if user owns the venue that hosts the event
        /// </summary>
        private async Task<bool> CheckEventVenueOwnership(int userId, int eventId, MembershipType[] requiredMemberships)
        {
            var venueId = await _dbContext.Set<Event>()
                .Where(e => e.Id == eventId && e.Venue != null)
                .Select(e => e.Venue!.Id)
                .FirstOrDefaultAsync();

            if (venueId == 0) return false;

            return await _dbContext.ResourceMemberships
                .Where(rm => rm.User.Id == userId
                        && rm.ResourceType == ResourceType.Venue
                        && rm.ResourceId == venueId
                        && rm.Roles.Any(role => requiredMemberships.Contains(role)))
                .AnyAsync();
        }

        /// <summary>
        /// Checks if user is an organizer of the event
        /// </summary>
        private async Task<bool> CheckEventOrganizerOwnership(int userId, int eventId, MembershipType[] requiredMemberships)
        {
            return await _dbContext.Set<Event>()
                .Where(e => e.Id == eventId)
                .SelectMany(e => e.Organizers)
                .AnyAsync(organizer => organizer.Id == userId);
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
