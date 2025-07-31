using Microsoft.EntityFrameworkCore;
using Mapster;
using Sonic.Models;
using Serilog;
using Sonic.API.Data;
using Sonic.API.Extensions;

namespace Sonic.API.Services;

public class EntityService<TDto, TCreateDto, TEntity> : IEntityService<TDto, TCreateDto, TEntity>
    where TEntity : GenericEntity
    where TDto : GenericEntity
    where TCreateDto : GenericCreateEntityDto
{
    protected readonly SonicDbContext _context;
    private readonly EntityRelationshipProcessor _relationshipProcessor;

    public EntityService(SonicDbContext context)
    {
        _context = context;
        _relationshipProcessor = new EntityRelationshipProcessor(context);
    }

    // ✅ Updated methods to accept includes parameter
    public virtual async Task<IEnumerable<TDto>> GetAllAsync(string[]? includes = null)
    {
        var query = _context.Set<TEntity>().ApplyIncludes(includes);
        var entities = await query.ToListAsync();
        return entities.Select(entity => entity.Adapt<TDto>());
    }

    // ✅ New paginated GetAllAsync method
    public virtual async Task<IEnumerable<TDto>> GetAllAsync(string[]? includes, int skip, int take)
    {
        var query = _context.Set<TEntity>()
            .ApplyIncludes(includes)
            .Skip(skip)
            .Take(take);
        
        var entities = await query.ToListAsync();
        return entities.Select(entity => entity.Adapt<TDto>());
    }

    // ✅ New GetCountAsync method
    public virtual async Task<int> GetCountAsync()
    {
        return await _context.Set<TEntity>().CountAsync();
    }

    public virtual async Task<TDto?> GetByIdAsync(int id, string[]? includes = null)
    {
        var query = _context.Set<TEntity>().ApplyIncludes(includes);
        var entity = await query.FirstOrDefaultAsync(e => e.Id == id);
        return entity?.Adapt<TDto>();
    }

    public virtual async Task<TDto?> CreateAsync(TCreateDto createDto)
{
    try
    {
        var entity = createDto.Adapt<TEntity>();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Uuid = Guid.NewGuid();

        // ✅ Only link to existing entities (no creation)
        await _relationshipProcessor.ProcessRelationshipsForCreateAsync(entity, createDto);

        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync();

        return entity.Adapt<TDto>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating {typeof(TEntity).Name}: {ex.Message}");
        return null;
    }
}

    public virtual async Task<TDto?> UpdateAsync(TDto updateDto)
    {
        try
        {
            var entity = await _context.Set<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == updateDto.Id);

            if (entity == null) return null;

            // Update basic properties (excluding navigation properties)

            updateDto.Adapt(entity, GetUpdateConfig());
            entity.UpdatedAt = DateTime.UtcNow;

            // ✅ Process relationships
            await _relationshipProcessor.ProcessRelationshipsAsync(entity, updateDto);

            await _context.SaveChangesAsync();
            return entity.Adapt<TDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating {typeof(TEntity).Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = GetEntityById(id);
        if (entity == null) return false;

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<IEnumerable<TDto>> SearchAsync(string search, string[]? includes = null)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return GetAllAsync(includes);
        }
        var query = _context.Set<TEntity>().AsQueryable().ApplyIncludes(includes);
        try
        {
            query = query.ApplySearch(new EntitySearch().ParseSearchTerms(search));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error applying search terms");
            return Task.FromResult(Enumerable.Empty<TDto>());
        }
        return Task.FromResult(query.Adapt<IEnumerable<TDto>>());
    }

    // ✅ New paginated SearchAsync method
    public Task<IEnumerable<TDto>> SearchAsync(string search, string[]? includes, int skip, int take)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return GetAllAsync(includes, skip, take);
        }
        var query = _context.Set<TEntity>().AsQueryable().ApplyIncludes(includes);
        try
        {
            query = query.ApplySearch(new EntitySearch().ParseSearchTerms(search))
                         .Skip(skip)
                         .Take(take);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error applying search terms");
            return Task.FromResult(Enumerable.Empty<TDto>());
        }
        return Task.FromResult(query.Adapt<IEnumerable<TDto>>());
    }

    private TEntity GetEntityById(int id)
    {
        return _context.Set<TEntity>().Find(id) ?? throw new KeyNotFoundException($"Entity with ID {id} not found.");
    }
    
    private TypeAdapterConfig GetUpdateConfig()
{
    var config = new TypeAdapterConfig();
    var setter = config.ForType<TDto, TEntity>();
    
    // Get all navigation properties and ignore them
    var entityType = typeof(TEntity);
    var navigationProperties = entityType.GetProperties()
        .Where(p => p.PropertyType.IsClass && 
                   p.PropertyType != typeof(string) && 
                   !p.PropertyType.IsValueType)
        .ToList();
    
    foreach (var prop in navigationProperties)
    {
        setter.Ignore(prop.Name);
    }
    
    return config;
}
}