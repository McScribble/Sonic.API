using Sonic.Models;
using Sonic.API.Services;
using Serilog;

namespace Sonic.API.Controllers;

public static class AuthControllerExtensions
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/account/register", async (IAuthService authService, UserRegisterDto userDto) =>
        {
            try
            {
                var createdUser = await authService.RegisterAsync(userDto);
                Log.Information("User registered successfully: {Username}", createdUser.Username);
                return Results.Created($"/api/Auth/{createdUser.Id}", createdUser);
            }
            catch (ArgumentException ex)
            {
                Log.Warning($"User registration failed: {ex.Message}");
                return Results.BadRequest("User Registration Failed.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during user registration.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .WithName("RegisterUser")
        .WithSummary("Register new user account")
        .WithDescription("Creates a new user account with email verification. Returns user details and authentication token upon successful registration.")
        .Produces<UserCreatedDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        app.MapPost("/api/account/login", async (IAuthService service, UserLoginDto loginDto, HttpContext context) =>
        {
            try
            {
                var tokenResponse = await service.LoginAsync(loginDto);
                Log.Information("User logged in successfully: {Login}", loginDto.Username);

                // DON'T set cookies server-side anymore
                // Just return the token response for client-side handling
                return Results.Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning($"Login failed: {ex.Message}");
                return Results.Unauthorized();
            }
            catch (ArgumentException ex)
            {
                Log.Warning($"Login failed: {ex.Message}");
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during user login.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .WithName("LoginUser")
        .WithSummary("Authenticate user login")
        .WithDescription("Authenticates a user with email/username and password. Returns JWT access token and refresh token for authenticated sessions.")
        .Produces<TokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        app.MapPost("/api/account/refresh-token", async (IAuthService authService, RefreshTokenRequestDto refreshTokenDto) =>
        {
            try
            {
                var newToken = await authService.RefreshTokenAsync(refreshTokenDto);
                Log.Information($"Token refreshed successfully for user: {refreshTokenDto.UserId}");
                // Log the successful token refresh
                return Results.Ok(newToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning($"Token refresh failed: {ex.Message}");
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during token refresh.");
                return Results.InternalServerError($"An error occurred while processing your request. {ex.Message}");
            }
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh authentication token")
        .WithDescription("Refreshes an expired JWT access token using a valid refresh token. Extends user session without requiring re-authentication.")
        .Produces<TokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}