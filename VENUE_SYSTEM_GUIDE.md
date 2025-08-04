# Venue Management System Guide

## Overview
The Venue Management System manages performance spaces, concert halls, clubs, and other locations where musical events take place. It provides comprehensive venue information, capacity management, and event hosting capabilities.

## Entity Structure

### Venue Entity
- **Purpose**: Represents physical locations for events
- **Address**: Complete location information
- **Capacity**: Maximum attendee capacity
- **Events**: Shows and performances hosted at the venue
- **Features**: Contact management, external integrations, capacity tracking

## API Endpoints

### Get All Venues
```
GET /api/venues
GET /api/venues?page=1&pageSize=10
GET /api/venues?include=Events
```

### Get Venue by ID
```
GET /api/venues/{id}
GET /api/venues/{id}?include=Events
```

### Search Venues
```
GET /api/venues/search?q=searchterm
GET /api/venues/search?q=Madison Square&include=Events
```

### Create Venue
```
POST /api/venues
Content-Type: application/json

{
  "name": "The Blue Note",
  "description": "Historic jazz club in the heart of the city",
  "capacity": 350,
  "address": {
    "street": "131 W 3rd St",
    "city": "New York",
    "state": "NY",
    "postalCode": "10012",
    "country": "USA",
    "latitude": 40.7282,
    "longitude": -74.0012
  },
  "phone": "+1-212-475-8592",
  "website": "https://bluenote.net",
  "email": "info@bluenote.net",
  "externalSources": [
    {
      "name": "Google Maps",
      "url": "https://maps.google.com/venue/123",
      "platform": "Maps"
    }
  ],
  "contacts": [
    {
      "type": "Email",
      "value": "booking@bluenote.net",
      "label": "Booking"
    }
  ]
}
```

### Update Venue
```
PUT /api/venues/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Venue Name",
  "capacity": 400,
  // ... other fields
}
```

## Data Models

### Venue Entity
```csharp
public class Venue : GenericEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Event> Events { get; set; }
    public virtual ICollection<Budget> Budgets { get; set; }
}
```

### VenueDto (API Response)
```csharp
public class VenueDto : GenericEntity
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    public List<EventSummaryDto> Events { get; set; }
}
```

### VenueSummaryDto (Reference)
```csharp
public class VenueSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public Address? Address { get; set; }
}
```

### Address Model
```csharp
public class Address
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
```

## Permission System

### Venue Permissions
- **View**: Viewer, Member, Manager, Owner
- **Update**: Manager, Owner
- **Delete**: Owner

### Resource Membership Roles
- **Owner**: Full control over the venue
- **Manager**: Can manage venue details and events
- **Member**: Can view venue information and create events
- **Viewer**: Read-only access to venue information

## Relationships

### Events
- One-to-many relationship with Events
- Venues can host multiple events
- Events are associated with exactly one venue
- Cascade venue permissions to hosted events

### Budgets
- One-to-many relationship (cascade ownership)
- Venues can own multiple budgets
- Budgets inherit permissions from venue ownership
- Useful for venue maintenance, equipment, operations

## Features

### Location Management
- **Complete Address**: Street, city, state, postal code, country
- **GPS Coordinates**: Latitude and longitude for mapping
- **Maps Integration**: Links to Google Maps, Apple Maps
- **Accessibility**: Information about venue accessibility

### Capacity Management
- **Maximum Capacity**: Total venue capacity
- **Event Planning**: Capacity constraints for event creation
- **Safety Compliance**: Ensure events don't exceed capacity
- **Seating Arrangements**: Different configurations

### Contact Information
- **General Contact**: Main venue phone and email
- **Booking Contact**: Event booking information
- **Technical Contact**: Sound/lighting technical contacts
- **Emergency Contact**: Emergency and security contacts

### External Integrations
- **Maps**: Google Maps, Apple Maps integration
- **Reviews**: Yelp, Google Reviews links
- **Ticketing**: Venue ticketing system integration
- **Social Media**: Venue social media presence

## Example Usage Scenarios

