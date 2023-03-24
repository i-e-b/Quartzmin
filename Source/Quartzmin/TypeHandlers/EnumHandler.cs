#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Quartzmin.TypeHandlers;

public class EnumHandler : OptionSetHandler
{
    public Type EnumType { get; set; }

    public EnumHandler() {
        EnumType = typeof(object);
    }
    public EnumHandler(Type enumType)
    {
        EnumType = enumType;
        Name = EnumType.FullName ?? EnumType.Name;
        DisplayName = EnumType.Name;
    }

    public override bool CanHandle(object? value)
    {
        return value != null && EnumType.IsInstanceOfType(value);
    }

    public override object? ConvertFrom(object? value)
    {
        if (value == null) return null;

        if (EnumType.IsInstanceOfType(value)) return value;

        if (value is not string str) return null;
        try
        {
            return Enum.Parse(EnumType, str, true);
        }
        catch
        {
            return null;
        }
    }

    private string GetDisplayName(string enumValue)
    {
        return EnumType
            .GetMember(enumValue).First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? enumValue;
    }

    public override KeyValuePair<string, string>[] GetItems()
    {
        return Enum.GetNames(EnumType)
            .Select(x => new KeyValuePair<string, string>(x, GetDisplayName(x)))
            .ToArray();
    }
}