#nullable enable
using System;

namespace Quartzmin.Models;

public class CalendarListItem
{
    public CalendarListItem(string name, string description, Type type)
    {
        Name = name;
        Description = description;
        Type = type;
    }
    
    public string Name { get; set; }

    public string Description { get; set; }

    public Type Type { get; set; }
}