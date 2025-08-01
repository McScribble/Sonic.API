
using Sonic.Models;
using Sonic.API.Services;
using Serilog;
using System.Security.Claims;

namespace Sonic.API.Controllers;
public static class UserControllerExtensions
{
    // Helper method to get user ID from context
    private static int? GetUserIdFromContext(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return null;
        }
        return userId;
    }
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{userId:int}", async (IUserService userService, int userId) =>
        {
            try
            {
                var user = await userService.GetUserByIdAsync(userId);
                Log.Information("User retrieved successfully: {Username}", user.Username);
                return Results.Ok(user);
            }
            catch (ArgumentException ex)
            {
                Log.Warning($"Get user by ID failed: {ex.Message}");
                return Results.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the user.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization() // Require "Manager" role
        .WithName("GetUserById")
        .WithSummary("Get user by ID")
        .WithDescription("Retrieves a specific user by their unique identifier. Requires authentication.")
        .Produces<UserReadDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for getting all users - Admin only for security with pagination
        app.MapGet("/api/users", async (IUserService userService, HttpContext context) =>
        {
            try
            {
                // Check if user is admin
                var userId = GetUserIdFromContext(context);
                if (!userId.HasValue)
                {
                    Log.Warning("Unauthorized access attempt to get all users - no user ID in context");
                    return Results.Forbid();
                }

                var user = await userService.GetUserByIdAsync(userId.Value);
                if (user?.IsAdmin != true)
                {
                    Log.Warning("Unauthorized access attempt to get all users by user {UserId}", userId);
                    return Results.Forbid();
                }

                // Parse pagination parameters
                var skip = 0;
                var take = 50; // Default page size
                
                if (int.TryParse(context.Request.Query["skip"], out var skipValue))
                {
                    skip = Math.Max(0, skipValue);
                }
                
                if (int.TryParse(context.Request.Query["take"], out var takeValue))
                {
                    take = Math.Min(Math.Max(1, takeValue), 50); // Min 1, max 50
                }

                var users = await userService.GetAllUsersAsync(skip, take);
                var totalCount = await userService.GetUsersCountAsync();
                
                var response = new
                {
                    Data = users,
                    TotalCount = totalCount,
                    Skip = skip,
                    Take = take,
                    HasMore = skip + users.Count() < totalCount
                };
                
                Log.Information("Retrieved users successfully by admin {UserId}: {Count} of {Total} total (skip: {Skip}, take: {Take})", 
                    userId, users.Count(), totalCount, skip, take);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving all users.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization() // Require authentication, admin check done in code
        .WithName("GetAllUsers")
        .WithSummary("Get all users with pagination (Admin only)")
        .WithDescription("Retrieves a paginated list of all users. Only accessible by administrators.")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation => new(operation)
        {
            Parameters = operation.Parameters.Concat(new[]
            {
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
                }
            }).ToList()
        })
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        //register endpoint for checking if username is taken - requires authentication to prevent enumeration
        app.MapGet("/api/users/username/{username}", (IUserService userService, string username, HttpContext context) =>
        {
            try
            {
                // Ensure user is authenticated to prevent username enumeration attacks
                var userId = GetUserIdFromContext(context);
                if (!userId.HasValue)
                {
                    Log.Warning("Unauthenticated username check attempt for username: {Username}", username);
                    return Results.Forbid();
                }

                // Additional validation - prevent checking too many usernames rapidly
                // In production, you might want to implement rate limiting here

                var isTaken = userService.UsernameTaken(username);
                Log.Information("Username availability checked by user {UserId} for username: {Username}", userId, username);
                return Results.Ok(isTaken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while checking if username is taken.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization() // Require authentication to prevent enumeration
        .WithName("CheckUsernameTaken")
        .WithSummary("Check if username is available")
        .WithDescription("Checks if a username is already taken. Requires authentication to prevent username enumeration attacks.")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for updating a user
        app.MapPut("/api/users/{userId:int}", async (IUserService userService, UserUpdateDto user, int userId, HttpContext httpContext) =>
        {
            try
            {
                // Get current user info from context
                var currentUserId = GetUserIdFromContext(httpContext);
                if (!currentUserId.HasValue)
                {
                    Log.Warning("Unauthorized update attempt - no user ID in context");
                    return Results.Forbid();
                }

                // Get current user to check admin status
                var currentUser = await userService.GetUserByIdAsync(currentUserId.Value);
                if (currentUser == null)
                {
                    Log.Warning("Current user {UserId} not found during update attempt", currentUserId);
                    return Results.Forbid();
                }

                // Check if user is admin or updating their own profile
                bool isAdmin = currentUser.IsAdmin;
                bool isUpdatingOwnProfile = currentUserId.Value == userId;

                if (!(isAdmin || isUpdatingOwnProfile))
                {
                    Log.Warning("Unauthorized update attempt by user {CurrentUserId} for user {TargetUserId}", currentUserId, userId);
                    return Results.Forbid();
                }

                // Retrieve the target user from the database
                var existingUser = await userService.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    Log.Warning("Target user with ID {UserId} not found for update.", userId);
                    return Results.NotFound($"User with ID {userId} not found.");
                }

                // Check if admin status is being changed - only admins can change admin status
                bool adminStatusChanged = existingUser.IsAdmin != user.IsAdmin;
                if (adminStatusChanged && !isAdmin)
                {
                    Log.Warning("Unauthorized admin status change attempt by user {CurrentUserId} for user {TargetUserId}", currentUserId, userId);
                    return Results.Forbid();
                }

                var newToken = await userService.UpdateUserAsync(user, userId);
                Log.Information("User {TargetUserId} updated successfully by user {CurrentUserId}", userId, currentUserId);
                return Results.Ok(newToken);
            }
            catch (ArgumentException ex)
            {
                Log.Warning($"Update user failed: {ex.Message}");
                return Results.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the user.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization() // Require authenticated user
        .WithName("UpdateUser")
        .WithSummary("Update user profile")
        .WithDescription("Updates a user's profile information. Users can update their own profile, or admins can update any user. Only admins can change admin status.")
        .Produces<TokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}