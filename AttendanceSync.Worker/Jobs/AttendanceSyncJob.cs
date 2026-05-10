using AttendanceSync.Infrastructure.Services;
using Hangfire;

namespace AttendanceSync.Worker.Jobs;

public class AttendanceSyncJob
{
    private readonly IIntegrationLogService _integrationLogService;
    private readonly IAttendanceSyncService _attendanceSyncService;

    public AttendanceSyncJob(
        IIntegrationLogService integrationLogService,
        IAttendanceSyncService attendanceSyncService)
    {
        _integrationLogService = integrationLogService;
        _attendanceSyncService = attendanceSyncService;
    }

    /// <summary>
    /// Main Hangfire recurring job.
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    public async Task RunAsync()
    {
        // Create job execution log
        var jobLog = await _integrationLogService
            .StartJobAsync("attendance-sync-job");

        // Run attendance synchronization
        await _attendanceSyncService.RunAsync(jobLog.Id);
    }
}
