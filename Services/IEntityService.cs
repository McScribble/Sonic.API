using Sonic.Models;

namespace Sonic.API.Services;

public interface IEntityService<TDto, TCreateDto, TEntity>
    where TDto : GenericEntity
    where TCreateDto : GenericCreateEntityDto
    where TEntity : GenericEntity
{
    Task<IEnumerable<TDto>> GetAllAsync(string[]? includes = null);
    Task<IEnumerable<TDto>> GetAllAsync(string[]? includes, int skip, int take);
    Task<int> GetCountAsync();
    Task<TDto?> GetByIdAsync(int entityId, string[]? includes = null);
    Task<IEnumerable<TDto>> SearchAsync(string searchTerm, string[]? includes = null);
    Task<IEnumerable<TDto>> SearchAsync(string searchTerm, string[]? includes, int skip, int take);
    Task<TDto?> CreateAsync(TCreateDto createDto);
    Task<TDto?> UpdateAsync(TDto updateDto);
    Task<bool> DeleteAsync(int entityId);
}
