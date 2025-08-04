# Instrument Management System Guide

## Overview
The Instrument Management System manages musical instruments used by artists and in performances. It provides comprehensive instrument cataloging, ownership tracking, technical specifications, and performance usage across events and tours.

## Entity Structure

### Instrument Entity
- **Classification**: Type, family, specific model information
- **Ownership**: Artist ownership and borrowing relationships
- **Specifications**: Technical details, serial numbers, condition
- **Usage**: Performance history and event tracking
- **Maintenance**: Condition tracking and service records

## API Endpoints

### Get All Instruments
```
GET /api/instruments
GET /api/instruments?page=1&pageSize=10
GET /api/instruments?include=Artists
```

### Get Instrument by ID
```
GET /api/instruments/{id}
GET /api/instruments/{id}?include=Artists
```

### Search Instruments
```
GET /api/instruments/search?q=searchterm
GET /api/instruments/search?q=Gibson Les Paul&include=Artists
```

### Create Instrument
```
POST /api/instruments
Content-Type: application/json

{
  "name": "1959 Gibson Les Paul Standard",
  "type": "Electric Guitar",
  "brand": "Gibson",
  "model": "Les Paul Standard",
  "year": 1959,
  "serialNumber": "9-0123",
  "description": "Vintage sunburst Les Paul in excellent condition",
  "condition": "Excellent",
  "value": 150000.00,
  "specifications": {
    "bodyWood": "Mahogany",
    "topWood": "Maple",
    "neckWood": "Mahogany",
    "fretboard": "Rosewood",
    "pickups": "PAF Humbuckers",
    "finish": "Sunburst"
  },
  "externalSources": [
    {
      "name": "Reverb Listing",
      "url": "https://reverb.com/item/12345",
      "platform": "Reverb"
    }
  ],
  "maintenanceNotes": "Refretted in 2020, original pickups"
}
```

### Update Instrument
```
PUT /api/instruments/{id}
Content-Type: application/json

{
  "id": 1,
  "condition": "Good",
  "value": 145000.00,
  "maintenanceNotes": "Refretted in 2020, original pickups. Minor ding on back.",
  // ... other fields
}
```

## Data Models

### Instrument Entity
```csharp
public class Instrument : GenericEntity
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public decimal? Value { get; set; }
    public Dictionary<string, string> Specifications { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public string? MaintenanceNotes { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Artist> Artists { get; set; }
    public virtual ICollection<EventArtist> EventArtists { get; set; }
}
```

### InstrumentDto (API Response)
```csharp
public class InstrumentDto : GenericEntity
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public decimal? Value { get; set; }
    public Dictionary<string, string> Specifications { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public string? MaintenanceNotes { get; set; }
    public List<ArtistSummaryDto> Artists { get; set; }
}
```

### InstrumentSummaryDto (Reference)
```csharp
public class InstrumentSummaryDto : GenericEntity
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
}
```

### InstrumentCreateDto (API Input)
```csharp
public class InstrumentCreateDto : GenericCreateEntityDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? SerialNumber { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public decimal? Value { get; set; }
    public Dictionary<string, string> Specifications { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public string? MaintenanceNotes { get; set; }
}
```

## Permission System

### Instrument Permissions
- **View**: Viewer, Member, Manager, Owner
- **Update**: Manager, Owner
- **Delete**: Owner

### Resource Membership Roles
- **Owner**: Full control over the instrument
- **Manager**: Can edit details and manage borrowing
- **Member**: Can view and use in performances
- **Viewer**: Read-only access to instrument information

## Relationships

### Artists
- Many-to-many relationship with Artists
- Instruments can be owned by multiple artists (partnerships, bands)
- Artists can own multiple instruments
- Support for borrowing and lending relationships

### Events
- Many-to-many relationship through EventArtist
- Instruments can be used at multiple events
- Events can feature multiple instruments
- Performance equipment tracking

## Features

### Instrument Classification
- **Type Categories**: Guitar, Bass, Drums, Keyboard, Wind, Brass, String, Percussion
- **Specific Types**: Electric Guitar, Acoustic Guitar, Bass Guitar, Drum Kit, etc.
- **Brand Recognition**: Major instrument manufacturers
- **Model Specifications**: Detailed model information and variations

### Technical Specifications
- **Construction Details**: Body wood, neck material, hardware
- **Electronics**: Pickups, preamps, built-in effects
- **Physical Measurements**: Scale length, body dimensions, weight
- **Custom Modifications**: Non-standard features and upgrades

