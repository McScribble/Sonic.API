# Tour Management System Guide

## Overview
The Tour Management System manages multi-city musical tours, organizing a series of performances across different locations. It provides comprehensive tour planning, show scheduling, artist coordination, and logistical management for concert tours of any scale.

## Entity Structure

### Tour Entity
- **Planning**: Tour dates, cities, routing optimization
- **Shows**: Individual performances within the tour
- **Artists**: Touring artists and supporting acts
- **Logistics**: Travel, accommodation, equipment coordination
- **Finance**: Tour budgets and expense tracking

## API Endpoints

### Get All Tours
```
GET /api/tours
GET /api/tours?page=1&pageSize=10
GET /api/tours?include=Events,Artists
```

### Get Tour by ID
```
GET /api/tours/{id}
GET /api/tours/{id}?include=Events,Artists
```

### Search Tours
```
GET /api/tours/search?q=searchterm
GET /api/tours/search?q=Summer Tour 2024&include=Events
```

### Create Tour
```
POST /api/tours
Content-Type: application/json

{
  "name": "World Harmony Tour 2024",
  "description": "International tour featuring contemporary world music artists",
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-08-31T00:00:00Z",
  "status": "Planning",
  "tourManager": "Sarah Johnson",
  "externalSources": [
    {
      "name": "Official Tour Website",
      "url": "https://worldharmonytour2024.com",
      "platform": "Website"
    }
  ],
  "contacts": [
    {
      "type": "Email",
      "value": "booking@worldharmonytour.com",
      "label": "Tour Booking"
    }
  ]
}
```

### Update Tour
```
PUT /api/tours/{id}
Content-Type: application/json

{
  "id": 1,
  "status": "Active",
  "tourManager": "Mike Wilson",
  // ... other fields
}
```

## Data Models

### Tour Entity
```csharp
public class Tour : GenericEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Planning";
    public string? TourManager { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Event> Events { get; set; }
    public virtual ICollection<Artist> Artists { get; set; }
    public virtual ICollection<Budget> Budgets { get; set; }
    public virtual ICollection<Expense> Expenses { get; set; }
}
```

### TourDto (API Response)
```csharp
public class TourDto : GenericEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
    public string? TourManager { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
    public List<EventSummaryDto> Events { get; set; }
    public List<ArtistSummaryDto> Artists { get; set; }
}
```

### TourSummaryDto (Reference)
```csharp
public class TourSummaryDto : GenericEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
}
```

### TourCreateDto (API Input)
```csharp
public class TourCreateDto : GenericCreateEntityDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Planning";
    public string? TourManager { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Contacts { get; set; }
}
```

## Permission System

### Tour Permissions
- **View**: Viewer, Member, Manager, Owner
- **Update**: Manager, Owner
- **Delete**: Owner

### Resource Membership Roles
- **Owner**: Full control over the tour (typically tour organizer)
- **Manager**: Can manage tour details, shows, and logistics
- **Member**: Can view tour information and participate in planning
- **Viewer**: Read-only access to tour information

## Relationships

### Events (Shows)
- One-to-many relationship with Events
- Tours consist of multiple shows/performances
- Events inherit tour context and information
- Cascading permissions from tour to events

### Artists
- Many-to-many relationship with Artists
- Tours can feature multiple artists (headliners, supporting acts)
- Artists can participate in multiple tours
- Support for different artist roles on tour

### Budgets
- One-to-many relationship (cascade ownership)
- Tours can have multiple budgets (production, marketing, travel)
- Budgets inherit permissions from tour ownership
- Comprehensive financial planning and tracking

### Expenses
- One-to-many relationship (cascade ownership)
- Tours generate numerous expenses across all categories
- Expenses can be associated with specific shows or general tour costs
- Complete expense tracking and approval workflow

## Tour Status Workflow

### Planning
- Initial tour planning and route development
- Venue booking and contract negotiations
- Artist scheduling and coordination
- Budget development and approval

### Confirmed
- All major bookings confirmed
- Contracts signed with venues and artists
- Marketing campaigns launched
- Logistics planning finalized

### Active
- Tour currently in progress
- Shows being performed
- Daily operations and management
- Real-time expense tracking

### Completed
- All shows performed successfully
- Financial reconciliation
- Final expense reports
- Tour wrap-up and documentation

### Cancelled
- Tour cancelled before completion
- Contract obligations handled
- Financial settlements processed
- Cancellation communications

## Features

### Route Planning
- **Geographic Optimization**: Efficient routing between cities
- **Travel Distance**: Minimize travel time and costs
- **Rest Days**: Schedule appropriate breaks between shows
- **Logistics Coordination**: Equipment transportation planning

