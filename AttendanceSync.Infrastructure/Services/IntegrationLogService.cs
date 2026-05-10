using AttendanceSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Services
{

    public class IntegrationLogService : IIntegrationLogService
    {
        private readonly AppDbContext _db;

        public IntegrationLogService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IntegrationJobLog> StartJobAsync(string jobName)
        {
            var log = new IntegrationJobLog
            {
                JobName = jobName,
                StartedAt = DateTime.UtcNow,
                Status = "Started",
                CreatedAt = DateTime.UtcNow
            };

            _db.IntegrationJobLogs.Add(log);
            await _db.SaveChangesAsync();

            return log;
        }

        public async Task MarkSuccessAsync(long jobLogId, int totalFetched, int totalSent, string? message = null)
        {
            var log = await _db.IntegrationJobLogs.FindAsync(jobLogId);
            if (log == null) return;

            log.FinishedAt = DateTime.UtcNow;
            log.Status = "Success";
            log.TotalFetchedRecords = totalFetched;
            log.TotalSentRecords = totalSent;
            log.TotalFailedRecords = 0;
            log.Message = message;

            await _db.SaveChangesAsync();
        }

        public async Task MarkFailedAsync(
            long jobLogId,
            string errorMessage,
            int totalFetched = 0,
            int totalSent = 0,
            int totalFailed = 0)
        {
            var log = await _db.IntegrationJobLogs.FindAsync(jobLogId);
            if (log == null) return;

            log.FinishedAt = DateTime.UtcNow;
            log.Status = "Failed";
            log.TotalFetchedRecords = totalFetched;
            log.TotalSentRecords = totalSent;
            log.TotalFailedRecords = totalFailed;
            log.ErrorMessage = errorMessage;

            await _db.SaveChangesAsync();
        }

        public async Task MarkPartialSuccessAsync(
            long jobLogId,
            int totalFetched,
            int totalSent,
            int totalFailed,
            string? message = null)
        {
            var log = await _db.IntegrationJobLogs.FindAsync(jobLogId);
            if (log == null) return;

            log.FinishedAt = DateTime.UtcNow;
            log.Status = "PartialSuccess";
            log.TotalFetchedRecords = totalFetched;
            log.TotalSentRecords = totalSent;
            log.TotalFailedRecords = totalFailed;
            log.Message = message;

            await _db.SaveChangesAsync();
        }

        public async Task LogErrorAsync(
            long? jobLogId,
            string source,
            string errorMessage,
            Exception? exception = null,
            string? requestPayload = null,
            string? responsePayload = null)
        {
            var error = new IntegrationErrorLog
            {
                JobLogId = jobLogId,
                Source = source,
                ErrorMessage = errorMessage,
                ExceptionDetails = exception?.ToString(),
                RequestPayload = requestPayload,
                ResponsePayload = responsePayload,
                CreatedAt = DateTime.UtcNow
            };

            _db.IntegrationErrorLogs.Add(error);
            await _db.SaveChangesAsync();
        }
    }

}

