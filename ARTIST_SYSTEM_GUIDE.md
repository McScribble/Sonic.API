# Artist Management System Guide

## Overview
The Artist Management System provides comprehensive functionality for managing music artists, their members, events, and relationships within the Sonic API ecosystem.

## Entity Structure

### Artist Entity
- **Purpose**: Represents individual artists or bands
- **Members**: Collection of Users who are part of the artist
- **Events**: Shows and performances the artist participates in
- **Features**: External sources integration, contact management, cascading ownership

## API Endpoints

### Get All Artists
```
GET /api/artists
GET /api/artists?page=1&pageSize=10
GET /api/artists?include=Members,Events
```

### Get Artist by ID
```
GET /api/artists/{id}
GET /api/artists/{id}?include=Members,Events
```

### Search Artists
```
GET /api/artists/search?q=searchterm
GET /api/artists/search?q=band name&include=Members
```

### Create Artist
```
POST /api/artists
Content-Type: application/json

{
  "name": "The Awesome Band",
  "description": "An incredible rock band",
  "genre": "Rock",
  "location": "New York, NY",
  "imageUrl": "https://example.com/image.jpg",
  "externalSources": [
    {
      "name": "Spotify",
      "url": "https://spotify.com/artist/123",
      "platform": "Spotify"
    }
  ],
  "contacts": [
    {
      "type": "Email",
      "value": "booking@awesomeband.com",
      "label": "Booking"
    }
  ]
}
```

### Update Artist
```
PUT /api/artists/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Band Name",
  "description": "Updated description",
  // ... other fields
}
```

## Data Models

### Artist Entity
```csharp
public class Artist : GenericEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    
    // Navigation Properties
    public virtual ICollection<User> Members { get; set; }
    public virtual ICollection<Event> Events { get; set; }
    public virtual ICollection<Tour> Tours { get; set; }
    public virtual ICollection<Budget> Budgets { get; set; }
}
```

### ArtistDto (API Response)
```csharp
public class ArtistDto : GenericEntity
{
    public List<ExternalSource> ExternalSources { get; set; }
    public string? Description { get; set; }
    public List<EventSummaryDto> Events { get; set; }
    public string? ImageUrl { get; set; }
    public List<UserSummaryDto> Members { get; set; }
    public List<ContactInfo> Contacts { get; set; }
}
```

### ArtistSummaryDto (Reference)
```csharp
public class ArtistSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Location { get; set; }
}
```

## Permission System

### Artist Permissions
- **View**: Viewer, Member, Manager, Owner
- **Update**: Member, Manager, Owner
- **Delete**: Owner

### Resource Membership
Artists participate in the resource membership system:
- **Owner**: Full control over the artist
- **Manager**: Can manage artist details and members
- **Member**: Can update artist information
- **Viewer**: Read-only access

## Relationships

### Members (Users)
- Many-to-many relationship with Users
- Members gain permissions based on their role
- Can have multiple artists per user

### Events
- Many-to-many relationship through EventArtists
- Artists can perform at multiple events
- Events can have multiple artists in lineup

### Tours
- Many-to-many relationship through TourArtists
- Artists can be part of multiple tours
- Tours can include multiple artists

### Budgets
- One-to-many relationship (cascade ownership)
- Artists can own multiple budgets
- Budgets inherit permissions from artist ownership

## External Integrations

### Supported Platforms
- **Spotify**: Artist profiles and music
- **Apple Music**: Artist information
- **Bandcamp**: Artist pages
- **SoundCloud**: Artist profiles
- **YouTube**: Artist channels
- **Instagram**: Social media
- **Facebook**: Social media
- **Twitter**: Social media

### Contact Types
- **Email**: Primary and booking emails
- **Phone**: Contact numbers
- **Website**: Official artist website
- **Social**: Social media handles

## Example Usage Scenarios

### 1. Create New Band
```javascript
POST /api/artists
{
  "name": "Rock Legends",
  "description": "Classic rock revival band",
  "genre": "Classic Rock",
  "location": "Los Angeles, CA",
  "externalSources": [
    {
      "name": "Spotify",
      "url": "https://open.spotify.com/artist/abc123",
      "platform": "Spotify"
    }
  ]
}
```

### 2. Add Members to Artist
```javascript
// Members are added through User-Artist relationships
// via the resource membership system
POST /api/resourcememberships
{
  "userId": 5,
  "resourceId": 1,
  "resourceType": "Artist",
  "roles": ["Member"]
}
```

### 3. Get Artist with Full Details
```javascript
GET /api/artists/1?include=Members,Events,Tours
```

## Database Schema

### Artists Table
```sql
CREATE TABLE "Artists" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Genre" VARCHAR(100),
    "Location" VARCHAR(200),
    "ImageUrl" TEXT,
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
- **UserArtists**: Links Users to Artists (members)
- **EventArtists**: Links Events to Artists (lineup)
- **TourArtists**: Links Tours to Artists (tour participants)

## Integration Points

### Event System
Artists can be added to event lineups and gain permissions to manage event details based on their membership role.

### Tour System
Artists can participate in tours, with cascade ownership allowing tour budget management.

### Budget System
Artists can own budgets for managing tour expenses, equipment costs, and revenue tracking.

### User System
Artists consist of User members with different permission levels for collaborative management.

## Best Practices

### 1. Member Management
- Use resource memberships for fine-grained access control
- Assign appropriate roles (Member, Manager, Owner)
- Regular audit of member permissions

### 2. External Sources
- Keep external source URLs up to date
- Use consistent platform naming
- Validate URLs before storing

### 3. Contact Information
- Maintain current contact details
- Use appropriate contact types
- Include context labels for clarity

### 4. Content Management
- Regular updates to descriptions and images
- Maintain genre consistency
- Keep location information current

The Artist Management System provides a robust foundation for managing musical acts with proper permission controls, external integrations, and relationship management across the Sonic API ecosystem.
