# 🏗️ Cascading Ownership System Implementation Guide

## 📋 **Overview**

This document outlines the implementation of a cascading ownership system that allows ownership to flow through entity relationships. This enables scenarios where:

- **Event owners** can manage events at venues they own
- **Venue owners** can manage all events at their venues
- **Event organizers** can manage events they organize
- **Multiple ownership paths** can exist for the same entity

## 🎯 **Key Features**

### ✅ **1. Attribute-Based Configuration**
- **`[CascadeOwnershipFrom]`** - Declares that an entity inherits ownership from related entities
- **`[CascadeOwnershipTo]`** - Declares that an entity grants ownership to related entities  
- **`[DirectOwnership]`** - Declares that an entity supports direct ResourceMembership records

### ✅ **2. Multi-Level Ownership Resolution**
- **Direct Ownership** - Traditional ResourceMembership records
- **Single Navigation** - Ownership through single related entity (Event → Venue)
- **Collection Navigation** - Ownership through collections (Event → Organizers)
- **Priority System** - Lower priority numbers checked first

### ✅ **3. Dynamic Resolution**
- **Reflection-Based** - Automatically discovers relationships through attributes
- **Type-Safe** - Compile-time type checking for entity relationships
- **Extensible** - Easy to add new ownership patterns

## 🔧 **Implementation Details**

### **Core Files Created:**

1. **`Models/Base/OwnershipAttributes.cs`** - Attribute definitions
2. **`Services/IResourcePermissionService.cs`** - Extended interface 
3. **`Services/ResourcePermissionService.cs`** - Enhanced with cascading logic
4. **`Services/DynamicOwnershipResolver.cs`** - Advanced dynamic resolution
5. **`Examples/CascadingOwnershipUsage.cs`** - Usage examples

### **Model Configuration Example:**

```csharp
// Event can inherit ownership from Venue and Organizers
[CascadeOwnershipFrom("Venue", typeof(Venue), Priority = 10)]
[CascadeOwnershipFrom("Organizers", typeof(User), Priority = 20)]
[DirectOwnership(ResourceType.Event)]
public class Event : GenericEntity
{
    public Venue? Venue { get; set; }
    public List<User> Organizers { get; set; } = new();
    // ... other properties
}

// Venue grants ownership to Events
[CascadeOwnershipTo("Events", typeof(Event))]
[DirectOwnership(ResourceType.Venue)]
public class Venue : GenericEntity
{
    public List<Event> Events { get; set; } = new();
    // ... other properties
}
```

### **Usage in Controllers:**

```csharp
// Check cascading permissions
var hasPermission = await permissionService.CheckCascadingResourcePermissionAsync<Event>(
    context, 
    eventId, 
    new[] { MembershipType.Owner, MembershipType.Manager });

if (!hasPermission)
{
    return Results.Forbid();
}
```

## 🔄 **Permission Resolution Flow**

1. **Admin Check** - Admins bypass all ownership checks
2. **Direct Ownership** - Check ResourceMembership records for the entity
3. **Cascading Ownership** - Check ownership through related entities:
   - Order by Priority (lower = higher priority)
   - Single navigation properties (Event.Venue)
   - Collection navigation properties (Event.Organizers)
4. **Return Result** - True if ANY ownership path grants access

## 📊 **Ownership Scenarios Supported**

| Scenario | Implementation | Example |
|----------|----------------|---------|
| **Direct Ownership** | ResourceMembership record | User owns Event directly |
| **Single Relation** | Event → Venue | Venue owner can manage events at venue |
| **Collection Relation** | Event → Organizers | Organizers can manage their events |
| **Multi-Path** | Multiple attributes | Event accessible via venue OR organizer ownership |
| **Priority Resolution** | Priority attribute | Check venue ownership before organizer ownership |

## 🛠️ **Service Registration**

```csharp
// In Program.cs
builder.Services.AddScoped<IResourcePermissionService, ResourcePermissionService>();
builder.Services.AddScoped<DynamicOwnershipResolver>();
```

## 🔍 **Advanced Features**

### **Dynamic Ownership Resolver**
- Uses reflection to discover relationships
- Builds dynamic LINQ expressions
- Supports complex navigation patterns
- Extensible for new entity types

### **Performance Considerations**
- **Single Query** - Admin and direct ownership checked in one query
- **Lazy Evaluation** - Cascading checks only run if direct ownership fails
- **Priority Ordering** - Most likely ownership paths checked first
- **Caching Potential** - Attribute metadata can be cached

## 🚀 **Future Enhancements**

1. **Attribute Caching** - Cache reflection results for better performance
2. **Custom Validators** - Allow custom ownership validation logic
3. **Audit Trail** - Track which ownership path granted access
4. **Time-Based Ownership** - Support temporary or scheduled ownership
5. **Conditional Cascading** - Rules-based cascading (e.g., only cascade certain membership types)

## 📈 **Scalability Benefits**

- **Declarative Configuration** - No code changes needed for new relationships
- **Type Safety** - Compile-time checking prevents configuration errors
- **Maintainable** - Ownership logic centralized in service layer
- **Testable** - Service can be easily mocked for unit tests
- **Extensible** - New ownership patterns added via attributes

## 🎉 **Summary**

This cascading ownership system provides a robust, scalable foundation for complex multi-tenant scenarios. By using attributes to declare relationships and a service-based architecture for resolution, you can support sophisticated ownership patterns while maintaining clean, maintainable code.

The system automatically handles the complexity of checking multiple ownership paths while providing excellent performance and type safety.
