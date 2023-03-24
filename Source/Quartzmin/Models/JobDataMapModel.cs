using System.Collections.Generic;

namespace Quartzmin.Models;

public class JobDataMapModel
{
    public List<JobDataMapItem> Items { get; } = new();
    public JobDataMapItem Template { get; set; }
}