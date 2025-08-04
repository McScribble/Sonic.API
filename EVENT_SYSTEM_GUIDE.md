# Event Management System Guide

## Overview
The Event Management System handles concerts, shows, festivals, and other musical events within the Sonic API. It provides comprehensive event scheduling, lineup management, and attendee tracking.

## Entity Structure

### Event Entity
- **Purpose**: Represents musical events, concerts, and shows
- **Venue**: Location where the event takes place
- **Lineup**: Artists performing at the event
- **Attendees**: Users attending the event
- **Organizers**: Users managing the event
- **Tour Association**: Optional connection to a tour

## API Endpoints

### Get All Events
```
GET /api/events
GET /api/events?page=1&pageSize=10
GET /api/events?include=Venue,Lineup,Attendees,Organizers,Tour
```

### Get Event by ID
```
GET /api/events/{id}
GET /api/events/{id}?include=Venue,Lineup,Attendees
```

### Search Events
```
GET /api/events/search?q=searchterm
GET /api/events/search?q=concert&include=Venue,Lineup
```

### Create Event
```
POST /api/events
Content-Type: application/json

{
  "name": "Summer Music Festival",
  "description": "The biggest music festival of the year",
  "date": "2024-07-15T19:00:00Z",
  "doors": "2024-07-15T18:00:00Z",
  "venueId": 1,
  "tourId": 3,
  "inviteLink": "https://tickets.example.com/summer-fest",
  "externalSources": [
    {
      "name": "Ticketmaster",
      "url": "https://ticketmaster.com/event/123",
      "platform": "Ticketing"
    }
  ],
  "contacts": [
    {
      "type": "Email",
      "value": "info@summerfest.com",
      "label": "General Info"
    }
  ]
}
```

### Update Event
```
PUT /api/events/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Event Name",
  "date": "2024-07-15T20:00:00Z",
  // ... other fields
}
```

## Data Models

### Event Entity
```csharp
public class Event : GenericEntity
{
    public required string Name { get; set; }
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public string? Description { get; set; }
    public string InviteLink { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    
    // Relationships
    public int? VenueId { get; set; }
    public virtual Venue? Venue { get; set; }
    public int? TourId { get; set; }
    public virtual Tour? Tour { get; set; }
    
    // Navigation Properties
    public virtual ICollection<User> Attendees { get; set; }
    public virtual ICollection<User> Organizers { get; set; }
    public virtual ICollection<Artist> Lineup { get; set; }
    public virtual ICollection<Budget> Budgets { get; set; }
}
```

### EventDto (API Response)
```csharp
public class EventDto : GenericEntity
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public string? Description { get; set; }
    public string InviteLink { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    
    // Navigation properties (using Summary DTOs)
    public VenueSummaryDto? Venue { get; set; }
    public int? TourId { get; set; }
    public TourSummaryDto? Tour { get; set; }
    public List<UserSummaryDto> Attendees { get; set; }
    public List<UserSummaryDto> Organizers { get; set; }
    public List<ArtistSummaryDto> Lineup { get; set; }
}
```

### EventSummaryDto (Reference)
```csharp
public class EventSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? VenueId { get; set; }
    public int? TourId { get; set; }
}
```

## Permission System

### Event Permissions
- **View**: Viewer, Member, Organizer, Owner
- **Update**: Organizer, Owner
- **Delete**: Owner

### Resource Membership Roles
- **Owner**: Full control over the event
- **Organizer**: Can manage event details, lineup, and attendees
- **Member**: Can view event details and attend
- **Viewer**: Read-only access to public event information

## Relationships

### Venue
- Many-to-one relationship with Venue
- Events must be associated with a venue (optional in data model)
- Venue provides location and capacity information

### Tour
- Many-to-one relationship with Tour (optional)
- Events can be part of a tour as "shows"
- Tour association enables tour-wide management

