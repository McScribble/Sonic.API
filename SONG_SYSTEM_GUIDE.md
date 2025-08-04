# Song Management System Guide

## Overview
The Song Management System manages musical compositions, tracks, and recordings. It provides comprehensive song metadata, Spotify integration for music discovery, artist associations, and performance tracking across events and tours.

## Entity Structure

### Song Entity
- **Metadata**: Title, genre, duration, release information
- **Artists**: Associated performers and composers
- **External Integration**: Spotify track data and streaming links
- **Performance**: Usage in events, tours, and setlists
- **Content**: Lyrics, credits, and detailed information

## API Endpoints

### Get All Songs
```
GET /api/songs
GET /api/songs?page=1&pageSize=10
GET /api/songs?include=Artists
```

### Get Song by ID
```
GET /api/songs/{id}
GET /api/songs/{id}?include=Artists
```

### Search Songs
```
GET /api/songs/search?q=searchterm
GET /api/songs/search?q=Bohemian Rhapsody&include=Artists
```

### Create Song
```
POST /api/songs
Content-Type: application/json

{
  "title": "Midnight Journey",
  "genre": "Jazz",
  "duration": "00:04:32",
  "releaseDate": "2024-03-15T00:00:00Z",
  "album": "Evening Stories",
  "trackNumber": 3,
  "isrc": "USAT21234567",
  "description": "A contemplative jazz piece exploring late-night urban landscapes",
  "lyrics": "Walking through the city lights...",
  "externalSources": [
    {
      "name": "Spotify",
      "url": "https://open.spotify.com/track/4iV5W9uYEdYUVa79Axb7Rh",
      "platform": "Spotify"
    }
  ],
  "credits": [
    {
      "type": "Composer",
      "value": "John Smith",
      "label": "Music and Lyrics"
    }
  ]
}
```

### Update Song
```
PUT /api/songs/{id}
Content-Type: application/json

{
  "id": 1,
  "title": "Updated Song Title",
  "genre": "Alternative Jazz",
  // ... other fields
}
```

## Data Models

### Song Entity
```csharp
public class Song : GenericEntity
{
    public required string Title { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
    public string? ISRC { get; set; }
    public string? Description { get; set; }
    public string? Lyrics { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Credits { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Artist> Artists { get; set; }
    public virtual ICollection<EventArtist> EventArtists { get; set; }
}
```

### SongDto (API Response)
```csharp
public class SongDto : GenericEntity
{
    public required string Title { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
    public string? ISRC { get; set; }
    public string? Description { get; set; }
    public string? Lyrics { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Credits { get; set; }
    public List<ArtistSummaryDto> Artists { get; set; }
}
```

### SongSummaryDto (Reference)
```csharp
public class SongSummaryDto : GenericEntity
{
    public required string Title { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
}
```

### SongCreateDto (API Input)
```csharp
public class SongCreateDto : GenericCreateEntityDto
{
    public required string Title { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Album { get; set; }
    public int? TrackNumber { get; set; }
    public string? ISRC { get; set; }
    public string? Description { get; set; }
    public string? Lyrics { get; set; }
    public List<ExternalSource> ExternalSources { get; set; }
    public List<ContactInfo> Credits { get; set; }
}
```

## Permission System

### Song Permissions
- **View**: Viewer, Member, Manager, Owner
- **Update**: Manager, Owner
- **Delete**: Owner

### Resource Membership Roles
- **Owner**: Full control over the song
- **Manager**: Can edit song details and metadata
- **Member**: Can view and reference song in events
- **Viewer**: Read-only access to song information

## Relationships

### Artists
- Many-to-many relationship with Artists
- Songs can have multiple associated artists (performers, composers, features)
- Artists can have multiple songs in their repertoire
- Support for different artist roles (lead, feature, composer, etc.)

### Events
- Many-to-many relationship through EventArtist
- Songs can be performed at multiple events
- Events can feature multiple songs
- Setlist and performance tracking

## Features

