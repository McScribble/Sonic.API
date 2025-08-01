using Sonic.Models;
using Sonic.API.Services;
using Sonic.Controllers;
using Serilog;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sonic.API.Data;
using Microsoft.AspNetCore.Mvc;

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
            [FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, 
            [FromServices] SonicDbContext dbContext,
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

        // ✅ Updated GetAll endpoint with includes support, admin filtering, and pagination
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s", async (
            [FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, 
            [FromServices] SonicDbContext dbContext,
            HttpContext context) =>
        {
            var includes = ParseIncludes(context.Request.Query["include"]);
            
            // Parse pagination parameters
            var pagination = ParsePagination(context.Request.Query);
            
            // Check if user is admin - admins can see all resources
            var userId = GetUserIdFromContext(context);
            if (userId.HasValue)
            {
                var user = await dbContext.Users.FindAsync(userId.Value);
                if (user?.IsAdmin == true)
                {
                    var allEntities = await entityService.GetAllAsync(includes, pagination.Skip, pagination.Take);
                    var totalCount = await entityService.GetCountAsync();
                    
                    Log.Information($"Admin retrieved {typeof(TEntity).Name.ToLower()}s successfully: {allEntities.Count()} of {totalCount} total (skip: {pagination.Skip}, take: {pagination.Take}) with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
                    
                    var response = new
                    {
                        Data = allEntities,
                        TotalCount = totalCount,
                        Skip = pagination.Skip,
                        Take = pagination.Take,
                        HasMore = pagination.Skip + allEntities.Count() < totalCount
                    };
                    
                    return Results.Ok(response);
                }
            }
            
            var entities = await entityService.GetAllAsync(includes, pagination.Skip, pagination.Take);
            var count = await entityService.GetCountAsync();
            
            Log.Information($"Retrieved {typeof(TEntity).Name.ToLower()}s successfully: {entities.Count()} of {count} total (skip: {pagination.Skip}, take: {pagination.Take}) with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            
            var paginatedResponse = new
            {
                Data = entities,
                TotalCount = count,
                Skip = pagination.Skip,
                Take = pagination.Take,
                HasMore = pagination.Skip + entities.Count() < count
            };
            
            return Results.Ok(paginatedResponse);
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
            [FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, 
            [FromServices] SonicDbContext dbContext,
            [FromBody] TDto entityUpdate, 
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
            [FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, 
            [FromServices] SonicDbContext dbContext,
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
            [FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, 
            [FromServices] SonicDbContext dbContext,
            [FromBody] TCreateDto entityCreateDto,
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

        // Register endpoint for searching entities with pagination support
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s/search", async ([FromServices] IEntityService<TDto, TCreateDto, TEntity> entityService, string q, HttpContext context) =>
        {
            if (string.IsNullOrWhiteSpace(q) || !EntitySearch.IsValidSearchTerm(q))
            {
                Log.Warning("Search failed: Search term is invalid.");
                return Results.BadRequest("Search term is invalid. Please provide a valid search term.");
            }

            var includes = ParseIncludes(context.Request.Query["include"]);
            var pagination = ParsePagination(context.Request.Query);
            
            var results = await entityService.SearchAsync(q, includes, pagination.Skip, pagination.Take);
            var totalCount = await entityService.GetCountAsync(); // Note: This is approximate for search results
            
            Log.Information($"Search completed for {typeof(TEntity).Name}: {results.Count()} results found (skip: {pagination.Skip}, take: {pagination.Take}) with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            
            var response = new
            {
                Data = results,
                Query = q,
                TotalCount = totalCount, // Approximate - actual search count would require additional query
                Skip = pagination.Skip,
                Take = pagination.Take,
                HasMore = results.Count() == pagination.Take // Approximate indication
            };
            
            return Results.Ok(response);
        })
        .WithName($"Search{typeof(TEntity).Name}s")
        .WithSummary($"Search {typeof(TEntity).Name}s with pagination")
        .WithDescription($"Search for {typeof(TEntity).Name}s by query term with pagination support.")
        .WithOpenApi(operation => new(operation)
        {
            Parameters = operation.Parameters.Concat(new[]
            {
                new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Name = "q",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Required = true,
                    Description = "Search query term",
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
                },
                new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Name = "skip",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Required = false,
                    Description = "Number of records to skip (default: 0)",
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "integer", Default = new Microsoft.OpenApi.Any.OpenApiInteger(0) }
                },
                new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Name = "take",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Required = false,
                    Description = "Number of records to take (default: 50, max: 50)",
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "integer", Default = new Microsoft.OpenApi.Any.OpenApiInteger(50) }
                },
                new Microsoft.OpenApi.Models.OpenApiParameter
                {
                    Name = "include",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Query,
                    Required = false,
                    Description = "Related entities to include (comma-separated)",
                    Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string" }
                }
            }).ToList()
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

        // Single query to check both admin status and resource memberships to prevent race conditions
        var userWithMemberships = await dbContext.Users
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

    // ✅ Helper method to parse pagination parameters from query string
    private static (int Skip, int Take) ParsePagination(IQueryCollection query)
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