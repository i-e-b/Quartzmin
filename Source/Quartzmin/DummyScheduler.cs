#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace Quartzmin;

internal class DummyScheduler : IScheduler
{
    private static Exception DummyException() => throw new InvalidOperationException("Scheduler was not correctly initialised before invoking");
    public Task<bool> IsJobGroupPaused(string groupName, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> IsTriggerGroupPaused(string groupName, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<SchedulerMetaData> GetMetaData(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<IJobExecutionContext>> GetCurrentlyExecutingJobs(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<string>> GetJobGroupNames(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<string>> GetTriggerGroupNames(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<string>> GetPausedTriggerGroups(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task Start(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task StartDelayed(TimeSpan delay, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task Standby(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task Shutdown(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task Shutdown(bool waitForJobsToComplete, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<DateTimeOffset> ScheduleJob(IJobDetail jobDetail, ITrigger trigger, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<DateTimeOffset> ScheduleJob(ITrigger trigger, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ScheduleJobs(IReadOnlyDictionary<IJobDetail, IReadOnlyCollection<ITrigger>> triggersAndJobs, bool replace, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ScheduleJob(IJobDetail jobDetail, IReadOnlyCollection<ITrigger> triggersForJob, bool replace, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> UnscheduleJob(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> UnscheduleJobs(IReadOnlyCollection<TriggerKey> triggerKeys, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<DateTimeOffset?> RescheduleJob(TriggerKey triggerKey, ITrigger newTrigger, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task AddJob(IJobDetail jobDetail, bool replace, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task AddJob(IJobDetail jobDetail, bool replace, bool storeNonDurableWhileAwaitingScheduling, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> DeleteJob(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> DeleteJobs(IReadOnlyCollection<JobKey> jobKeys, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task TriggerJob(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task TriggerJob(JobKey jobKey, JobDataMap data, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task PauseJob(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task PauseJobs(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task PauseTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task PauseTriggers(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResumeJob(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResumeJobs(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResumeTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResumeTriggers(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task PauseAll(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResumeAll(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<JobKey>> GetJobKeys(GroupMatcher<JobKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<ITrigger>> GetTriggersOfJob(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<TriggerKey>> GetTriggerKeys(GroupMatcher<TriggerKey> matcher, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IJobDetail?> GetJobDetail(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<ITrigger?> GetTrigger(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<TriggerState> GetTriggerState(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task ResetTriggerFromErrorState(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task AddCalendar(string calName, ICalendar calendar, bool replace, bool updateTriggers, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> DeleteCalendar(string calName, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<ICalendar?> GetCalendar(string calName, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<IReadOnlyCollection<string>> GetCalendarNames(CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> Interrupt(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> Interrupt(string fireInstanceId, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> CheckExists(JobKey jobKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task<bool> CheckExists(TriggerKey triggerKey, CancellationToken cancellationToken = new()) => throw DummyException();
    public Task Clear(CancellationToken cancellationToken = new()) => throw DummyException();
    public string SchedulerName  => throw DummyException();
    public string SchedulerInstanceId => throw DummyException();
    public SchedulerContext Context => throw DummyException();
    public bool InStandbyMode => throw DummyException();
    public bool IsShutdown => throw DummyException();
    public IJobFactory JobFactory { set=>throw DummyException(); }
    public IListenerManager ListenerManager => throw DummyException();
    public bool IsStarted => throw DummyException();
}