### Spotify Integration
- **Search**: Find songs on Spotify by title and artist
- **Track Data**: Import duration, album, and metadata
- **Streaming Links**: Direct links to Spotify tracks
- **Album Art**: Retrieve album artwork from Spotify
- **Artist Matching**: Match songs to Spotify artist profiles

### Metadata Management
- **ISRC Codes**: International Standard Recording Code tracking
- **Release Information**: Album, track number, release date
- **Duration**: Precise track timing
- **Genre Classification**: Musical style categorization
- **Credits**: Composer, lyricist, producer information

### Content Management
- **Lyrics**: Full song lyrics with formatting
- **Descriptions**: Song background and story
- **Credits**: Detailed contributor information
- **External Links**: Streaming platforms, music videos, sheet music

### Performance Tracking
- **Event Usage**: Track which events feature the song
- **Setlist Position**: Order within event performances
- **Performance History**: Historical performance data
- **Popularity Metrics**: Most performed songs analytics

## Spotify Integration Details

### Search Spotify Tracks
```
GET /api/spotify/search?query=song title artist&type=track
```

### Response Structure
```json
{
  "tracks": {
    "items": [
      {
        "id": "4iV5W9uYEdYUVa79Axb7Rh",
        "name": "Bohemian Rhapsody",
        "artists": [
          {
            "id": "1dfeR4HaWDbWqFHLkxsg1d",
            "name": "Queen"
          }
        ],
        "album": {
          "id": "6i6folBtxKV28WX3ZgUIez",
          "name": "A Night at the Opera",
          "images": [
            {
              "url": "https://i.scdn.co/image/ab67616d0000b273e319baafd16e84f0408af2a0",
              "height": 640,
              "width": 640
            }
          ]
        },
        "duration_ms": 355000,
        "external_urls": {
          "spotify": "https://open.spotify.com/track/4iV5W9uYEdYUVa79Axb7Rh"
        }
      }
    ]
  }
}
```

## Example Usage Scenarios

### 1. Create Song with Spotify Data
```javascript
// First, search Spotify
GET /api/spotify/search?query=Stairway to Heaven Led Zeppelin&type=track

// Then create song with Spotify metadata
POST /api/songs
{
  "title": "Stairway to Heaven",
  "genre": "Rock",
  "duration": "00:08:02",
  "album": "Led Zeppelin IV",
  "trackNumber": 4,
  "releaseDate": "1971-11-08T00:00:00Z",
  "externalSources": [
    {
      "name": "Spotify",
      "url": "https://open.spotify.com/track/5CQ30WqJwcep0pYcV4AMNc",
      "platform": "Spotify"
    }
  ]
}
```

### 2. Create Original Composition
```javascript
POST /api/songs
{
  "title": "Dreams of Tomorrow",
  "genre": "Indie Folk",
  "duration": "00:03:45",
  "description": "An introspective piece about hope and future aspirations",
  "lyrics": "In the quiet of the morning...",
  "credits": [
    {
      "type": "Composer",
      "value": "Sarah Johnson",
      "label": "Music and Lyrics"
    },
    {
      "type": "Producer",
      "value": "Mike Wilson",
      "label": "Recording Producer"
    }
  ]
}
```

### 3. Add Song to Artist
```javascript
// After creating song, associate with artist
POST /api/artists/1/songs/2
```

### 4. Get Songs by Artist
```javascript
GET /api/artists/1?include=Songs
```

### 5. Search Songs with Filters
```javascript
GET /api/songs/search?q=rock&include=Artists
GET /api/songs?include=Artists&page=1&pageSize=20
```

## Database Schema

### Songs Table
```sql
CREATE TABLE "Songs" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Genre" VARCHAR(100),
    "Duration" INTERVAL,
    "ReleaseDate" DATE,
    "Album" VARCHAR(200),
    "TrackNumber" INTEGER,
    "ISRC" VARCHAR(12),
    "Description" TEXT,
    "Lyrics" TEXT,
    "ExternalSources" JSONB DEFAULT '[]',
    "Credits" JSONB DEFAULT '[]',
    
    -- Standard GenericEntity fields
    "Uuid" UUID NOT NULL DEFAULT gen_random_uuid(),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Emoji" TEXT
);
```

