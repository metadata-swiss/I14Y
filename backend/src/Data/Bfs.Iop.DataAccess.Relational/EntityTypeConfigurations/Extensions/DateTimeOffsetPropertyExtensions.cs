using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;

internal static class DateTimeOffsetPropertyExtensions
{
    private static TimeZoneInfo? _localTimeZone;
    private const string LocalSystemTimeZoneId = "W. Europe Standard Time";

    public static TimeZoneInfo LocalTimeZone
    {
        get => _localTimeZone ?? TimeZoneInfo.FindSystemTimeZoneById(LocalSystemTimeZoneId);
        set => _localTimeZone = value;
    }

    private static readonly ValueConverter<DateTimeOffset, DateTimeOffset> _dateTimeOffsetUtcStorageConverter = new(
        v => TimeZoneInfo.ConvertTime(v, TimeZoneInfo.Utc),
        v => v.AsLocal()
    );

    private static readonly ValueConverter<DateTimeOffset?, DateTimeOffset?> _nullableDateTimeOffsetUtcStorageConverter = new(
        v => !v.HasValue ? null : TimeZoneInfo.ConvertTime(v.Value, TimeZoneInfo.Utc),
        v => v.HasValue ? v.Value.AsLocal() : null
    );

    /// <summary>
    /// Adds value conversion to maintain local time zone when storing and loading date time offset values to and from the db
    /// </summary>
    /// <returns></returns>
    public static PropertyBuilder<DateTimeOffset> AddLocalDateTimeOffsetConversion(this PropertyBuilder<DateTimeOffset> propertyBuilder) =>
        propertyBuilder.HasConversion(_dateTimeOffsetUtcStorageConverter);

    /// <summary>
    /// Adds value conversion to maintain local time zone when storing and loading date time offset values to and from the db
    /// </summary>
    /// <returns></returns>
    public static PropertyBuilder<DateTimeOffset?> AddLocalDateTimeOffsetConversion(this PropertyBuilder<DateTimeOffset?> propertyBuilder) => 
        propertyBuilder.HasConversion(_nullableDateTimeOffsetUtcStorageConverter);

    private static DateTimeOffset AsLocal(this DateTimeOffset value) => 
        TimeZoneInfo.ConvertTime(value, LocalTimeZone);
}
