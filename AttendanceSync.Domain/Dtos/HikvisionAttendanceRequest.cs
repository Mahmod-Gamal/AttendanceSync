using System.Text.Json.Serialization;

namespace AttendanceSync.Domain.Dtos
{
    public class HikvisionAttendanceRequest
    {
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("searchCriteria")]
        public HikvisionAttendanceSearchCriteria SearchCriteria { get; set; } = new();
    }

    public class HikvisionAttendanceSearchCriteria
    {
        [JsonPropertyName("beginTime")]
        public string BeginTime { get; set; } = null!;

        [JsonPropertyName("endTime")]
        public string EndTime { get; set; } = null!;

        [JsonPropertyName("type")]
        public int Type { get; set; } = 1;
    }
}
