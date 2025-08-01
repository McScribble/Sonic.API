# 🏗️ Entity Service & Controller Implementation Guide

## 📋 **Overview**

This document provides a comprehensive guide to the `IEntityService<TDto, TCreateDto, TEntity>` and `EntityControllerExtensions` system, which forms the backbone of the Sonic API's generic entity management. This system provides a unified approach to CRUD operations, pagination, searching, includes, and cascading ownership across all entity types.

## 🎯 **Key Features**

### ✅ **1. Generic CRUD Operations**
- **Type-Safe** - Strongly typed with DTO pattern
- **Automated Mapping** - Uses Mapster for object mapping
- **Relationship Processing** - Automatic handling of navigation properties
- **Audit Fields** - Automatic CreatedAt/UpdatedAt/Uuid management

### ✅ **2. Advanced Querying**
- **Pagination** - Skip/take parameters with configurable limits
- **Includes** - Dynamic navigation property loading
- **Search** - Full-text search with multiple operators
- **Filtering** - Admin-based filtering and access control

### ✅ **3. Security & Authorization**
- **Resource Membership** - Fine-grained permission control
- **Cascading Ownership** - Inherited permissions through relationships
- **Admin Override** - Platform-wide admin privileges
- **Authentication Required** - All endpoints require valid JWT

### ✅ **4. Performance Optimization**
- **Lazy Loading** - Optional navigation property loading
- **Query Optimization** - Single queries for permission checks
- **Caching Ready** - Service-based architecture supports caching
- **Batch Operations** - Efficient bulk operations

## 🔧 **Implementation Details**

### **Core Components:**

1. **`IEntityService<TDto, TCreateDto, TEntity>`** - Generic service interface
2. **`EntityService<TDto, TCreateDto, TEntity>`** - Service implementation
3. **`EntityControllerExtensions`** - Generic controller endpoints
4. **`EntityRelationshipProcessor`** - Relationship management
5. **`QueryableExtensions`** - Query enhancement extensions
6. **`EntitySearch`** - Advanced search functionality

### **Type Constraints:**

```csharp
where TDto : GenericEntity          // Read/Update DTO
where TCreateDto : GenericCreateEntityDto  // Create DTO
where TEntity : GenericEntity       // Database entity
```

## 🎮 **Service Interface Methods**

### **Read Operations:**

```csharp
// Get all entities with optional includes
Task<IEnumerable<TDto>> GetAllAsync(string[]? includes = null);

// Get all entities with pagination and includes
Task<IEnumerable<TDto>> GetAllAsync(string[]? includes, int skip, int take);

// Get total count for pagination
Task<int> GetCountAsync();

// Get single entity by ID with optional includes
Task<TDto?> GetByIdAsync(int entityId, string[]? includes = null);
```

### **Search Operations:**

```csharp
// Search entities with optional includes
Task<IEnumerable<TDto>> SearchAsync(string searchTerm, string[]? includes = null);

// Search entities with pagination and includes
Task<IEnumerable<TDto>> SearchAsync(string searchTerm, string[]? includes, int skip, int take);
```

### **Write Operations:**

```csharp
// Create new entity
Task<TDto?> CreateAsync(TCreateDto createDto);

// Update existing entity
Task<TDto?> UpdateAsync(TDto updateDto);

// Delete entity by ID
Task<bool> DeleteAsync(int entityId);
```

## 🌐 **Controller Endpoints**

### **Generated Endpoints for Each Entity:**

| Method | Endpoint | Description | Features |
|--------|----------|-------------|----------|
| `GET` | `/api/{entity}s/{id}` | Get by ID | Includes, Resource Permissions |
| `GET` | `/api/{entity}s` | Get all | Pagination, Includes, Admin Filtering |
| `POST` | `/api/{entity}s` | Create | Auto-ownership, Relationship Processing |
| `PUT` | `/api/{entity}s` | Update | Resource Permissions, Relationship Processing |
| `DELETE` | `/api/{entity}s/{id}` | Delete | Resource Permissions |
| `GET` | `/api/{entity}s/search` | Search | Pagination, Includes, Search Operators |

