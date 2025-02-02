﻿#nullable enable
using Quartz;
using Quartz.Impl.Matchers;
using Quartzmin.Helpers;
using Quartzmin.Models;
using Quartz.Plugins.RecentHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Quartzmin.Controllers;

public class JobsController : PageControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var keys = (await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup())).OrderBy(x => x.ToString());
        var list = new List<JobListItem>();
        var knownTypes = new List<string>();

        foreach (var key in keys)
        {
            var detail = await GetJobDetail(key);
            var item = new JobListItem
            {
                Concurrent = !detail.ConcurrentExecutionDisallowed,
                Persist = detail.PersistJobDataAfterExecution,
                Recovery = detail.RequestsRecovery,
                JobName = key.Name,
                Group = key.Group,
                Type = detail.JobType.FullName ?? detail.JobType.Name,
                History = Histogram.Empty,
                Description = detail.Description ?? ""
            };
            knownTypes.Add(detail.JobType.RemoveAssemblyDetails());
            list.Add(item);
        }

        Services.Cache.UpdateJobTypes(knownTypes);

        ViewBag.Groups = (await Scheduler.GetJobGroupNames()).GroupArray();

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> New()
    {
        var job = new JobPropertiesViewModel { IsNew = true };
        var jobDataMap = new JobDataMapModel { Template = JobDataMapItemTemplate };

        job.GroupList = (await Scheduler.GetJobGroupNames()).GroupArray();
        job.Group = SchedulerConstants.DefaultGroup;
        job.TypeList = Services.Cache.JobTypes;

        return View("Edit", new JobViewModel { Job = job, DataMap = jobDataMap });
    }

    [HttpGet]
    public async Task<IActionResult> Trigger(string name, string group)
    {
        if (!EnsureValidKey(name, group)) return BadRequest()!;

        var jobKey = JobKey.Create(name, group);
        var job = await GetJobDetail(jobKey);
        var jobDataMap = new JobDataMapModel { Template = JobDataMapItemTemplate };

        ViewBag.JobName = name;
        ViewBag.Group = group;

        jobDataMap.Items.AddRange(job.GetJobDataMapModel(Services));

        return View(jobDataMap);
    }

    [HttpPost, ActionName("Trigger"), JsonErrorResponse]
    public async Task<IActionResult> PostTrigger(string name, string group)
    {
        if (Request is null) return BadRequest()!;
        if (!EnsureValidKey(name, group)) return BadRequest()!;

        var jobDataMap = (await Request.GetJobDataMapForm()).GetModel(Services);

        var result = new ValidationResult();

        ModelValidator.Validate(jobDataMap, result.Errors);

        if (result.Success)
        {
            await Scheduler.TriggerJob(JobKey.Create(name, group), jobDataMap.GetQuartzJobDataMap());
        }

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string name, string group, bool clone = false)
    {
        if (!EnsureValidKey(name, group)) return BadRequest()!;

        var jobKey = JobKey.Create(name, group);
        var job = await GetJobDetail(jobKey);

        var jobDataMap = new JobDataMapModel { Template = JobDataMapItemTemplate };

        var jobModel = new JobPropertiesViewModel
        {
            IsNew = clone,
            IsCopy = clone,
            JobName = name,
            Group = group,
            GroupList = (await Scheduler.GetJobGroupNames()).GroupArray(),
            Type = job.JobType.RemoveAssemblyDetails(),
            TypeList = Services.Cache.JobTypes,
            Description = job.Description ?? "No Description",
            Recovery = job.RequestsRecovery
        };

        if (clone)
            jobModel.JobName += " - Copy";

        jobDataMap.Items.AddRange(job.GetJobDataMapModel(Services));

        return View("Edit", new JobViewModel { Job = jobModel, DataMap = jobDataMap });
    }

    private async Task<IJobDetail> GetJobDetail(JobKey key)
    {
        var job = await Scheduler.GetJobDetail(key);

        if (job == null)
            throw new InvalidOperationException("Job " + key + " not found.");

        return job;
    } 

    [HttpPost, JsonErrorResponse]
    public async Task<IActionResult> Save([FromForm] JobViewModel model, bool trigger)
    {
        if (Request is null) return BadRequest()!;
        var jobModel = model.Job;
        if (jobModel?.Type is null ||
            jobModel.JobName is null) return BadRequest()!;
        
        var jobDataMap = (await Request.GetJobDataMapForm()).GetModel(Services);

        var result = new ValidationResult();

        model.Validate(result.Errors);
        ModelValidator.Validate(jobDataMap, result.Errors);

        if (!result.Success) return Json(result);
        
        var jobType = Type.GetType(jobModel.Type, true);
        if (jobType is null) return BadRequest()!;

        IJobDetail BuildJob(JobBuilder builder) {
            return builder
                .OfType(jobType)
                .WithIdentity(jobModel.JobName, jobModel.Group!)
                .WithDescription(jobModel.Description)
                .SetJobData(jobDataMap.GetQuartzJobDataMap())
                .RequestRecovery(jobModel.Recovery)
                .Build();
        }

        if (jobModel.IsNew)
        {
            await Scheduler.AddJob(BuildJob(JobBuilder.Create().StoreDurably()), replace: false);
        }
        else if (jobModel.OldJobName is not null)
        {
            var oldJob = await GetJobDetail(JobKey.Create(jobModel.OldJobName, jobModel.OldGroup));
            await Scheduler.UpdateJob(oldJob.Key, BuildJob(oldJob.GetJobBuilder()));
        }
        else
        {
            return BadRequest()!;
        }

        if (trigger)
        {
            await Scheduler.TriggerJob(JobKey.Create(jobModel.JobName, jobModel.Group));
        }

        return Json(result);
    }

    [HttpPost, JsonErrorResponse]
    public async Task<IActionResult> Delete([FromBody] KeyModel model)
    {
        if (!EnsureValidKey(model)) return BadRequest()!;

        var key = model.ToJobKey();
        
        if (key is null)throw new InvalidOperationException("Cannot delete job, key is invalid");
        if (!await Scheduler.DeleteJob(key)) throw new InvalidOperationException("Cannot delete job " + key);

        return NoContent()!;
    }

    [HttpGet, JsonErrorResponse]
    public async Task<IActionResult> AdditionalData()
    {
        var keys = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var history = await Scheduler.Context.GetExecutionHistoryStore().Maybe(s=>s.FilterLastOfEveryJob(10));
        var historyByJob = history?.ToLookup(x => x.Job);

        var list = new List<object>();
        foreach (var key in keys)
        {
            var triggers = await Scheduler.GetTriggersOfJob(key);

            var nextFires = triggers.Select(x => x.GetNextFireTimeUtc()?.UtcDateTime).ToArray();

            list.Add(new
            {
                JobName = key.Name, key.Group,
                History = historyByJob?.TryGet(key.ToString()).ToHistogram(),
                NextFireTime = nextFires.Where(x => x != null).OrderBy(x => x).FirstOrDefault()?.ToDefaultFormat(),
            });
        }

        return View(list);
    }

    [HttpGet]
    public Task<IActionResult> Duplicate(string name, string group)
    {
        return Edit(name, group, clone: true);
    }

    private bool EnsureValidKey(string? name, string? group) => !(string.IsNullOrEmpty(name!) || string.IsNullOrEmpty(group!));
    private bool EnsureValidKey(KeyModel model) => EnsureValidKey(model.Name, model.Group);

}