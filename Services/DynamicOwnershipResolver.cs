using Sonic.Models;
using Sonic.Models.Base;
using Sonic.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Linq.Expressions;
using Serilog;

namespace Sonic.API.Services
{
    /// <summary>
    /// Service for dynamically resolving ownership relationships using reflection and attributes
    /// </summary>
    public class DynamicOwnershipResolver
    {
        private readonly SonicDbContext _dbContext;

        public DynamicOwnershipResolver(SonicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Dynamically checks cascading ownership using reflection and attributes
        /// </summary>
        public async Task<bool> CheckCascadingOwnershipDynamic<TEntity>(
            int userId, 
            int resourceId, 
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cascadeAttributes = entityType.GetCustomAttributes<CascadeOwnershipFromAttribute>()
                .OrderBy(attr => attr.Priority)
                .ToList();

            foreach (var cascadeAttr in cascadeAttributes)
            {
                try
                {
                    var hasOwnership = await CheckOwnershipThroughDynamicRelation<TEntity>(
                        userId, resourceId, cascadeAttr, requiredMemberships);
                    
                    if (hasOwnership)
                    {
                        Log.Information($"Cascading ownership found: User {userId} has access to {entityType.Name} {resourceId} through {cascadeAttr.PropertyName}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error checking dynamic cascading ownership for {entityType.Name} through {cascadeAttr.PropertyName}");
                }
            }

            return false;
        }

        /// <summary>
        /// Dynamically checks ownership through a relationship using reflection
        /// </summary>
        private async Task<bool> CheckOwnershipThroughDynamicRelation<TEntity>(
            int userId,
            int resourceId,
            CascadeOwnershipFromAttribute cascadeAttr,
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            var entityType = typeof(TEntity);
            var property = entityType.GetProperty(cascadeAttr.PropertyName);
            
            if (property == null)
            {
                Log.Warning($"Property {cascadeAttr.PropertyName} not found on {entityType.Name}");
                return false;
            }

            // Handle single navigation property (e.g., Event.Venue)
            if (property.PropertyType == cascadeAttr.OwningEntityType)
            {
                return await CheckSingleNavigationOwnership<TEntity>(
                    userId, resourceId, cascadeAttr, requiredMemberships);
            }

            // Handle collection navigation property (e.g., Event.Organizers)
            if (property.PropertyType.IsGenericType && 
                property.PropertyType.GetGenericArguments().Contains(cascadeAttr.OwningEntityType))
            {
                return await CheckCollectionNavigationOwnership<TEntity>(
                    userId, resourceId, cascadeAttr, requiredMemberships);
            }

            Log.Warning($"Unsupported property type {property.PropertyType.Name} for cascading ownership on {entityType.Name}.{cascadeAttr.PropertyName}");
            return false;
        }

        /// <summary>
        /// Checks ownership through a single navigation property
        /// </summary>
        private async Task<bool> CheckSingleNavigationOwnership<TEntity>(
            int userId,
            int resourceId,
            CascadeOwnershipFromAttribute cascadeAttr,
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            // Get the ResourceType for the owning entity
            var owningEntityResourceType = GetResourceTypeForEntity(cascadeAttr.OwningEntityType);
            if (owningEntityResourceType == null)
            {
                return false;
            }

            // Build a dynamic query to get the owning entity's ID
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, cascadeAttr.PropertyName);
            var idProperty = Expression.Property(property, "Id");
            var lambda = Expression.Lambda<Func<TEntity, int>>(idProperty, parameter);

            var owningEntityId = await _dbContext.Set<TEntity>()
                .Where(e => EF.Property<int>(e, "Id") == resourceId)
                .Select(lambda)
                .FirstOrDefaultAsync();

            if (owningEntityId == 0)
            {
                return false;
            }

            // Check if user has ownership of the owning entity
            return await _dbContext.ResourceMemberships
                .Where(rm => rm.User.Id == userId
                        && rm.ResourceType == owningEntityResourceType.Value
                        && rm.ResourceId == owningEntityId
                        && rm.Roles.Any(role => requiredMemberships.Contains(role)))
                .AnyAsync();
        }

        /// <summary>
        /// Checks ownership through a collection navigation property
        /// </summary>
        private async Task<bool> CheckCollectionNavigationOwnership<TEntity>(
            int userId,
            int resourceId,
            CascadeOwnershipFromAttribute cascadeAttr,
            MembershipType[] requiredMemberships)
            where TEntity : class
        {
            // For collections like Event.Organizers (List<User>), check if the current user is in the collection
            if (cascadeAttr.OwningEntityType == typeof(User))
            {
                // Build dynamic query to check if user is in the collection
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var property = Expression.Property(parameter, cascadeAttr.PropertyName);
                var anyMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(User));
                
                var userParameter = Expression.Parameter(typeof(User), "u");
                var userIdProperty = Expression.Property(userParameter, "Id");
                var userIdConstant = Expression.Constant(userId);
                var userIdEquals = Expression.Equal(userIdProperty, userIdConstant);
                var userLambda = Expression.Lambda<Func<User, bool>>(userIdEquals, userParameter);
                
                var anyCall = Expression.Call(anyMethod, property, userLambda);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(anyCall, parameter);

                return await _dbContext.Set<TEntity>()
                    .Where(e => EF.Property<int>(e, "Id") == resourceId)
                    .AnyAsync(lambda);
            }

            return false;
        }

        /// <summary>
        /// Gets the ResourceType for an entity type
        /// </summary>
        private ResourceType? GetResourceTypeForEntity(Type entityType)
        {
            var directOwnershipAttr = entityType.GetCustomAttribute<DirectOwnershipAttribute>();
            return directOwnershipAttr?.ResourceType;
        }
    }
}
