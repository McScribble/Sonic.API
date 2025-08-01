
using Sonic.API.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Serilog;

namespace Sonic.API.Controllers;

public static class GoogleAuthControllerExtensions
{
    public static void MapGoogleAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/account/login/google",
        ([FromQuery] string returnUrl,
        HttpContext context) =>
        {
            var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}/api/account/login/google/callback?returnUrl={returnUrl}";
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items = { { "returnUrl", returnUrl } }
            };
            Log.Information("Redirecting to Google for authentication.");
            return Results.Challenge(properties, new[] { "Google" });
        })
        .WithName("GoogleLogin")
        .WithSummary("Initiate Google OAuth login")
        .WithDescription("Redirects user to Google OAuth consent screen for authentication. Requires a returnUrl parameter to redirect back after authentication.")
        .Produces(StatusCodes.Status302Found);

        app.MapGet("/api/account/login/google/callback", async ([FromQuery] string returnUrl, HttpContext context, IAuthService authService) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                Log.Warning("Google authentication failed.");
                return Results.Unauthorized();
            }

            var token = await authService.LoginWithGoogleAsync(result.Principal);

            Log.Information("Google authentication successful, redirecting to {ReturnUrl}", returnUrl);
            Log.Information("Setting cookies for JWT and refresh token. Token: {@Token}", token);

            context.Response.Cookies.Append("jwt", token.AccessToken, new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = token.ExpiresAt.ToUniversalTime()
            });

            context.Response.Cookies.Append("refreshToken", token.RefreshToken, new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            Log.Information("Cookies set for JWT and refresh token, redirecting to {ReturnUrl}", returnUrl);

            return Results.Redirect(returnUrl ?? "/");
        })
        .WithName("GoogleLoginCallback")
        .WithSummary("Handle Google OAuth callback")
        .WithDescription("Processes the OAuth callback from Google, creates or logs in the user, sets authentication cookies, and redirects to the specified return URL.")
        .Produces(StatusCodes.Status302Found)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithOpenApi();
    }
}