### Lineup (Artists)
- Many-to-many relationship with Artists
- Multiple artists can perform at an event
- Artists gain organizer permissions for events they're in

### Attendees (Users)
- Many-to-many relationship with Users
- Users can attend multiple events
- Tracks event attendance and engagement

### Organizers (Users)
- Many-to-many relationship with Users
- Users can organize multiple events
- Organizers have management permissions

### Budgets
- One-to-many relationship (optional association)
- Events can have associated budgets for expense tracking
- Useful for festival production, equipment rental, etc.

## Features

### Time Management
- **Date**: Main event date and time
- **Doors**: When attendees can enter
- **Duration**: Calculated from start/end times
- **Timezone**: Handled via UTC timestamps

### External Integrations
- **Ticketing**: Links to ticket platforms
- **Streaming**: Live stream URLs
- **Social Media**: Event promotion links
- **Venues**: Integration with venue systems

### Contact Management
- **General Info**: Main event contact
- **Ticketing**: Ticket sales contact
- **Media**: Press and media contacts
- **Emergency**: Emergency contact information

## Example Usage Scenarios

### 1. Create Music Festival
```javascript
POST /api/events
{
  "name": "Rock the Summer Festival",
  "description": "3-day outdoor music festival",
  "date": "2024-08-10T16:00:00Z",
  "doors": "2024-08-10T15:00:00Z",
  "venueId": 5,
  "inviteLink": "https://tickets.example.com/rock-summer",
  "externalSources": [
    {
      "name": "Official Website",
      "url": "https://rocksummer.com",
      "platform": "Website"
    }
  ]
}
```

### 2. Add Artists to Lineup
```javascript
// Artists are added through Event-Artist relationships
PUT /api/events/1
{
  "id": 1,
  "lineupIds": [1, 2, 3, 4]  // Artist IDs
}
```

### 3. Get Event with Full Details
```javascript
GET /api/events/1?include=Venue,Lineup,Attendees,Organizers,Tour
```

### 4. Find Events by Venue
```javascript
GET /api/events/search?q=venue name&include=Venue
```

## Database Schema

### Events Table
```sql
CREATE TABLE "Events" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Date" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Doors" TIMESTAMP WITH TIME ZONE NOT NULL,
    "InviteLink" TEXT,
    "VenueId" INTEGER REFERENCES "Venues"("Id") ON DELETE RESTRICT,
    "TourId" INTEGER REFERENCES "Tours"("Id") ON DELETE SET NULL,
    "ExternalSources" JSONB DEFAULT '[]',
    "Contacts" JSONB DEFAULT '[]',
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Junction Tables
- **EventAttendees**: Links Events to Users (attendance)
- **EventOrganizers**: Links Events to Users (organization)
- **EventArtists**: Links Events to Artists (lineup)

## Integration Points

### Venue System
Events are hosted at venues, inheriting location and capacity constraints from the venue system.

### Tour System
Events can be part of tours as "shows", enabling tour-wide management and coordination.

### Artist System
Artists can be added to event lineups and gain organizer permissions for events they perform at.

### Budget System
Events can have associated budgets for tracking production costs, revenue, and expenses.

### User System
Users can attend and organize events, with different permission levels for event management.

## Best Practices

### 1. Event Planning
- Set realistic dates and times
- Confirm venue availability
- Plan adequate setup time between doors and start

### 2. Lineup Management
- Confirm artist availability before adding to lineup
- Consider set times and technical requirements
- Maintain backup artist list

### 3. Attendee Management
- Monitor capacity limits
- Provide clear event information
- Maintain contact information for updates

### 4. External Integration
- Keep ticketing links up to date
- Maintain social media presence
- Provide emergency contact information

### 5. Tour Integration
- Coordinate with tour schedules
- Maintain consistent branding across tour events
- Share resources between tour shows

The Event Management System provides comprehensive functionality for managing musical events of any scale, from intimate concerts to large festivals, with robust permission controls and seamless integration with other Sonic API systems.
