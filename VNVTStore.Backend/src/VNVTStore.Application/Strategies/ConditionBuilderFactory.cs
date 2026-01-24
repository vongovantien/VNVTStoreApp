using System.Collections.Concurrent;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Strategies;

/// <summary>
/// Factory for creating condition strategies (Factory Pattern + Singleton for strategies).
/// Open for extension (add new strategies) but closed for modification (OCP).
/// </summary>
public static class ConditionBuilderFactory
{
    // Flyweight Pattern: Cache strategy instances (they are stateless)
    private static readonly ConcurrentDictionary<SearchCondition, IConditionStrategy> _strategies = new();
    
    static ConditionBuilderFactory()
    {
        // Pre-register all strategies
        RegisterStrategy(SearchCondition.IsNull, new NullCheckStrategy(isNull: true));
        RegisterStrategy(SearchCondition.IsNotNull, new NullCheckStrategy(isNull: false));
        RegisterStrategy(SearchCondition.Contains, new ContainsStrategy());
        RegisterStrategy(SearchCondition.Equal, new EqualStrategy(caseSensitive: false));
        RegisterStrategy(SearchCondition.EqualExact, new EqualStrategy(caseSensitive: true));
        RegisterStrategy(SearchCondition.NotEqual, new NotEqualStrategy());
        RegisterStrategy(SearchCondition.In, new InStrategy(notIn: false));
        RegisterStrategy(SearchCondition.NotIn, new InStrategy(notIn: true));
        RegisterStrategy(SearchCondition.GreaterThan, new ComparisonStrategy(">"));
        RegisterStrategy(SearchCondition.GreaterThanEqual, new ComparisonStrategy(">="));
        RegisterStrategy(SearchCondition.LessThan, new ComparisonStrategy("<"));
        RegisterStrategy(SearchCondition.LessThanEqual, new ComparisonStrategy("<="));
        RegisterStrategy(SearchCondition.DateTimeRange, new DateRangeStrategy());
        RegisterStrategy(SearchCondition.DayPart, new DatePartStrategy("day"));
        RegisterStrategy(SearchCondition.MonthPart, new DatePartStrategy("month"));
        RegisterStrategy(SearchCondition.DatePart, new DatePartStrategy("date"));
    }
    
    /// <summary>
    /// Gets the strategy for a given search condition.
    /// </summary>
    public static IConditionStrategy GetStrategy(SearchCondition condition)
    {
        if (_strategies.TryGetValue(condition, out var strategy))
            return strategy;
        
        throw new NotSupportedException($"No strategy registered for condition: {condition}");
    }
    
    /// <summary>
    /// Registers a custom strategy (for extensibility).
    /// </summary>
    public static void RegisterStrategy(SearchCondition condition, IConditionStrategy strategy)
    {
        _strategies[condition] = strategy;
    }
    
    /// <summary>
    /// Checks if a strategy exists for the condition.
    /// </summary>
    public static bool HasStrategy(SearchCondition condition)
    {
        return _strategies.ContainsKey(condition);
    }
}