### Artist-Song Relationship
```sql
CREATE TABLE "ArtistSongs" (
    "ArtistId" INTEGER NOT NULL,
    "SongId" INTEGER NOT NULL,
    PRIMARY KEY ("ArtistId", "SongId"),
    FOREIGN KEY ("ArtistId") REFERENCES "Artists" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("SongId") REFERENCES "Songs" ("Id") ON DELETE CASCADE
);
```

### Indexes
```sql
CREATE INDEX "IX_Songs_Title" ON "Songs" ("Title");
CREATE INDEX "IX_Songs_Genre" ON "Songs" ("Genre");
CREATE INDEX "IX_Songs_Album" ON "Songs" ("Album");
CREATE INDEX "IX_Songs_ReleaseDate" ON "Songs" ("ReleaseDate");
CREATE INDEX "IX_Songs_ISRC" ON "Songs" ("ISRC");
```

## Integration Points

### Artist System
Songs are associated with artists for repertoire management and performance tracking.

### Event System
Songs are used in event setlists and performance lineups through the EventArtist relationship.

### Spotify Service
Direct integration with Spotify Web API for music discovery and metadata enrichment.

### External Services
Links to streaming platforms, music videos, and digital sheet music services.

## Music Metadata Standards

### ISRC (International Standard Recording Code)
- **Format**: CC-XXX-YY-NNNNN
- **Country Code**: Two-letter country identifier
- **Registrant Code**: Three-character organization code
- **Year**: Two-digit year of registration
- **Designation**: Five-digit unique identifier

### Duration Format
- **Database**: PostgreSQL INTERVAL type
- **API**: ISO 8601 duration format (PT4M32S)
- **Display**: Human-readable format (4:32)

### Genre Classification
- **Primary**: Main musical style
- **Subgenres**: Detailed style classification
- **Cross-Genre**: Multiple genre tags
- **Evolution**: Genre changes over time

## Best Practices

### 1. Metadata Accuracy
- Verify song titles and artist names
- Use official release dates and album names
- Include ISRC codes when available
- Maintain consistent genre classifications

### 2. Spotify Integration
- Search with artist name for better matches
- Verify track duration matches
- Check album and release year consistency
- Handle multiple versions of same song

### 3. Content Management
- Format lyrics consistently
- Include detailed credit information
- Maintain external source links
- Update streaming platform URLs

### 4. Performance Tracking
- Associate songs with correct artists
- Track setlist positions accurately
- Monitor song popularity metrics
- Maintain performance history

### 5. Rights Management
- Track composer and publisher information
- Include licensing and rights data
- Maintain copyright information
- Handle cover versions appropriately

## Song Discovery Workflow

### 1. Spotify Search
```javascript
// User searches for existing song
GET /api/spotify/search?query=user input&type=track

// System returns matching tracks with metadata
// User selects correct track
```

### 2. Import Metadata
```javascript
// System pre-fills form with Spotify data
{
  "title": "Track Name",
  "duration": "PT3M45S",
  "album": "Album Name",
  "externalSources": [{
    "name": "Spotify",
    "url": "https://open.spotify.com/track/...",
    "platform": "Spotify"
  }]
}
```

### 3. Enhance with Local Data
```javascript
// User adds local information
{
  "description": "Live version from concert tour",
  "lyrics": "Full song lyrics...",
  "credits": [
    { "type": "Composer", "value": "Artist Name" }
  ]
}
```

### 4. Create and Associate
```javascript
// Create song and associate with artists
POST /api/songs
// Then associate with artists/events as needed
```

The Song Management System provides comprehensive functionality for managing musical compositions with rich metadata, Spotify integration for discovery, and flexible association with artists and events for complete repertoire and performance management.