### **Controller Registration Example:**

```csharp
// In Program.cs - Register entity endpoints
app.MapEntityEndpoints<VenueDto, VenueCreateDto, Venue>(
    ResourceType.Venue,
    new Dictionary<EndpointTypes, MembershipType[]>
    {
        { EndpointTypes.Get, new[] { MembershipType.Viewer, MembershipType.Member, MembershipType.Owner } },
        { EndpointTypes.Update, new[] { MembershipType.Owner, MembershipType.Manager } },
        { EndpointTypes.Delete, new[] { MembershipType.Owner } }
    });

app.MapEntityEndpoints<EventDto, EventCreateDto, Event>(
    ResourceType.Event,
    new Dictionary<EndpointTypes, MembershipType[]>
    {
        { EndpointTypes.Get, new[] { MembershipType.Viewer, MembershipType.Member, MembershipType.Owner } },
        { EndpointTypes.Update, new[] { MembershipType.Owner, MembershipType.Manager } },
        { EndpointTypes.Delete, new[] { MembershipType.Owner } }
    });
```

## 🔍 **Advanced Features**

### **1. Include System**

**Query Parameter:**
```
GET /api/events/123?include=Venue,Organizers,Lineup
```

**Nested Includes:**
```
GET /api/events/123?include=Venue.Address,Organizers.ContactInfo
```

**Implementation:**
```csharp
public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, string[]? includes)
{
    if (includes == null || !includes.Any()) return query;
    
    foreach (var include in includes)
    {
        // Validates include path exists before applying
        query = ApplyIncludePath(query, include.Trim());
    }
    return query;
}
```

### **2. Pagination System**

**Query Parameters:**
```
GET /api/events?skip=20&take=10
```

**Response Format:**
```json
{
  "data": [...],
  "totalCount": 150,
  "skip": 20,
  "take": 10,
  "hasMore": true
}
```

**Configuration:**
```csharp
const int DefaultTake = 50;  // Default page size
const int MaxTake = 50;      // Maximum page size (prevents abuse)
```

### **3. Search System**

**Simple Search:**
```
GET /api/events/search?q=concert&include=Venue
```

**Advanced Search Operators:**
- **Contains** - Partial string matching
- **Equals** - Exact string matching  
- **StartsWith** - Prefix matching
- **EndsWith** - Suffix matching
- **Like** - Fuzzy matching using Levenshtein distance

**Search Implementation:**
```csharp
public EntitySearch ParseSearchTerms(string searchString)
{
    // Parses search string into structured search terms
    // Supports field:value syntax and operators
    // Example: "name:concert location:seattle"
}
```

### **4. Relationship Processing**

**Automatic Relationship Handling:**
```csharp
public async Task ProcessRelationshipsAsync<TEntity, TDto>(TEntity entity, TDto dto)
{
    // Handles navigation properties automatically
    // Links to existing entities (no creation)
    // Supports collections and single references
    // Updates many-to-many relationships
}
```

**Supported Relationship Types:**
- **One-to-One** - Single navigation properties
- **One-to-Many** - Collection navigation properties
- **Many-to-Many** - Junction table relationships

## 🔒 **Security & Authorization**

### **Resource Membership Configuration:**

```csharp
var membershipConfig = new Dictionary<EndpointTypes, MembershipType[]>
{
    { EndpointTypes.Get, new[] { MembershipType.Viewer, MembershipType.Member, MembershipType.Owner } },
    { EndpointTypes.Create, new[] { MembershipType.Member, MembershipType.Owner } },
    { EndpointTypes.Update, new[] { MembershipType.Owner, MembershipType.Manager } },
    { EndpointTypes.Delete, new[] { MembershipType.Owner } }
};
```

### **Permission Check Flow:**

1. **Authentication** - JWT token validation
2. **Admin Check** - Bypass all permissions if admin
3. **Direct Ownership** - Check ResourceMembership records
4. **Cascading Ownership** - Check ownership through relationships
5. **Access Decision** - Grant/deny based on membership

### **Cascading Ownership Integration:**

