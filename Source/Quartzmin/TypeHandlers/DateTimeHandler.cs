#nullable enable
using System;
using System.Globalization;

namespace Quartzmin.TypeHandlers;

[EmbeddedTypeHandlerResources(nameof(DateTimeHandler))]
public class DateTimeHandler : TypeHandlerBase
{
    public bool IgnoreTimeComponent { get; set; }

    public bool IsUtc { get; set; }

    private string? _dateFormat;
    public string DateFormat
    {
        get => _dateFormat ?? DateTimeSettings.DefaultDateFormat;
        set => _dateFormat = value;
    }

    private string? _timeFormat;
    public string TimeFormat
    {
        get => _timeFormat ?? DateTimeSettings.DefaultTimeFormat;
        set => _timeFormat = value;
    }

    public string GetExpectedFormat() => IgnoreTimeComponent ? DateFormat : $"{DateFormat} {TimeFormat}";

    public override bool CanHandle(object? value)
    {
        if (value is not DateTime dt) return false;
        var missingTime = dt.TimeOfDay.Ticks == 0;

        if (IgnoreTimeComponent == missingTime || IsUtc)
            return (IsUtc == (dt.Kind == DateTimeKind.Utc));

        return false;
    }

    public override object? ConvertFrom(object? value)
    {
        if (value is DateTime dt)
            return Normalize(dt);

        if (value is string str && DateTime.TryParseExact(str, new[] { $"{DateFormat} {TimeFormat}", DateFormat }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return Normalize(result);

        return null;
    }

    private DateTime Normalize(DateTime dt)
    {
        return IgnoreTimeComponent
            ? DateTime.SpecifyKind(dt.Date, DateTimeKind.Unspecified)
            : DateTime.SpecifyKind(dt, IsUtc ? DateTimeKind.Utc : DateTimeKind.Local);
    }

    public override string? ConvertToString(object? value)
    {
        if (value is null) return null;
        return string.Format(CultureInfo.InvariantCulture, $"{{0:{GetExpectedFormat()}}}", value);
    }
}