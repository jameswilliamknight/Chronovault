namespace Chronovault.Service.Framework;

/// <summary>
///     Contract for jobs that run periodically on a fixed schedule.
/// </summary>
public interface IPeriodicJob
{
    /// <summary>Interval between job executions.</summary>
    TimeSpan Period { get; }

    /// <summary>Unique identifier for this job type.</summary>
    JobName JobName { get; }

    /// <summary>Emoji icon for log output.</summary>
    string Icon { get; }

    /// <summary>Executes the job logic.</summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}

