using System.Diagnostics;

namespace Chronovault.Service.Framework;

/// <summary>
///     Generic hosted service that runs any <see cref="IPeriodicJob"/> on a fixed schedule.
///     Simplified from AraWhanui's version - no claims/correlation/pipeline dependencies.
/// </summary>
public class PeriodicHostedService<TJob>(IServiceScopeFactory scopeFactory) : BackgroundService
    where TJob : IPeriodicJob
{
    private static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(60);
    private readonly Stopwatch _stopwatch = new();
    private string _icon = "⚙️";
    private string _jobName = typeof(TJob).Name;
    private TimeSpan _period = DefaultPeriod;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Resolve job to get configured period/name/icon
        await using (var initScope = scopeFactory.CreateAsyncScope())
        {
            var job = initScope.ServiceProvider.GetRequiredService<TJob>();
            _period = job.Period;
            _jobName = job.JobName.Value;
            _icon = job.Icon;
        }

        Log.Information(
            "PeriodicHostedServiceExecuteAsync {Icon} {JobName} starting, interval: {PeriodMinutes:F1} minutes",
            _icon, _jobName, _period.TotalMinutes);

        using var timer = new PeriodicTimer(_period);

        try
        {
            // Run immediately on startup, then wait for each tick
            do
            {
                await ScopeAndExecuteJobAsync(stoppingToken);
            } while (!stoppingToken.IsCancellationRequested &&
                     await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown - not an error
        }
        finally
        {
            Log.Information("PeriodicHostedServiceExecuteAsync {Icon} {JobName} shutting down", _icon, _jobName);
        }
    }

    private async Task ScopeAndExecuteJobAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<TJob>();

        Log.Debug("PeriodicHostedServiceScopeAndExecuteJobAsync {Icon} {JobName} starting", _icon, _jobName);

        _stopwatch.Restart();
        try
        {
            await job.ExecuteAsync(stoppingToken);

            _stopwatch.Stop();
            Log.Information(
                "PeriodicHostedServiceScopeAndExecuteJobAsync {Icon} {JobName} completed in {ElapsedMs}ms",
                _icon, _jobName, _stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw; // Let the outer handler deal with shutdown
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            Log.Error(ex,
                "PeriodicHostedServiceScopeAndExecuteJobAsync {Icon} {JobName} failed after {ElapsedMs}ms",
                _icon, _jobName, _stopwatch.ElapsedMilliseconds);
        }
    }
}