### Condition and Value Tracking
- **Condition Grades**: Mint, Excellent, Very Good, Good, Fair, Poor
- **Market Value**: Current estimated market value
- **Insurance Value**: Replacement cost for insurance
- **Historical Value**: Tracking value changes over time

### Maintenance Management
- **Service History**: Professional maintenance and repairs
- **Condition Notes**: Detailed condition descriptions
- **Modification Log**: Tracking changes and upgrades
- **Care Instructions**: Specific maintenance requirements

## Instrument Types and Specifications

### Guitars
```json
{
  "type": "Electric Guitar",
  "specifications": {
    "bodyWood": "Mahogany",
    "topWood": "Maple",
    "neckWood": "Maple",
    "fretboard": "Rosewood",
    "scaleLength": "24.75\"",
    "frets": "22",
    "pickups": "2 Humbuckers",
    "bridge": "Tune-o-matic",
    "tuners": "Grover",
    "finish": "Sunburst"
  }
}
```

### Keyboards
```json
{
  "type": "Digital Piano",
  "specifications": {
    "keys": "88 weighted",
    "polyphony": "256 voices",
    "sounds": "500+ preset sounds",
    "effects": "Reverb, Chorus, Delay",
    "connectivity": "USB, MIDI, Audio Out",
    "power": "AC Adapter or Batteries"
  }
}
```

### Drums
```json
{
  "type": "Acoustic Drum Kit",
  "specifications": {
    "kickDrum": "22\" x 18\"",
    "snare": "14\" x 5.5\"",
    "toms": "10\", 12\", 16\"",
    "cymbals": "Hi-hat, Crash, Ride",
    "shells": "Maple",
    "heads": "Coated Ambassador",
    "hardware": "Chrome plated"
  }
}
```

### Wind Instruments
```json
{
  "type": "Tenor Saxophone",
  "specifications": {
    "key": "Bb",
    "material": "Brass",
    "finish": "Lacquered",
    "keywork": "High F#",
    "mouthpiece": "Hard Rubber",
    "case": "Hard Shell with Plush Interior"
  }
}
```

## Example Usage Scenarios

### 1. Add Vintage Guitar
```javascript
POST /api/instruments
{
  "name": "1965 Fender Stratocaster",
  "type": "Electric Guitar",
  "brand": "Fender",
  "model": "Stratocaster",
  "year": 1965,
  "serialNumber": "L12345",
  "description": "Olympic White Stratocaster with original case",
  "condition": "Very Good",
  "value": 35000.00,
  "specifications": {
    "bodyWood": "Alder",
    "neckWood": "Maple",
    "fretboard": "Rosewood",
    "pickups": "3 Single Coils",
    "bridge": "Synchronized Tremolo",
    "finish": "Olympic White"
  },
  "maintenanceNotes": "Original electronics, refretted in 1995"
}
```

### 2. Add Modern Keyboard
```javascript
POST /api/instruments
{
  "name": "Nord Stage 3 88",
  "type": "Stage Piano",
  "brand": "Nord",
  "model": "Stage 3 88",
  "year": 2023,
  "serialNumber": "NS3-88-2023-5678",
  "description": "Professional stage piano with weighted keys",
  "condition": "Excellent",
  "value": 4500.00,
  "specifications": {
    "keys": "88 weighted",
    "engines": "Piano, Organ, Sample Synth",
    "polyphony": "120 voices",
    "effects": "Reverb, Delay, Chorus, Tremolo",
    "connectivity": "USB, MIDI, Audio I/O"
  }
}
```

### 3. Associate Instrument with Artist
```javascript
// After creating instrument, associate with artist
POST /api/artists/1/instruments/2
```

### 4. Get Artist's Instruments
```javascript
GET /api/artists/1?include=Instruments
```

### 5. Search by Type
```javascript
GET /api/instruments/search?q=Electric Guitar&include=Artists
```

## Database Schema

