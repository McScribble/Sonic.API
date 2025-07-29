using Sonic.Models;
using Sonic.API.Services;
using Serilog;

namespace Sonic.API.Controllers;
public static class EntityControllerExtensions
{
    public static void MapEntityEndpoints<TDto, TCreateDto, TEntity>(this IEndpointRouteBuilder app, string editRole, string readRole)
        where TDto : GenericEntity where TCreateDto : GenericCreateEntityDto where TEntity : GenericEntity
    {
        // ✅ Updated GetById endpoint with includes support
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s/{entityId:int}", async (IEntityService<TDto, TCreateDto, TEntity> entityService, int entityId, HttpContext context) =>
        {
            var entityType = typeof(TEntity);
            var includes = ParseIncludes(context.Request.Query["include"]);
            
            var existingEntity = await entityService.GetByIdAsync(entityId, includes);
            if (existingEntity == null)
            {
                Log.Warning($"Get {entityType.Name} by ID failed: {entityType.Name} not found with ID {entityId}");
                return Results.NotFound($"{entityType.Name} not found");
            }

            Log.Information($"{entityType.Name} retrieved successfully with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            return Results.Ok(existingEntity);
        })
        .RequireAuthorization(policy => policy.RequireRole(readRole))
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

        // ✅ Updated GetAll endpoint with includes support
        app.MapGet("/api/" + typeof(TEntity).Name.ToLower() + "s", async (IEntityService<TDto, TCreateDto, TEntity> entityService, HttpContext context) =>
        {
            var includes = ParseIncludes(context.Request.Query["include"]);
            
            var entities = await entityService.GetAllAsync(includes);
            Log.Information($"Retrieved all {typeof(TEntity).Name.ToLower()}s successfully with includes: [{string.Join(", ", includes ?? Array.Empty<string>())}]");
            return Results.Ok(entities);
        })
        .RequireAuthorization(policy => policy.RequireRole(readRole))
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

        // Register endpoint for updating an entity
        app.MapPut("/api/" + typeof(TEntity).Name.ToLower() + "s", async (IEntityService<TDto, TCreateDto, TEntity> entityService, TDto entityUpdate, HttpContext httpContext) =>
        {
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
        .RequireAuthorization(policy => policy.RequireRole(editRole))
        .WithName($"Update{typeof(TEntity).Name}")
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for deleting an entity
        app.MapDelete("/api/" + typeof(TEntity).Name.ToLower() + "s/{entityId:int}", async (IEntityService<TDto, TCreateDto, TEntity> entityService, int entityId) =>
        {
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
        .RequireAuthorization(policy => policy.RequireRole(Role.Manager))
        .WithName($"Delete{typeof(TEntity).Name}")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        app.MapPost("/api/" + typeof(TEntity).Name.ToLower() + "s", async (IEntityService<TDto, TCreateDto, TEntity> entityService, TCreateDto entityCreateDto) =>
        {
            var createdEntity = await entityService.CreateAsync(entityCreateDto);
            if (createdEntity == null)
            {
                Log.Warning($"Create {typeof(TEntity).Name} failed: Invalid {typeof(TEntity).Name} data");
                return Results.BadRequest($"Invalid {typeof(TEntity).Name} data.");
            }

            Log.Information($"{typeof(TEntity).Name} created successfully: {createdEntity.Name}");
            return Results.Created($"/api/{typeof(TEntity).Name.ToLower()}s/{createdEntity.Id}", createdEntity);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Manager))
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