```csharp
// Check cascading permissions (if entity supports it)
var hasPermission = await permissionService.CheckCascadingResourcePermissionAsync<Event>(
    context, eventId, requiredMemberships);
```

## 📊 **Usage Examples**

### **Service Usage:**

```csharp
public class EventService : EntityService<EventDto, EventCreateDto, Event>
{
    public EventService(SonicDbContext context) : base(context) { }
    
    // Can override methods for custom behavior
    public override async Task<EventDto?> CreateAsync(EventCreateDto createDto)
    {
        // Custom validation or processing
        if (createDto.Date < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past");
            
        return await base.CreateAsync(createDto);
    }
}
```

### **Controller Customization:**

```csharp
// Add custom endpoints alongside generic ones
app.MapEntityEndpoints<EventDto, EventCreateDto, Event>(ResourceType.Event, membershipConfig);

// Custom endpoint
app.MapGet("/api/events/upcoming", async (IEntityService<EventDto, EventCreateDto, Event> service) =>
{
    var upcomingEvents = await service.SearchAsync($"Date:>{DateTime.UtcNow:yyyy-MM-dd}");
    return Results.Ok(upcomingEvents);
});
```

### **Client Usage Examples:**

```javascript
// Get event with related data
GET /api/events/123?include=Venue,Organizers,Lineup

// Get paginated events
GET /api/events?skip=0&take=20&include=Venue

// Search events
GET /api/events/search?q=concert&include=Venue&skip=0&take=10

// Create event
POST /api/events
{
  "name": "Summer Concert",
  "date": "2025-08-15T19:00:00Z",
  "venueId": 456,
  "organizerIds": [123, 789]
}
```

## ⚡ **Performance Considerations**

### **Query Optimization:**
- **Single Queries** - Permission checks combined where possible
- **Lazy Loading** - Includes only loaded when requested
- **Pagination** - Limits result set size
- **Index Support** - Optimized for common query patterns

### **Caching Strategy:**
- **Service Level** - Can be wrapped with caching decorators
- **Response Caching** - HTTP caching headers supported
- **Entity Caching** - Frequently accessed entities cached

### **Memory Management:**
- **Streaming Results** - IAsyncEnumerable support for large datasets
- **Connection Pooling** - DbContext lifecycle managed by DI
- **Resource Disposal** - Proper disposal patterns

## 🚀 **Best Practices**

### **1. DTO Design:**
```csharp
// Separate DTOs for different operations
public class VenueDto : GenericEntity { /* Full data */ }
public class VenueCreateDto : GenericCreateEntityDto { /* Creation data */ }
public class VenueSummaryDto { /* List view data */ }
```

### **2. Service Registration:**
```csharp
// Register services with proper lifetime
builder.Services.AddScoped<IEntityService<VenueDto, VenueCreateDto, Venue>>(
    provider => new EntityService<VenueDto, VenueCreateDto, Venue>(
        provider.GetRequiredService<SonicDbContext>()));
```

### **3. Error Handling:**
```csharp
try
{
    var result = await entityService.CreateAsync(createDto);
    return Results.Created($"/api/venues/{result.Id}", result);
}
catch (ArgumentException ex)
{
    return Results.BadRequest(ex.Message);
}
catch (Exception ex)
{
    Log.Error(ex, "Error creating venue");
    return Results.InternalServerError("An error occurred");
}
```

### **4. Validation:**
```csharp
// Use FluentValidation or similar
public class VenueCreateDtoValidator : AbstractValidator<VenueCreateDto>
{
    public VenueCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}
```

## 🎉 **Summary**

The Entity Service & Controller system provides a comprehensive, type-safe foundation for building RESTful APIs with advanced features:

- **🎯 Generic & Reusable** - Single implementation works for all entities
- **🔒 Secure** - Built-in authentication and authorization
- **⚡ Performant** - Optimized queries and caching support
- **🧪 Testable** - Service-based architecture with dependency injection
- **📈 Scalable** - Supports pagination, includes, and search
- **🛠️ Maintainable** - Clear separation of concerns and extensible design

This system dramatically reduces boilerplate code while providing enterprise-grade features for entity management, making it easy to build consistent, secure, and performant APIs.
