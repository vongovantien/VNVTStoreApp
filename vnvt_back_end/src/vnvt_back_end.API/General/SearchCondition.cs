namespace vnvt_back_end.Core.General
{
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
}