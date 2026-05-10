using System.Text.Json.Serialization;

namespace AttendanceSync.Domain.Dtos
{
    public class HikvisionAttendanceResponse
    {
        [JsonPropertyName("data")]
        public HikvisionAttendanceData? Data { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class HikvisionAttendanceData
    {
        [JsonPropertyName("totalNum")]
        public int TotalNum { get; set; }

        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("recordList")]
        public List<HikvisionRecord> RecordList { get; set; } = new();
    }

    public class HikvisionRecord
    {
        [JsonPropertyName("recordGuid")]
        public string RecordGuid { get; set; } = null!;

        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = null!;

        [JsonPropertyName("deviceTime")]
        public DateTime DeviceTime { get; set; }

        [JsonPropertyName("direction")]
        public int Direction { get; set; }

        [JsonPropertyName("personInfo")]
        public HikvisionPersonInfo? PersonInfo { get; set; }
    }

    public class HikvisionPersonInfo
    {
        [JsonPropertyName("baseInfo")]
        public HikvisionBaseInfo? BaseInfo { get; set; }
    }

    public class HikvisionBaseInfo
    {
        [JsonPropertyName("personCode")]
        public string PersonCode { get; set; } = null!;

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
    }
}
