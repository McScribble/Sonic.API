using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sonic.API.Data;
using Sonic.Models;

namespace Sonic.API.Services;

public class EntityRelationshipProcessor
{
    private readonly SonicDbContext _context;

    public EntityRelationshipProcessor(SonicDbContext context)
    {
        _context = context;
    }

    public async Task ProcessRelationshipsAsync<TEntity, TDto>(TEntity entity, TDto dto) 
        where TEntity : GenericEntity
        where TDto : GenericEntity
    {
        await ProcessRelationshipsInternalAsync(entity, dto);
    }

    public async Task ProcessRelationshipsForCreateAsync<TEntity, TCreateDto>(TEntity entity, TCreateDto createDto)
        where TEntity : GenericEntity
        where TCreateDto : GenericCreateEntityDto
    {
        await ProcessRelationshipsInternalAsync(entity, createDto);
    }

    private async Task ProcessRelationshipsInternalAsync<TEntity>(TEntity entity, object dto) 
        where TEntity : GenericEntity
    {
        var entityType = typeof(TEntity);
        var dtoType = dto.GetType();

        // Find all navigation properties in the entity
        var navigationProperties = entityType.GetProperties()
            .Where(p => IsNavigationProperty(p))
            .ToList();

        foreach (var navProperty in navigationProperties)
        {
            // Find corresponding property in DTO
            var dtoProperty = dtoType.GetProperty(navProperty.Name);
            if (dtoProperty == null) continue;

            var dtoValue = dtoProperty.GetValue(dto);
            if (dtoValue == null) continue;

            await ProcessNavigationProperty(entity, navProperty, dtoValue);
        }
    }

    private bool IsNavigationProperty(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        
        // Check if it's a single GenericEntity
        if (typeof(GenericEntity).IsAssignableFrom(propertyType))
            return true;

        // Check if it's a collection of GenericEntity
        if (propertyType.IsGenericType)
        {
            var genericArgs = propertyType.GetGenericArguments();
            if (genericArgs.Length == 1 && typeof(GenericEntity).IsAssignableFrom(genericArgs[0]))
                return true;
        }

        return false;
    }

    private async Task ProcessNavigationProperty<TEntity>(TEntity entity, PropertyInfo navProperty, object dtoValue) 
        where TEntity : class
    {
        var propertyType = navProperty.PropertyType;

        if (typeof(IEnumerable<GenericEntity>).IsAssignableFrom(propertyType) && propertyType != typeof(string))
        {
            // Handle collections
            await ProcessCollectionProperty(entity, navProperty, dtoValue);
        }
        else if (typeof(GenericEntity).IsAssignableFrom(propertyType))
        {
            // Handle single entity
            await ProcessSingleProperty(entity, navProperty, dtoValue);
        }
    }

    private async Task ProcessCollectionProperty<TEntity>(TEntity entity, PropertyInfo navProperty, object dtoValue) 
        where TEntity : class
    {
        var collectionType = navProperty.PropertyType;
        var relatedEntityType = GetRelatedEntityType(collectionType);
        if (relatedEntityType == null) return;

        var dtoCollection = (IEnumerable<object>)dtoValue;
        var validIds = new List<int>();

        // Extract valid IDs from the DTO collection
        foreach (var dtoItem in dtoCollection)
        {
            var idProperty = dtoItem.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var id = (int)idProperty.GetValue(dtoItem)!;
                if (id > 0)
                {
                    validIds.Add(id);
                }
            }
        }

        // ✅ Use a generic method to handle the type-safe loading and setting
        await SetCollectionProperty(entity, navProperty, relatedEntityType, validIds);
    }

    // ✅ New generic method to handle type-safe collection setting
    private async Task SetCollectionProperty<TEntity>(TEntity entity, PropertyInfo navProperty, Type relatedEntityType, List<int> validIds)
        where TEntity : class
    {
        // Use reflection to call the generic method with the correct type
        var method = typeof(EntityRelationshipProcessor)
            .GetMethod(nameof(SetCollectionPropertyGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(typeof(TEntity), relatedEntityType);

        await (Task)method.Invoke(this, new object[] { entity, navProperty, validIds })!;
    }

    // ✅ Type-safe generic method
    private async Task SetCollectionPropertyGeneric<TEntity, TRelated>(TEntity entity, PropertyInfo navProperty, List<int> validIds)
        where TEntity : GenericEntity
        where TRelated : GenericEntity
    {
        if (!validIds.Any())
        {
            navProperty.SetValue(entity, new List<TRelated>());
            return;
        }

        // Load entities with correct type
        var existingEntities = await _context.Set<TRelated>()
            .Where(e => validIds.Contains(e.Id))
            .ToListAsync();

        // Set the properly typed collection
        navProperty.SetValue(entity, existingEntities);
        
        Console.WriteLine($"Linked {existingEntities.Count} existing {typeof(TRelated).Name}(s)");
    }

    private async Task ProcessSingleProperty<TEntity>(TEntity entity, PropertyInfo navProperty, object dtoValue) 
        where TEntity : class
    {
        var relatedEntityType = navProperty.PropertyType;
        
        // Get the ID from the DTO item
        var idProperty = dtoValue.GetType().GetProperty("Id");
        if (idProperty == null) return;

        var id = (int)idProperty.GetValue(dtoValue)!;
        if (id <= 0) return; // Only link to existing entities

        // Load existing entity only
        var existingEntity = await LoadExistingEntity(relatedEntityType, id);
        navProperty.SetValue(entity, existingEntity);

        if (existingEntity != null)
        {
            Console.WriteLine($"Linked to existing {relatedEntityType.Name} with ID: {id}");
        }
        else
        {
            Console.WriteLine($"Warning: {relatedEntityType.Name} with ID {id} not found");
        }
    }

    private async Task<IEnumerable<object>> LoadExistingEntitiesByIds(Type entityType, List<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<object>();

        // Use reflection to call the generic method
        var method = typeof(EntityRelationshipProcessor)
            .GetMethod(nameof(LoadExistingEntitiesByIdsGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityType);

        var task = (Task<IEnumerable<object>>)method.Invoke(this, new object[] { ids })!;
        return await task;
    }

    private async Task<IEnumerable<object>> LoadExistingEntitiesByIdsGeneric<T>(List<int> ids) where T : GenericEntity
    {
        var entities = await _context.Set<T>()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();
        return entities.Cast<object>();
    }

    private async Task<object?> LoadExistingEntity(Type entityType, int id)
    {
        // Use reflection to call the generic method
        var method = typeof(EntityRelationshipProcessor)
            .GetMethod(nameof(LoadExistingEntityGeneric), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityType);

        var task = (Task<object?>)method.Invoke(this, new object[] { id })!;
        return await task;
    }

    private async Task<object?> LoadExistingEntityGeneric<T>(int id) where T :GenericEntity
    {
        return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
    }

    private Type? GetRelatedEntityType(Type navigationPropertyType)
    {
        if (navigationPropertyType.IsGenericType)
        {
            var genericArgs = navigationPropertyType.GetGenericArguments();
            return genericArgs.FirstOrDefault();
        }
        
        return navigationPropertyType;
    }
}