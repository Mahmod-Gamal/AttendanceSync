using AttendanceSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Services
{
    public interface IIntegrationLogService
    {
        Task<IntegrationJobLog> StartJobAsync(string jobName);

        Task MarkSuccessAsync(
            long jobLogId,
            int totalFetched,
            int totalSent,
            string? message = null);

        Task MarkFailedAsync(
            long jobLogId,
            string errorMessage,
            int totalFetched = 0,
            int totalSent = 0,
            int totalFailed = 0);

        Task MarkPartialSuccessAsync(
            long jobLogId,
            int totalFetched,
            int totalSent,
            int totalFailed,
            string? message = null);

        Task LogErrorAsync(
            long? jobLogId,
            string source,
            string errorMessage,
            Exception? exception = null,
            string? requestPayload = null,
            string? responsePayload = null);
    }
}

