using System.Text.Json.Serialization;

namespace AttendanceSync.Domain.Dtos
{
    public class HikvisionLoginResponse
    {
        [JsonPropertyName("data")]
        public HikvisionLoginData? Data { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class HikvisionLoginData
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;

        [JsonPropertyName("expireTime")]
        public long ExpireTime { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("areaDomain")]
        public string AreaDomain { get; set; } = null!;
    }
}
