using Quartz;

namespace Quartzmin.Models;

public class KeyModel
{
    public string Name { get; set; }

    public string Group { get; set; }

    public JobKey ToJobKey() => new(Name, Group);

    public TriggerKey ToTriggerKey() => new(Name, Group);
}