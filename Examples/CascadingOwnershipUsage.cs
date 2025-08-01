using Sonic.Models;
using Sonic.API.Services;

namespace Sonic.API.Examples
{
    /// <summary>
    /// Examples of how to use cascading ownership in controllers
    /// </summary>
    public static class CascadingOwnershipUsageExamples
    {
        // Example: How to use cascading ownership in EntityController
        public static async Task<bool> ExampleUsageInController(
            IResourcePermissionService permissionService,
            HttpContext context,
            int eventId)
        {
            // For Event entity - this would automatically check:
            // 1. Direct ownership of the event
            // 2. Ownership of the venue hosting the event  
            // 3. Being an organizer of the event

            var hasPermission = await permissionService.CheckCascadingResourcePermissionAsync<Event>(
                context, 
                eventId, 
                new[] { MembershipType.Owner, MembershipType.Manager });

            // The cascading check will:
            // 1. Check if user has direct ResourceMembership for this Event
            // 2. Check if user owns the Venue that hosts this Event (via Event.Venue)
            // 3. Check if user is in the Event.Organizers collection
            // 4. Return true if ANY of these conditions are met

            return hasPermission;
        }

        // Example usage in EntityController GET endpoint:
        /*
        app.MapGet("/api/events/{id:int}", async (
            [FromServices] IEntityService<EventDto, EventCreateDto, Event> entityService,
            [FromServices] IResourcePermissionService permissionService,
            int id,
            HttpContext context) =>
        {
            // Use cascading permission check
            var hasPermission = await permissionService.CheckCascadingResourcePermissionAsync<Event>(
                context, id, new[] { MembershipType.Viewer, MembershipType.Member, MembershipType.Owner });

            if (!hasPermission)
            {
                return Results.Forbid();
            }

            var entity = await entityService.GetByIdAsync(id);
            return entity != null ? Results.Ok(entity) : Results.NotFound();
        });
        */
    }
}
