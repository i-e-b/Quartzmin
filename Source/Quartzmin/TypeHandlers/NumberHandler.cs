﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Quartzmin.TypeHandlers;

[EmbeddedTypeHandlerResources(nameof(NumberHandler), Script = "")]
public class NumberHandler : TypeHandlerBase
{
    private static readonly Dictionary<UnderlyingType, Type> _clrTypes = new()
    {
        [UnderlyingType.Decimal] = typeof(decimal),
        [UnderlyingType.Double] = typeof(double),
        [UnderlyingType.Float] = typeof(float),
        [UnderlyingType.Integer] = typeof(int),
        [UnderlyingType.Long] = typeof(long),
    };

    public UnderlyingType NumberType { get; set; }

    public Type? GetClrType() => _clrTypes[NumberType];

    public NumberHandler() { }

    public NumberHandler(UnderlyingType numberType)
    {
        NumberType = numberType;
        Name = NumberType.ToString()!;
        DisplayName = Name;
    }

    public override bool CanHandle(object? value)
    {
        if (value == null) return false;
        return GetClrType()?.IsInstanceOfType(value) == true;
    }

    public override object? ConvertFrom(object? value)
    {
        var cult = CultureInfo.InvariantCulture;

        if (value is string str)
        {
            str = str.Replace(" ", "").Replace(",", ".");

            if (NumberType == UnderlyingType.Decimal && decimal.TryParse(str, NumberStyles.Any, cult, out var decimalResult))
                return decimalResult;
            if (NumberType == UnderlyingType.Double && double.TryParse(str, NumberStyles.Any, cult, out var dResult))
                return dResult;
            if (NumberType == UnderlyingType.Float && float.TryParse(str, NumberStyles.Any, cult, out var fResult))
                return fResult;
            if (NumberType == UnderlyingType.Integer && int.TryParse(str, NumberStyles.Any, cult, out var iResult))
                return iResult;
            if (NumberType == UnderlyingType.Long && long.TryParse(str, NumberStyles.Any, cult, out var lResult))
                return lResult;
        }

        if (value is decimal || value is double || value is float || value is int || value is long)
        {
            var clrType = GetClrType();
            if (clrType is not null) return Convert.ChangeType(value, clrType, cult);
        }

        return null;
    }

    public enum UnderlyingType
    {
        Decimal,
        Double,
        Float,
        Integer,
        Long
    }
}