### Instruments Table
```sql
CREATE TABLE "Instruments" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Type" VARCHAR(100) NOT NULL,
    "Brand" VARCHAR(100),
    "Model" VARCHAR(100),
    "Year" INTEGER,
    "SerialNumber" VARCHAR(100),
    "Description" TEXT,
    "Condition" VARCHAR(50),
    "Value" DECIMAL(10,2),
    "Specifications" JSONB DEFAULT '{}',
    "ExternalSources" JSONB DEFAULT '[]',
    "MaintenanceNotes" TEXT,
    
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Artist-Instrument Relationship
```sql
CREATE TABLE "ArtistInstruments" (
    "ArtistId" INTEGER NOT NULL,
    "InstrumentId" INTEGER NOT NULL,
    "Role" VARCHAR(50), -- Owner, Borrower, etc.
    "StartDate" DATE,
    "EndDate" DATE,
    PRIMARY KEY ("ArtistId", "InstrumentId"),
    FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("InstrumentId") REFERENCES "Instruments" ("Id") ON DELETE CASCADE
);
```

### Indexes
```sql
CREATE INDEX "IX_Instruments_Type" ON "Instruments" ("Type");
CREATE INDEX "IX_Instruments_Brand" ON "Instruments" ("Brand");
CREATE INDEX "IX_Instruments_Model" ON "Instruments" ("Model");
CREATE INDEX "IX_Instruments_Year" ON "Instruments" ("Year");
CREATE INDEX "IX_Instruments_SerialNumber" ON "Instruments" ("SerialNumber");
CREATE INDEX "IX_Instruments_Value" ON "Instruments" ("Value");
```

## Integration Points

### Artist System
Instruments are owned and used by artists for performances and recordings.

### Event System
Instruments are tracked for specific performances through the EventArtist relationship.

### Insurance Systems
Value tracking and condition monitoring for insurance purposes.

### External Services
Links to instrument marketplaces, manufacturer databases, and service providers.

## Condition Classifications

### Condition Grades
- **Mint**: Perfect condition, like new
- **Excellent**: Very minor signs of use, no functional issues
- **Very Good**: Light wear, fully functional
- **Good**: Moderate wear, may have minor issues
- **Fair**: Heavy wear, functional with some limitations
- **Poor**: Significant wear or damage, may need repair

### Value Factors
- **Age and Rarity**: Vintage instruments often increase in value
- **Brand Recognition**: Premium brands maintain higher values
- **Condition**: Significant impact on market value
- **Modifications**: Can increase or decrease value
- **Provenance**: Famous ownership can dramatically increase value

## Maintenance Tracking

### Service Types
- **Setup and Adjustment**: Regular maintenance for optimal performance
- **Repair**: Fixing damage or worn components
- **Restoration**: Comprehensive refurbishment
- **Modification**: Adding or changing features
- **Cleaning**: Regular cleaning and preservation

### Documentation
- **Service Dates**: When maintenance was performed
- **Service Provider**: Who performed the work
- **Work Description**: Detailed description of service
- **Parts Replaced**: List of components changed
- **Cost**: Service costs for budgeting

## Best Practices

### 1. Accurate Documentation
- Record detailed specifications
- Maintain comprehensive condition notes
- Document all modifications and repairs
- Keep service records updated
- Include high-quality photos

### 2. Value Management
- Regularly update market values
- Track condition changes
- Document provenance and history
- Maintain insurance documentation
- Monitor market trends

### 3. Maintenance Scheduling
- Establish regular maintenance schedules
- Track usage for wear patterns
- Monitor environmental conditions
- Document any issues promptly
- Maintain service provider relationships

### 4. Security and Insurance
- Secure storage when not in use
- Comprehensive insurance coverage
- Serial number documentation
- Theft prevention measures
- Recovery procedures

### 5. Performance Integration
- Track which instruments used for performances
- Monitor performance-related wear
- Plan instrument rotation for tours
- Coordinate with event planning
- Maintain backup instruments

## Instrument Borrowing System

### Borrowing Workflow
1. **Request**: Artist requests to borrow instrument
2. **Approval**: Owner approves or denies request
3. **Agreement**: Terms and conditions established
4. **Transfer**: Instrument custody transferred
5. **Return**: Instrument returned with condition check

### Borrowing Records
```json
{
  "borrower": "Artist Name",
  "owner": "Owner Name",
  "startDate": "2024-03-01",
  "endDate": "2024-03-15",
  "purpose": "Recording session",
  "conditions": "Return in same condition",
  "deposit": 500.00,
  "insurance": "Borrower responsible"
}
```

## Integration with Performance Management

### Event Equipment Lists
- Track which instruments used for each performance
- Plan equipment needs for upcoming events
- Coordinate instrument transportation
- Monitor instrument usage patterns
- Prevent scheduling conflicts

### Tour Planning
- Plan instrument requirements for entire tour
- Coordinate shipping and logistics
- Arrange backup instruments
- Handle customs and international travel
- Manage insurance coverage during travel

The Instrument Management System provides comprehensive functionality for managing musical instruments with detailed specifications, condition tracking, and seamless integration with artist and event management for complete equipment lifecycle management.