### 1. Create Concert Hall
```javascript
POST /api/venues
{
  "name": "Symphony Hall",
  "description": "World-class concert hall with perfect acoustics",
  "capacity": 2625,
  "address": {
    "street": "301 Massachusetts Avenue",
    "city": "Boston",
    "state": "MA",
    "postalCode": "02115",
    "country": "USA"
  },
  "website": "https://symphonyhall.org",
  "externalSources": [
    {
      "name": "Official Website",
      "url": "https://symphonyhall.org",
      "platform": "Website"
    }
  ]
}
```

### 2. Create Small Club
```javascript
POST /api/venues
{
  "name": "The Cellar",
  "description": "Intimate underground music venue",
  "capacity": 75,
  "address": {
    "street": "123 Underground Ave",
    "city": "Austin",
    "state": "TX",
    "postalCode": "78701",
    "country": "USA"
  }
}
```

### 3. Get Venue with Events
```javascript
GET /api/venues/1?include=Events
```

### 4. Find Venues by City
```javascript
GET /api/venues/search?q=New York&include=Events
```

## Database Schema

### Venues Table
```sql
CREATE TABLE "Venues" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "Capacity" INTEGER NOT NULL DEFAULT 0,
    "Phone" VARCHAR(20),
    "Website" TEXT,
    "Email" VARCHAR(255),
    "ExternalSources" JSONB DEFAULT '[]',
    "Contacts" JSONB DEFAULT '[]',
    
    -- Address as embedded object
    "Address_Street" VARCHAR(255),
    "Address_City" VARCHAR(100),
    "Address_State" VARCHAR(50),
    "Address_PostalCode" VARCHAR(20),
    "Address_Country" VARCHAR(50),
    "Address_Latitude" DOUBLE PRECISION,
    "Address_Longitude" DOUBLE PRECISION,
    
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Indexes
```sql
-- Geographic indexes for location searches
CREATE INDEX "IX_Venues_Address_City" ON "Venues" ("Address_City");
CREATE INDEX "IX_Venues_Address_State" ON "Venues" ("Address_State");
CREATE INDEX "IX_Venues_Capacity" ON "Venues" ("Capacity");
CREATE INDEX "IX_Venues_Location" ON "Venues" ("Address_Latitude", "Address_Longitude");
```

## Integration Points

### Event System
Venues host events and provide location and capacity constraints for event planning.

### Budget System
Venues can own budgets for facility management, maintenance, and operational expenses.

### Maps Integration
Integration with mapping services for location display and directions.

### User System
Users can manage venues through the resource membership system with appropriate permissions.

## Maps Integration

### Google Maps Integration
The venue system integrates with Google Maps API for:
- **Place Details**: Retrieving venue information
- **Geocoding**: Converting addresses to coordinates
- **Place Autocomplete**: Venue search suggestions
- **Directions**: Navigation to venues

### API Endpoints
```
GET /api/maps/places/autocomplete?input=venue name
GET /api/maps/places/details/{placeId}
```

## Best Practices

### 1. Location Accuracy
- Verify address information
- Include GPS coordinates when possible
- Test mapping integration
- Provide clear directions and landmarks

### 2. Capacity Management
- Accurately measure and record capacity
- Consider different seating configurations
- Account for safety regulations
- Plan for accessibility requirements

### 3. Contact Information
- Maintain current contact details
- Provide multiple contact methods
- Include emergency contact information
- Update booking contact details regularly

### 4. Venue Descriptions
- Provide detailed venue descriptions
- Include technical specifications
- Note accessibility features
- Highlight unique venue characteristics

### 5. External Integration
- Keep external source URLs current
- Maintain social media presence
- Update mapping information
- Monitor online reviews and ratings

## Venue Types

### Concert Halls
- Large capacity (1000+ seats)
- Professional audio/lighting systems
- Formal atmosphere
- Orchestra and symphony performances

### Music Clubs
- Medium capacity (100-500 people)
- Intimate atmosphere
- Bar/restaurant combination
- Live music focus

### Festivals Grounds
- Large outdoor capacity (5000+ people)
- Multiple stages
- Camping facilities
- Weather considerations

### Theaters
- Seated venue with stage
- Drama and musical theater
- Fixed seating arrangements
- Professional lighting systems

### Arenas/Stadiums
- Very large capacity (10,000+ people)
- Sports venue adaptation
- Major touring acts
- Complex logistics

The Venue Management System provides comprehensive functionality for managing performance spaces of all types and sizes, with robust location management, capacity tracking, and seamless integration with the event and budget management systems.
