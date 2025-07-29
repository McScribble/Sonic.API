
using Sonic.Models;
using Sonic.API.Services;
using Serilog;

namespace Sonic.API.Controllers;
public static class UserControllerExtensions
{
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
        .Produces<UserReadDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for getting all users
        app.MapGet("/api/users", async (IUserService userService) =>
        {
            try
            {
                var users = await userService.GetAllUsersAsync();
                Log.Information("Retrieved all users successfully.");
                return Results.Ok(users);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving all users.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Manager)) // Require "Manager" role
        .WithName("GetAllUsers")
        .Produces<IEnumerable<User>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        //register endpoint for checking if username is taken
        app.MapGet("/api/users/username/{username}", (IUserService userService, string username) =>
        {
            try
            {
                var isTaken = userService.UsernameTaken(username);
                return Results.Ok(isTaken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while checking if username is taken.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Manager)) // Require "Manager" role
        .WithName("CheckUsernameTaken")
        .Produces<bool>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        // Register endpoint for updating a user
        app.MapPut("/api/users/{userId:int}", async (IUserService userService, UserUpdateDto user, int userId, HttpContext httpContext) =>
        {
            try
            {
            // Get roles and username from claims
            var userClaims = httpContext.User;
            var isAdmin = userClaims.IsInRole(Role.Admin);
            var isManager = userClaims.IsInRole(Role.Manager);
            var tokenUsername = userClaims.Identity?.Name;

            // Check if user is admin/manager or updating own username
            if (!(isAdmin || isManager || string.Equals(tokenUsername, user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Warning("Unauthorized update attempt by {Username} for user {TargetUsername}", tokenUsername, user.Username);
                return Results.Forbid();
            }

            // Retrieve the current user from the database
            var existingUser = await userService.GetUserByIdAsync(userId);
            if (existingUser == null)
            {
                Log.Warning("User with ID {UserId} not found for update.", userId);
                return Results.NotFound($"User with ID {userId} not found.");
            }

            // Check if roles are being changed
            bool rolesChanged = !existingUser.Roles.OrderBy(r => r).SequenceEqual(user.Roles?.OrderBy(r => r) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            // If roles are being changed and the user is not an admin or manager, forbid
            if (rolesChanged && !(isAdmin || isManager))
            {
                Log.Warning("Unauthorized role change attempt by {Username} for user {TargetUsername}", tokenUsername, user.Username);
                return Results.Forbid();
            }

            var newToken = await userService.UpdateUserAsync(user, userId);
            Log.Information("User updated successfully: {Username}", user.Username);
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
        .Produces<TokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}