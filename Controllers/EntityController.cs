using Sonic.Models;
using Sonic.API.Services;
using Sonic.Controllers;
using Serilog;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sonic.API.Data;

namespace Sonic.API.Controllers;
public static class EntityControllerExtensions
{
    public static void MapEntityEndpoints<TDto, TCreateDto, TEntity>(
        this IEndpointRouteBuilder app, 
        ResourceType resourceType,
        Dictionary<EndpointTypes, MembershipType[]>? membershipConfig = null)
        where TDto : GenericEntity where TCreateDto : GenericCreateEntityDto where TEntity : GenericEntity
    {
        // ✅ Updated GetById endpoint with includes support and resource membership checking
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s/{entityId:int}", async (
            IEntityService<TDto, TCreateDto, TEntity> entityService, 
            SonicDbContext dbContext,
            int entityId, 
            HttpContext context) =>
        {
            var entityType = typeof(TEntity);
            var includes = ParseIncludes(context.Request.Query["include"]);

            // Check resource membership if configured
            if (membershipConfig?.ContainsKey(EndpointTypes.Get) == true)
            {
                var requiredMemberships = membershipConfig[EndpointTypes.Get];
                var hasPermission = await CheckResourcePermissionAsync(context, dbContext, resourceType, entityId, requiredMemberships);
                
                if (!hasPermission)
                {
                    Log.Warning($"Access denied: User lacks required membership for {entityType.Name} {entityId}");
                    return Results.Forbid();
                }
            }
            
            var existingEntity = await entityService.GetByIdAsync(entityId, includes);
            if (existingEntity == null)
            {
                Log.Warning($"Get {entityType.Name} by ID failed: {entityType.Name} not found with ID {entityId}");
                return Results.NotFound($"{entityType.Name} not found");
            }

            Log.Information($"{entityType.Name} retrieved successfully with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            return Results.Ok(existingEntity);
        })
        .RequireAuthorization() // Require authentication but no specific roles
        .WithName($"Get{typeof(TEntity).Name}ById")
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation => 
        {
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "include",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Description = "Comma-separated list of navigation properties to include (e.g., 'RequiredInstruments,OptionalInstruments')",
                Required = false,
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
            });
            return operation;
        });

        // ✅ Updated GetAll endpoint with includes support and admin filtering
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s", async (
            IEntityService<TDto, TCreateDto, TEntity> entityService, 
            SonicDbContext dbContext,
            HttpContext context) =>
        {
            var includes = ParseIncludes(context.Request.Query["include"]);
            
            // Check if user is admin - admins can see all resources
            var userId = GetUserIdFromContext(context);
            if (userId.HasValue)
            {
                var user = await dbContext.Users.FindAsync(userId.Value);
                if (user?.IsAdmin == true)
                {
                    var allEntities = await entityService.GetAllAsync(includes);
                    Log.Information($"Admin retrieved all {typeof(TEntity).Name.ToLower()}s successfully: {allEntities.Count()} total with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
                    return Results.Ok(allEntities);
                }
            }
            
            var entities = await entityService.GetAllAsync(includes);
            Log.Information($"Retrieved all {typeof(TEntity).Name.ToLower()}s successfully with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            return Results.Ok(entities);
        })
        .RequireAuthorization() // Require authentication but no specific roles
        .WithName($"GetAll{typeof(TEntity).Name}s")
        .Produces<IEnumerable<TDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation => 
        {
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "include",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Description = "Comma-separated list of navigation properties to include (e.g., 'RequiredInstruments,OptionalInstruments')",
                Required = false,
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
            });
            return operation;
        });

        // Register endpoint for updating an entity with resource membership checking
        app.MapPut("/api/" + typeof(TEntity).Name.ToLower() + "s", async (
            IEntityService<TDto, TCreateDto, TEntity> entityService, 
            SonicDbContext dbContext,
            TDto entityUpdate, 
            HttpContext httpContext) =>
        {
            // Check resource membership if configured
            if (membershipConfig?.ContainsKey(EndpointTypes.Update) == true)
            {
                var requiredMemberships = membershipConfig[EndpointTypes.Update];
                var hasPermission = await CheckResourcePermissionAsync(httpContext, dbContext, resourceType, entityUpdate.Id, requiredMemberships);
                
                if (!hasPermission)
                {
                    Log.Warning($"Update access denied: User lacks required membership for {typeof(TEntity).Name} {entityUpdate.Id}");
                    return Results.Forbid();
                }
            }

            entityUpdate.UpdatedAt = DateTime.UtcNow;
            var updatedEntity = await entityService.UpdateAsync(entityUpdate);
            if (updatedEntity == null)
            {
                Log.Warning($"Update {typeof(TEntity).Name} failed: {typeof(TEntity).Name} not found with ID: {entityUpdate.Id}");
                return Results.NotFound($"{typeof(TEntity).Name} not found.");
            }
            Log.Information($"{typeof(TEntity).Name} updated successfully: {updatedEntity.Name}");
            return Results.Ok(updatedEntity);
        })
        .RequireAuthorization() // Require authentication but no specific roles
        .WithName($"Update{typeof(TEntity).Name}")
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for deleting an entity with resource membership checking
        app.MapDelete("/api/" + typeof(TEntity).Name.ToLower() + "s/{entityId:int}", async (
            IEntityService<TDto, TCreateDto, TEntity> entityService, 
            SonicDbContext dbContext,
            int entityId,
            HttpContext context) =>
        {
            // Check resource membership if configured
            if (membershipConfig?.ContainsKey(EndpointTypes.Delete) == true)
            {
                var requiredMemberships = membershipConfig[EndpointTypes.Delete];
                var hasPermission = await CheckResourcePermissionAsync(context, dbContext, resourceType, entityId, requiredMemberships);
                
                if (!hasPermission)
                {
                    Log.Warning($"Delete access denied: User lacks required membership for {typeof(TEntity).Name} {entityId}");
                    return Results.Forbid();
                }
            }

            var result = await entityService.DeleteAsync(entityId);
            if (result)
            {
                Log.Information($"{typeof(TEntity).Name} deleted successfully: {entityId}");
                return Results.Ok();
            }
            else
            {
                Log.Warning($"Delete {typeof(TEntity).Name} failed: {typeof(TEntity).Name} not found with ID: {entityId}");
                return Results.NotFound($"{typeof(TEntity).Name} with ID {entityId} not found.");
            }
        })
        .RequireAuthorization() // Require authentication but no specific roles
        .WithName($"Delete{typeof(TEntity).Name}")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        app.MapPost("/api/" + typeof(TEntity).Name.ToLower() + "s", async (
            IEntityService<TDto, TCreateDto, TEntity> entityService, 
            SonicDbContext dbContext,
            TCreateDto entityCreateDto,
            HttpContext context) =>
        {
            var createdEntity = await entityService.CreateAsync(entityCreateDto);
            if (createdEntity == null)
            {
                Log.Warning($"Create {typeof(TEntity).Name} failed: Invalid {typeof(TEntity).Name} data");
                return Results.BadRequest($"Invalid {typeof(TEntity).Name} data.");
            }

            // Automatically create Owner membership for the creating user if membership is configured
            // Note: Admins will have platform-wide access and don't strictly need ownership memberships,
            // but we still create them for consistency and potential future role changes
            if (membershipConfig != null)
            {
                var userId = GetUserIdFromContext(context);
                if (userId.HasValue)
                {
                    var user = await dbContext.Users.FindAsync(userId.Value);
                    if (user != null)
                    {
                        var membership = new ResourceMembership
                        {
                            User = user,
                            ResourceId = createdEntity.Id,
                            ResourceType = resourceType,
                            Roles = new List<MembershipType> { MembershipType.Owner },
                            Name = $"Owner of {typeof(TEntity).Name} {createdEntity.Name}",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Uuid = Guid.NewGuid()
                        };

                        dbContext.ResourceMemberships.Add(membership);
                        await dbContext.SaveChangesAsync();
                        
                        Log.Information($"Created Owner membership for user {userId} on {typeof(TEntity).Name} {createdEntity.Id}");
                    }
                }
            }

            Log.Information($"{typeof(TEntity).Name} created successfully: {createdEntity.Name}");
            return Results.Created($"/api/{typeof(TEntity).Name.ToLower()}s/{createdEntity.Id}", createdEntity);
        })
        .RequireAuthorization() // Require authentication but no specific roles
        .WithName($"Create{typeof(TEntity).Name}")
        .Produces<TDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        // Register endpoint for searching entities
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s/search", async (IEntityService<TDto, TCreateDto, TEntity> entityService, string q, HttpContext context) =>
        {
            if (string.IsNullOrWhiteSpace(q) || !EntitySearch.IsValidSearchTerm(q))
            {
                Log.Warning("Search failed: Search term is invalid.");
                return Results.BadRequest("Search term is invalid. Please provide a valid search term.");
            }

            var includes = ParseIncludes(context.Request.Query["include"]);
            var results = await entityService.SearchAsync(q, includes);
            Log.Information($"Search completed for {typeof(TEntity).Name}: {results.Count()} results found with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            return Results.Ok(results);
        })
        .WithOpenApi(operation => 
        {
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "q",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Description = "Search term",
                Required = true,
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
            });
            operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
            {
                Name = "include",
                In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                Description = "Comma-separated list of navigation properties to include",
                Required = false,
                Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
            });
            return operation;
        });
    }

    // ✅ Helper method to check resource membership permissions
    private static async Task<bool> CheckResourcePermissionAsync(
        HttpContext context,
        SonicDbContext dbContext,
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

        // Check if user is admin - admins have platform-wide privileges
        var user = await dbContext.Users.FindAsync(userId);
        if (user?.IsAdmin == true)
        {
            Log.Information($"Admin override: User {userId} granted access to {resourceType} {resourceId} due to admin privileges");
            return true;
        }

        // Check if user has any of the required memberships for this resource
        var hasPermission = await dbContext.ResourceMemberships
            .AnyAsync(rm => rm.User.Id == userId
                         && rm.ResourceType == resourceType
                         && rm.ResourceId == resourceId
                         && rm.Roles.Any(role => requiredMemberships.Contains(role)));

        if (!hasPermission)
        {
            Log.Warning($"Resource permission check failed: User {userId} lacks required membership {string.Join(",", requiredMemberships)} for {resourceType} {resourceId}");
        }

        return hasPermission;
    }

    // ✅ Helper method to get user ID from context
    private static int? GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return null;
        }
        return userId;
    }

    // ✅ Helper method to parse includes from query string
    private static string[]? ParseIncludes(string? include)
    {
        if (string.IsNullOrWhiteSpace(include))
            return null;

        return include
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .ToArray();
    }
}