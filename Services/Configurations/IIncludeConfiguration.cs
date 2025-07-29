using System;

namespace Sonic.API.Services;

public interface IIncludeConfiguration<TEntity> where TEntity : class
{
    IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query);
}