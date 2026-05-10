using AttendanceSync.Domain.Dtos;
using AttendanceSync.Domain.Entities;
using AttendanceSync.Infrastructure.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AttendanceSync.Infrastructure.Services
{
    public class AttendanceSyncService : IAttendanceSyncService
    {
        private readonly IHikvisionClient _hikvisionClient;
        private readonly IKayanClient _kayanClient;
        private readonly AppDbContext _db;
        private readonly IIntegrationLogService _logService;
        private readonly IConfiguration _config;

        public AttendanceSyncService(
            IHikvisionClient hikvisionClient,
            IKayanClient kayanClient,
            AppDbContext db,
            IIntegrationLogService logService,
            IConfiguration config)
        {
            _hikvisionClient = hikvisionClient;
            _kayanClient = kayanClient;
            _db = db;
            _logService = logService;
            _config = config;
        }

        public async Task RunAsync(long jobLogId)
        {
            int totalFetched = 0;
            int totalSent = 0;
            int totalFailed = 0;

            try
            {
                var token = await _hikvisionClient.GetValidTokenAsync();

                var pageSize = _config.GetValue<int>("SyncSettings:PageSize", 100);
                var initialLookBackDays = _config.GetValue<int>("SyncSettings:InitialLookBackDays", 30);
                var overlapMinutes = _config.GetValue<int>("SyncSettings:OverlapMinutes", 5);

                var endTime = DateTime.Now;
                var lastSyncedAttendanceTime = await _db.SyncedAttendanceLogs
                    .Where(x => x.IsSynced)
                    .MaxAsync(x => (DateTime?)x.AttendanceTime);
                var beginTime = lastSyncedAttendanceTime?.AddMinutes(-overlapMinutes)
                    ?? endTime.AddDays(-initialLookBackDays);

                var pageIndex = 1;
                var hasMore = true;

                while (hasMore)
                {
                    var hikResponse = await _hikvisionClient.GetAttendanceAsync(
                        token,
                        beginTime,
                        endTime,
                        pageIndex,
                        pageSize);

                    if (hikResponse?.ErrorCode != "0" || hikResponse.Data == null)
                    {
                        await _logService.LogErrorAsync(
                            jobLogId,
                            "HikvisionAttendanceFetch",
                            "Hikvision returned invalid response.",
                            requestPayload: JsonSerializer.Serialize(new { beginTime, endTime, pageIndex, pageSize }),
                            responsePayload: JsonSerializer.Serialize(hikResponse));

                        break;
                    }

                    var records = hikResponse.Data.RecordList;
                    totalFetched += records.Count;

                    if (!records.Any())
                        break;

                    var recordIds = records
                        .Where(x => !string.IsNullOrWhiteSpace(x.RecordGuid))
                        .Select(x => x.RecordGuid)
                        .ToList();

                    var alreadySyncedIds = await _db.SyncedAttendanceLogs
                        .Where(x => recordIds.Contains(x.HikvisionRecordId))
                        .Select(x => x.HikvisionRecordId)
                        .ToListAsync();
                    var alreadySyncedIdSet = alreadySyncedIds.ToHashSet();

                    var newRecords = records
                        .Where(x => !string.IsNullOrWhiteSpace(x.RecordGuid))
                        .Where(x => !alreadySyncedIdSet.Contains(x.RecordGuid))
                        .GroupBy(x => x.RecordGuid)
                        .Select(x => x.OrderByDescending(r => r.DeviceTime).First())
                        .ToList();

                    var invalidRecords = newRecords
                        .Where(HasMissingRequiredKayanData)
                        .ToList();

                    if (invalidRecords.Any())
                    {
                        totalFailed += invalidRecords.Count;

                        await _logService.LogErrorAsync(
                            jobLogId,
                            "KayanValidation",
                            "Skipped Hikvision records missing required employee code.",
                            requestPayload: JsonSerializer.Serialize(invalidRecords.Select(x => new
                            {
                                x.RecordGuid,
                                x.DeviceTime,
                                x.DeviceName,
                                PersonCode = x.PersonInfo?.BaseInfo?.PersonCode
                            })));
                    }

                    var recordsToSend = newRecords
                        .Where(x => !HasMissingRequiredKayanData(x))
                        .ToList();

                    if (recordsToSend.Any())
                    {
                        var kayanRequest = recordsToSend.Select(MapToKayanRequest).ToList();
                        string? kayanResponse = null;

                        try
                        {
                            kayanResponse = await _kayanClient.SendBulkAttendanceAsync(kayanRequest);
                            EnsureKayanResponseSuccessful(kayanResponse);

                            foreach (var record in recordsToSend)
                            {
                                var mapped = MapToKayanRequest(record);

                                _db.SyncedAttendanceLogs.Add(new SyncedAttendanceLog
                                {
                                    HikvisionRecordId = record.RecordGuid,
                                    EmployeeCode = record.PersonInfo!.BaseInfo!.PersonCode,
                                    AttendanceTime = record.DeviceTime,
                                    Direction = record.Direction.ToString(),
                                    KayanRequestPayload = JsonSerializer.Serialize(mapped),
                                    KayanResponsePayload = kayanResponse,
                                    IsSynced = true,
                                    SyncedAt = DateTime.UtcNow,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }

                            await _db.SaveChangesAsync();
                            totalSent += recordsToSend.Count;
                        }
                        catch (Exception ex)
                        {
                            totalFailed += recordsToSend.Count;

                            await _logService.LogErrorAsync(
                                jobLogId,
                                "KayanBulkSend",
                                "Failed to send records to Kayan.",
                                ex,
                                JsonSerializer.Serialize(kayanRequest),
                                kayanResponse);
                        }
                    }

                    var totalNum = hikResponse.Data.TotalNum;
                    var fetchedSoFar = pageIndex * pageSize;

                    hasMore = fetchedSoFar < totalNum;
                    pageIndex++;
                }

                if (totalFailed > 0 && totalSent > 0)
                {
                    await _logService.MarkPartialSuccessAsync(
                        jobLogId,
                        totalFetched,
                        totalSent,
                        totalFailed,
                        "Some records synced successfully, some failed.");
                }
                else if (totalFailed > 0)
                {
                    await _logService.MarkFailedAsync(
                        jobLogId,
                        "All records failed.",
                        totalFetched,
                        totalSent,
                        totalFailed);
                }
                else
                {
                    await _logService.MarkSuccessAsync(
                        jobLogId,
                        totalFetched,
                        totalSent,
                        "Attendance sync completed successfully.");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(
                    jobLogId,
                    "Unknown",
                    ex.Message,
                    ex);

                await _logService.MarkFailedAsync(jobLogId, ex.Message, totalFetched, totalSent, totalFailed);
            }
        }

        private static bool HasMissingRequiredKayanData(HikvisionRecord record)
        {
            return string.IsNullOrWhiteSpace(record.PersonInfo?.BaseInfo?.PersonCode);
        }

        private static void EnsureKayanResponseSuccessful(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new InvalidOperationException("Kayan returned an empty response body.");

            if (ContainsPlainTextFailureSignal(responseBody))
                throw new InvalidOperationException($"Kayan reported a failure. Response: {responseBody}");

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (ContainsFailureSignal(document.RootElement))
                    throw new InvalidOperationException($"Kayan reported one or more failed records. Response: {responseBody}");
            }
            catch (JsonException)
            {
                // Some Kayan installations return plain text on success. Plain text failure words were checked above.
            }
        }

        private static bool ContainsFailureSignal(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var name = property.Name;
                    var value = property.Value;

                    if ((name.Equals("success", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("isSuccess", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("isSuccessful", StringComparison.OrdinalIgnoreCase))
                        && value.ValueKind == JsonValueKind.False)
                        return true;

                    if ((name.Equals("hasError", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("hasErrors", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("isError", StringComparison.OrdinalIgnoreCase))
                        && value.ValueKind == JsonValueKind.True)
                        return true;

                    if ((name.Equals("failedCount", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("failureCount", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("totalFailed", StringComparison.OrdinalIgnoreCase))
                        && value.ValueKind == JsonValueKind.Number
                        && value.TryGetInt32(out var failureCount)
                        && failureCount > 0)
                        return true;

                    if ((name.Equals("status", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("result", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("message", StringComparison.OrdinalIgnoreCase))
                        && value.ValueKind == JsonValueKind.String
                        && ContainsPlainTextFailureSignal(value.GetString() ?? string.Empty))
                        return true;

                    if ((name.Equals("error", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("errors", StringComparison.OrdinalIgnoreCase)
                            || name.Equals("validationErrors", StringComparison.OrdinalIgnoreCase))
                        && HasContent(value))
                        return true;

                    if (ContainsFailureSignal(value))
                        return true;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (ContainsFailureSignal(item))
                        return true;
                }
            }

            return false;
        }

        private static bool ContainsPlainTextFailureSignal(string value)
        {
            return value.Contains("failed", StringComparison.OrdinalIgnoreCase)
                || value.Contains("failure", StringComparison.OrdinalIgnoreCase)
                || value.Contains("error", StringComparison.OrdinalIgnoreCase)
                || value.Contains("invalid", StringComparison.OrdinalIgnoreCase)
                || value.Contains("rejected", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasContent(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Array => element.GetArrayLength() > 0,
                JsonValueKind.Object => element.EnumerateObject().Any(),
                JsonValueKind.String => !string.IsNullOrWhiteSpace(element.GetString()),
                JsonValueKind.Null => false,
                JsonValueKind.Undefined => false,
                _ => true
            };
        }

        private static KayanAttendanceRequest MapToKayanRequest(HikvisionRecord record)
        {
            return new KayanAttendanceRequest
            {
                Tid = record.RecordGuid,
                EmployeeCardNumber = record.PersonInfo!.BaseInfo!.PersonCode,
                AttendanceDate = record.DeviceTime,
                FunctionType = MapDirection(record.Direction),
                MachineName = record.DeviceName
            };
        }

        private static string MapDirection(int direction)
        {
            return direction switch
            {
                0 => "IN",
                1 => "OUT",
                _ => "UNKNOWN"
            };
        }
    }
}
