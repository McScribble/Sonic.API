using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Sonic.API.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, string[]? includes) 
        where T : class
    {
        if (includes == null || !includes.Any())
            return query;

        foreach (var include in includes)
        {
            if (string.IsNullOrWhiteSpace(include))
                continue;

            try
            {
                // Handle nested properties (e.g., "RequiredInstruments.Category")
                query = ApplyIncludePath(query, include.Trim());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to include '{include}': {ex.Message}");
                // Continue processing other includes
            }
        }

        return query;
    }

    private static IQueryable<T> ApplyIncludePath<T>(IQueryable<T> query, string includePath) 
        where T : class
    {
        var entityType = typeof(T);
        var parts = includePath.Split('.');
        
        // Validate that the include path exists
        if (!IsValidIncludePath(entityType, parts))
        {
            Console.WriteLine($"Invalid include path '{includePath}' for entity {entityType.Name}");
            return query;
        }

        // Build the include expression
        var parameter = Expression.Parameter(entityType, "e");
        Expression propertyExpression = parameter;

        foreach (var part in parts)
        {
            var propertyInfo = propertyExpression.Type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                Console.WriteLine($"Property '{part}' not found in type {propertyExpression.Type.Name}");
                return query;
            }
            propertyExpression = Expression.Property(propertyExpression, propertyInfo);
        }

        var lambda = Expression.Lambda(propertyExpression, parameter);

        // Apply the include using reflection
        var includeMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "Include" && 
                       m.GetParameters().Length == 2 && 
                       m.GetParameters()[1].ParameterType.IsGenericType);

        var genericInclude = includeMethod.MakeGenericMethod(entityType, propertyExpression.Type);
        return (IQueryable<T>)genericInclude.Invoke(null, new object[] { query, lambda })!;
    }

    private static bool IsValidIncludePath(Type entityType, string[] parts)
    {
        var currentType = entityType;
        
        foreach (var part in parts)
        {
            var property = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                return false;

            currentType = property.PropertyType;
            
            // Handle collections
            if (currentType.IsGenericType && 
                typeof(IEnumerable<>).IsAssignableFrom(currentType.GetGenericTypeDefinition()))
            {
                currentType = currentType.GetGenericArguments()[0];
            }
        }

        return true;
    }
}
