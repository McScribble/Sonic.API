using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Sonic.Models;

public static class SearchExtensions
{
    public static IQueryable<TEntity> ApplySearch<TEntity>(this IQueryable<TEntity> query, EntitySearch search)
        where TEntity : class
    {
        if (search == null || search.SearchTerms == null || !search.SearchTerms.Any())
        {
            return query;
        }

        // Validate entity has search field
        var entityType = typeof(TEntity);
        foreach (var term in search.SearchTerms)
        {
            if (!entityType.GetProperties().Any(p => p.Name.Equals(term.FieldName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Field '{term.FieldName}' does not exist on entity type '{entityType.Name}'.");
            }
        }

        foreach (var term in search.SearchTerms)
        {
            switch (term.Operator)
            {
                case SearchOperators.Contains:
                    query = query.Where(e => EF.Property<string>(e, term.FieldName).Contains(term.SearchTerm));
                    break;
                case SearchOperators.Equals:
                    query = query.Where(e => EF.Property<string>(e, term.FieldName) == term.SearchTerm);
                    break;
                // Add other cases as needed
                case SearchOperators.StartsWith:
                    query = query.Where(e => EF.Property<string>(e, term.FieldName).StartsWith(term.SearchTerm));
                    break;
                case SearchOperators.EndsWith:
                    query = query.Where(e => EF.Property<string>(e, term.FieldName).EndsWith(term.SearchTerm));
                    break;
                case SearchOperators.Like:
                    // ✅ First get the data, then apply Levenshtein using reflection
                    var candidates = query.ToList(); // Execute the query first
                    var filteredResults = candidates.Where(e =>
                    {
                        var propertyValue = GetPropertyValue(e, term.FieldName);
                        return propertyValue?.LevenshteinDistance(term.SearchTerm, 4) == true;
                    });
                    query = filteredResults.AsQueryable();
                    break;
            }
        }

        return query;
    }

    public static bool LevenshteinDistance(this string source, string target, int maxDistance = 4)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return false;
        }

        int distance = DamerauLevenshteinDistance(source, target, maxDistance);
        return distance < maxDistance;
    }

    /// <summary>
    /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
    /// integers, where each integer represents the code point of a character in the source string.
    /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
    /// </summary>
    /// <param name="source">An array of the code points of the first string</param>
    /// <param name="target">An array of the code points of the second string</param>
    /// <param name="threshold">Maximum allowable distance</param>
    /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Levenshtein distance between the strings</returns>
    public static int DamerauLevenshteinDistance(string source, string target, int threshold)
    {

        int length1 = source.Length;
        int length2 = target.Length;

        // Return trivial case - difference in string lengths exceeds threshhold
        if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

        // Ensure arrays [i] / length1 use shorter length 
        if (length1 > length2)
        {
            Swap(ref target, ref source);
            Swap(ref length1, ref length2);
        }

        static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }

        int maxi = length1;
        int maxj = length2;

        int[] dCurrent = new int[maxi + 1];
        int[] dMinus1 = new int[maxi + 1];
        int[] dMinus2 = new int[maxi + 1];
        int[] dSwap;

        for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

        int jm1 = 0, im1 = 0, im2 = -1;

        for (int j = 1; j <= maxj; j++)
        {

            // Rotate
            dSwap = dMinus2;
            dMinus2 = dMinus1;
            dMinus1 = dCurrent;
            dCurrent = dSwap;

            // Initialize
            int minDistance = int.MaxValue;
            dCurrent[0] = j;
            im1 = 0;
            im2 = -1;

            for (int i = 1; i <= maxi; i++)
            {

                int cost = source[im1] == target[jm1] ? 0 : 1;

                int del = dCurrent[im1] + 1;
                int ins = dMinus1[i] + 1;
                int sub = dMinus1[im1] + cost;

                //Fastest execution for min value of 3 integers
                int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                    min = Math.Min(min, dMinus2[im2] + cost);

                dCurrent[i] = min;
                if (min < minDistance) { minDistance = min; }
                im1++;
                im2++;
            }
            jm1++;
            if (minDistance > threshold) { return int.MaxValue; }
        }

        int result = dCurrent[maxi];
        return (result > threshold) ? int.MaxValue : result;
    }

    private static string? GetPropertyValue<T>(T entity, string propertyName)
    {
        if (entity == null) return null;
        
        var property = typeof(T).GetProperty(propertyName, 
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        
        return property?.GetValue(entity)?.ToString();
    }
}

public class EntitySearch
{
    public List<EntitySearchTerm> SearchTerms { get; set; } = new List<EntitySearchTerm>();

    public EntitySearch ParseSearchTerms(string value)
    {
        SearchTerms = value.Split(':')
            .Select(part =>
            {
                var parts = part.Split(SearchOperators.AllOperators.ToArray(), 2);
                return new EntitySearchTerm
                {
                    FieldName = parts[0],
                    SearchTerm = parts.Length > 1 ? parts[1] : string.Empty,
                    Operator = part.Length > parts[0].Length ? part[parts[0].Length] : SearchOperators.Contains // Default operator
                };
            }).ToList();
        return this;
    }

    public string SerializedSearchTerms
    {
        get => string.Join(":", SearchTerms.Select(term => $"{term.FieldName}{term.Operator}{term.SearchTerm}"));
    }

    public static bool IsValidSearchTerm(string term)
    {
        var searchParts = term.Split(":");
        return searchParts.All(part =>
            !string.IsNullOrWhiteSpace(part)
            && part.Split(SearchOperators.AllOperators.ToArray()).Length == 2);
    }
}

public class EntitySearchTerm
{
    public string SearchTerm { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public char Operator { get; set; } = SearchOperators.Contains; // Default operator
}

public static class SearchOperators
{
    public new const char Equals = '=';
    public const char NotEquals = '!';
    public const char Contains = '^';
    public const char StartsWith = '>';
    public const char EndsWith = '<';
    public const char Like = '~';

    public static List<char> AllOperators => new List<char>
    {
        Equals,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
        Like
    };
}
