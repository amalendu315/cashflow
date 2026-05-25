namespace Cashflow.Web.Utilities;

public static class AppClock
{
    private const int IndiaOffsetMinutes = 330;

    public static DateTime NowIst()
    {
        DateTime istNow = DateTime.UtcNow.AddMinutes(IndiaOffsetMinutes);

        return DateTime.SpecifyKind(istNow, DateTimeKind.Unspecified);
    }

    public static DateOnly TodayIst()
    {
        return DateOnly.FromDateTime(NowIst());
    }

    public static DateTime ConvertUtcToIst(DateTime utcDateTime)
    {
        DateTime normalizedUtc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        DateTime istDateTime = normalizedUtc.AddMinutes(IndiaOffsetMinutes);

        return DateTime.SpecifyKind(istDateTime, DateTimeKind.Unspecified);
    }
}