### Show Management
- **Venue Coordination**: Work with multiple venues across tour
- **Date Scheduling**: Coordinate availability of artists and venues
- **Capacity Planning**: Match tour scale to venue capacities
- **Local Promotion**: Coordinate marketing in each market

### Artist Coordination
- **Lineup Management**: Headliners and supporting acts
- **Schedule Coordination**: Artist availability across tour dates
- **Contract Management**: Terms for each artist on tour
- **Performance Planning**: Set times and technical requirements

### Financial Management
- **Budget Planning**: Comprehensive tour budget development
- **Expense Tracking**: Real-time expense monitoring
- **Revenue Projections**: Ticket sales and merchandise forecasting
- **Financial Reporting**: Profitability analysis and reporting

## Example Usage Scenarios

### 1. Create Regional Tour
```javascript
POST /api/tours
{
  "name": "Midwest Indie Circuit 2024",
  "description": "5-city tour featuring emerging indie artists",
  "startDate": "2024-07-15T00:00:00Z",
  "endDate": "2024-07-29T00:00:00Z",
  "status": "Planning",
  "tourManager": "Alex Thompson",
  "contacts": [
    {
      "type": "Email",
      "value": "alex@indiecircuit.com",
      "label": "Tour Manager"
    },
    {
      "type": "Phone",
      "value": "+1-555-TOUR-MGR",
      "label": "Tour Manager Mobile"
    }
  ]
}
```

### 2. Create International Tour
```javascript
POST /api/tours
{
  "name": "Global Rhythms World Tour",
  "description": "International tour spanning 4 continents",
  "startDate": "2024-09-01T00:00:00Z",
  "endDate": "2024-12-15T00:00:00Z",
  "status": "Confirmed",
  "tourManager": "Maria Rodriguez",
  "externalSources": [
    {
      "name": "Tour Website",
      "url": "https://globalrhythmstour.com",
      "platform": "Website"
    },
    {
      "name": "Instagram",
      "url": "https://instagram.com/globalrhythmstour",
      "platform": "Instagram"
    }
  ]
}
```

### 3. Add Shows to Tour
```javascript
// Create events associated with the tour
POST /api/events
{
  "name": "Global Rhythms - London",
  "eventDate": "2024-09-15T20:00:00Z",
  "venueId": 5,
  "tourId": 1,
  "description": "London stop of the Global Rhythms World Tour"
}
```

### 4. Associate Artists with Tour
```javascript
// Add artists to tour
POST /api/artists/1/tours/1  // Headliner
POST /api/artists/2/tours/1  // Supporting act
```

### 5. Get Complete Tour Information
```javascript
GET /api/tours/1?include=Events,Artists
```

## Database Schema

