#nullable enable
using System;
using Quartz;

namespace Quartzmin.Models;

public class KeyModel
{
    public string? Name { get; set; }

    public string? Group { get; set; }

    public JobKey ToJobKey()
    {
        if (Name is null) throw new Exception($"Invalid '{nameof(Name)}' when converting {nameof(KeyModel)} to {nameof(JobKey)}");
        return new(Name, Group);
    }

    public TriggerKey ToTriggerKey()
    {
        if (Name is null) throw new Exception($"Invalid '{nameof(Name)}' when converting {nameof(KeyModel)} to {nameof(TriggerKey)}");
        if (Group is null) throw new Exception($"Invalid '{nameof(Group)}' when converting {nameof(KeyModel)} to {nameof(TriggerKey)}");
        return new(Name, Group);
    }
}