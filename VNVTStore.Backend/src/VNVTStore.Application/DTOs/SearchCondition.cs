namespace VNVTStore.Application.DTOs;

public enum SearchCondition
{
    Equal,
    NotEqual,
    Contains,
    GreaterThan,
    GreaterThanEqual,
    LessThan,
    LessThanEqual,
    DateTimeRange,
    DayPart,
    MonthPart,
    DatePart,
    IsNull,
    IsNotNull,
    In,
    NotIn,
    EqualExact
}
