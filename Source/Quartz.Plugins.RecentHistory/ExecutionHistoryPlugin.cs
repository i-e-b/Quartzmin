using Quartz.Impl.Matchers;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Quartz.Plugins.RecentHistory;

[UsedImplicitly]
public class ExecutionHistoryPlugin : ISchedulerPlugin, IJobListener
{
    private IScheduler? _scheduler;
    private IExecutionHistoryStore? _store;

    public string Name { get; set; } = "";
    public Type? StoreType { get; set; }

    public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        Name = pluginName;
        _scheduler = scheduler;
        _scheduler.ListenerManager.AddJobListener(this, EverythingMatcher<JobKey>.AllJobs());
            
        return Task.FromResult(0);
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        if (_scheduler is null) throw new Exception($"Scheduler not supplied to {nameof(ExecutionHistoryPlugin)} before calling {nameof(Start)}");
        _store = _scheduler.Context.GetExecutionHistoryStore();

        if (_store is null)
        {
            if (StoreType is not null) _store = (IExecutionHistoryStore)Activator.CreateInstance(StoreType)!;

            if (_store is null) throw new Exception(nameof(StoreType) + " is not set.");

            _scheduler.Context.SetExecutionHistoryStore(_store);
        }

        _store.SchedulerName = _scheduler.SchedulerName;

        await _store.Purge();
    }

    public Task Shutdown(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (_store is null) throw new Exception($"Store was not set in {nameof(ExecutionHistoryPlugin)} before calling {nameof(JobToBeExecuted)}. Make sure to call {nameof(Start)} first");
        var entry = new ExecutionHistoryEntry
        {
            FireInstanceId = context.FireInstanceId,
            SchedulerInstanceId = context.Scheduler.SchedulerInstanceId,
            SchedulerName = context.Scheduler.SchedulerName,
            ActualFireTimeUtc = context.FireTimeUtc.UtcDateTime,
            ScheduledFireTimeUtc = context.ScheduledFireTimeUtc?.UtcDateTime,
            Recovering = context.Recovering,
            Job = context.JobDetail.Key.ToString(),
            Trigger = context.Trigger.Key.ToString(),
        };
        await _store.Save(entry);
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        if (_store is null) throw new Exception($"Store was not set in {nameof(ExecutionHistoryPlugin)} before calling {nameof(JobToBeExecuted)}. Make sure to call {nameof(Start)} first");
            
        var entry = await _store.Get(context.FireInstanceId);
        if (entry is not null)
        {
            entry.FinishedTimeUtc = DateTime.UtcNow;
            entry.ExceptionMessage = jobException?.GetBaseException().Message ?? "";
            await _store.Save(entry);
        }
        if (jobException == null)
            await _store.IncrementTotalJobsExecuted();
        else
            await _store.IncrementTotalJobsFailed();
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (_store is null) throw new Exception($"Store was not set in {nameof(ExecutionHistoryPlugin)} before calling {nameof(JobToBeExecuted)}. Make sure to call {nameof(Start)} first");

        var entry = await _store.Get(context.FireInstanceId);
        if (entry != null)
        {
            entry.Vetoed = true;
            await _store.Save(entry);
        }
    }
}