### Tours Table
```sql
CREATE TABLE "Tours" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "StartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Planning',
    "TourManager" VARCHAR(200),
    "ExternalSources" JSONB DEFAULT '[]',
    "Contacts" JSONB DEFAULT '[]',
    
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Artist-Tour Relationship
```sql
CREATE TABLE "ArtistTours" (
    "ArtistId" INTEGER NOT NULL,
    "TourId" INTEGER NOT NULL,
    "Role" VARCHAR(50), -- Headliner, Supporting, Guest
    "JoinDate" DATE,
    "LeaveDate" DATE,
    PRIMARY KEY ("ArtistId", "TourId"),
    FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("TourId") REFERENCES "Tours" ("Id") ON DELETE CASCADE
);
```

### Events Relationship
```sql
-- Events table already has TourId foreign key
ALTER TABLE "Events" 
ADD CONSTRAINT "FK_Events_Tours_TourId" 
FOREIGN KEY ("TourId") REFERENCES "Tours" ("Id") ON DELETE SET NULL;
```

### Indexes
```sql
CREATE INDEX "IX_Tours_StartDate" ON "Tours" ("StartDate");
CREATE INDEX "IX_Tours_EndDate" ON "Tours" ("EndDate");
CREATE INDEX "IX_Tours_Status" ON "Tours" ("Status");
CREATE INDEX "IX_Tours_TourManager" ON "Tours" ("TourManager");
```

## Integration Points

### Event System
Tours organize multiple events (shows) with shared context and logistics.

### Artist System
Artists participate in tours with defined roles and scheduling.

### Venue System
Tours work with multiple venues across different locations.

### Budget System
Tours own budgets for financial planning and expense management.

### User System
Users manage tours through the resource membership system.

## Tour Types

### Club Tours
- **Scale**: Small venues (100-500 capacity)
- **Duration**: 2-4 weeks
- **Geography**: Regional or national
- **Logistics**: Simple, minimal crew
- **Budget**: Lower cost, higher profit margins

### Theater Tours
- **Scale**: Mid-size venues (500-2000 capacity)
- **Duration**: 4-8 weeks
- **Geography**: National or international
- **Logistics**: Moderate production requirements
- **Budget**: Balanced investment and returns

### Arena Tours
- **Scale**: Large venues (5000-20000 capacity)
- **Duration**: 2-6 months
- **Geography**: International
- **Logistics**: Complex production and crew
- **Budget**: High investment, major revenue potential

### Festival Circuit
- **Scale**: Multiple large outdoor venues
- **Duration**: Festival season (3-6 months)
- **Geography**: National or international
- **Logistics**: Shared bills, varying production
- **Budget**: Guaranteed fees, lower production costs

## Tour Planning Workflow

### 1. Concept Development
```javascript
// Initial tour concept
POST /api/tours
{
  "name": "Artist Name Tour 2024",
  "status": "Planning",
  "startDate": "2024-06-01",
  "endDate": "2024-08-31"
}
```

### 2. Route Planning
```javascript
// Add potential cities and venues
// Research venue availability and capacity
// Optimize routing for efficient travel
```

### 3. Venue Booking
```javascript
// Create events for confirmed venues
POST /api/events
{
  "name": "Tour Stop - City Name",
  "eventDate": "2024-06-15T20:00:00Z",
  "venueId": 10,
  "tourId": 1
}
```

### 4. Artist Coordination
```javascript
// Add artists to tour
POST /api/artists/1/tours/1
```

### 5. Budget Creation
```javascript
// Create tour budget
POST /api/budgets
{
  "name": "Tour Production Budget",
  "category": "Production",
  "totalAmount": 500000.00,
  "tourId": 1
}
```

## Financial Management

### Revenue Streams
- **Ticket Sales**: Primary revenue from show tickets
- **Merchandise**: Tour merchandise sales
- **Sponsorship**: Corporate sponsorship deals
- **VIP Packages**: Premium ticket experiences

### Expense Categories
- **Production**: Stage, lighting, sound equipment
- **Transportation**: Buses, trucks, flights
- **Accommodation**: Hotels, per diems
- **Crew**: Salaries, benefits, travel expenses
- **Marketing**: Advertising, promotion, PR
- **Venue**: Rental fees, technical costs
- **Insurance**: Tour insurance coverage

### Profitability Analysis
- **Break-even Analysis**: Minimum ticket sales required
- **Margin Analysis**: Profit margins per show and overall tour
- **ROI Calculation**: Return on investment analysis
- **Risk Assessment**: Financial risk factors and mitigation

## Best Practices

### 1. Planning and Logistics
- Start planning 6-12 months in advance
- Build in buffer time between shows
- Plan for weather and travel delays
- Coordinate with all stakeholders early
- Develop contingency plans

### 2. Financial Management
- Create detailed budgets with contingencies
- Track expenses in real-time
- Monitor ticket sales against projections
- Plan for currency fluctuations on international tours
- Maintain cash flow throughout tour

### 3. Artist and Crew Care
- Schedule appropriate rest days
- Provide quality accommodations
- Maintain crew morale and health
- Plan for medical emergencies
- Ensure proper insurance coverage

### 4. Marketing and Promotion
- Coordinate local promotion in each market
- Maintain consistent tour branding
- Leverage social media and digital marketing
- Build relationships with local media
- Plan merchandise strategy

### 5. Risk Management
- Obtain comprehensive insurance coverage
- Plan for show cancellations
- Maintain emergency funds
- Develop crisis communication plans
- Monitor weather and political situations

## Tour Success Metrics

### Financial Metrics
- **Total Revenue**: Overall tour income
- **Profit Margin**: Percentage of revenue retained as profit
- **Cost Per Show**: Average expenses per performance
- **Revenue Per Show**: Average income per performance

### Operational Metrics
- **On-Time Performance**: Shows starting as scheduled
- **Cancellation Rate**: Percentage of shows cancelled
- **Capacity Utilization**: Average venue fill rates
- **Crew Retention**: Staff stability throughout tour

### Marketing Metrics
- **Ticket Sales Velocity**: Speed of ticket sales
- **Social Media Engagement**: Online buzz and interaction
- **Press Coverage**: Media attention and reviews
- **Merchandise Sales**: Revenue from tour merchandise

The Tour Management System provides comprehensive functionality for planning, executing, and managing musical tours of any scale, with robust integration across all system components for complete tour lifecycle management.
