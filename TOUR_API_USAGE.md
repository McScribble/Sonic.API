# Tour API Usage Guide

## Overview
Tours now use the **generic entity endpoints** instead of custom controller methods. This provides consistent API patterns across all entities and reduces code duplication.

## Available Endpoints

### 1. Get All Tours
```
GET /api/tours
GET /api/tours?page=1&pageSize=10
GET /api/tours?include=Artists,Shows
```

**Response:** Array of `TourDto` objects with pagination

### 2. Get Tour by ID
```
GET /api/tours/{id}
GET /api/tours/{id}?include=Artists,Shows
```

**Response:** Single `TourDto` object

### 3. Search Tours
```
GET /api/tours/search?q=searchterm
GET /api/tours/search?q=artist name&include=Artists,Shows
```

**Response:** Array of matching `TourDto` objects

### 4. Create Tour
```
POST /api/tours
Content-Type: application/json

{
  "name": "World Tour 2024",
  "description": "Amazing world tour",
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "startCity": "New York",
  "endCity": "Los Angeles",
  "artistIds": [1, 2]
}
```

**Response:** Created `TourDto` object

### 5. Update Tour
```
PUT /api/tours/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Tour Name",
  "description": "Updated description",
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "startCity": "Boston",
  "endCity": "San Francisco",
  "artistIds": [1, 2],
  "showIds": [5, 6, 7]
}
```

**Response:** Updated `TourDto` object

### 6. Delete Tour
```
DELETE /api/tours/{id}
```

**Response:** 204 No Content

## Common Use Cases

### Find Tours by Artist
```
GET /api/tours/search?q=artist name&include=Artists
```

### Get Active Tours
```
GET /api/tours?include=Shows
```
Then filter client-side for tours where current date is between `startDate` and `endDate`.

### Add Show to Tour
1. Get the current tour: `GET /api/tours/{id}?include=Shows`
2. Add the show ID to the `showIds` array
3. Update: `PUT /api/tours/{id}` with the updated data

### Remove Show from Tour
1. Get the current tour: `GET /api/tours/{id}?include=Shows`
2. Remove the show ID from the `showIds` array
3. Update: `PUT /api/tours/{id}` with the updated data

## Include Options
- `Artists` - Include associated artists
- `Shows` - Include associated events (shows)
- `Artists,Shows` - Include both

## Benefits of Generic Endpoints

✅ **Consistency**: Same API patterns across all entities
✅ **Flexibility**: Built-in pagination, search, and includes
✅ **Maintainability**: Less code to maintain
✅ **Security**: Automatic resource membership checking
✅ **Performance**: Optimized queries with proper includes

## Migration from Custom Controller and Service

The following custom components have been **completely removed**:

### Removed Files:
- ❌ `Controllers/TourController.cs` - Custom controller with specific endpoints
- ❌ `Services/TourService.cs` - Custom service with specialized methods  
- ❌ `Services/ITourService.cs` - Custom service interface

### Removed Methods:
| Old Custom Method | New Generic Endpoint |
|-------------------|---------------------|
| `GetToursByArtist(artistId)` | `GET /api/tours/search?q=artistName&include=Artists` |
| `GetActiveTours()` | `GET /api/tours?include=Shows` + client-side filtering |
| `AddShowToTour(tourId, showId)` | `PUT /api/tours/{id}` with updated showIds |
| `RemoveShowFromTour(tourId, showId)` | `PUT /api/tours/{id}` with updated showIds |

### What Remains:
✅ Generic `IEntityService<TourDto, TourCreateDto, Tour>` registration in `Program.cs`
✅ Generic entity endpoints via `MapEntityEndpoints<TourDto, TourCreateDto, Tour>`
✅ All Tour models (`Tour.cs`, `TourDto.cs`, `TourCreateDto.cs`, `TourSummaryDto.cs`)
✅ Database configuration and migrations

The generic approach provides **more flexibility** and maintains **consistency** with the rest of the API, while reducing code duplication and maintenance overhead.
