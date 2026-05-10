using System.Text.Json.Serialization;

namespace AttendanceSync.Domain.Dtos
{
    public class HikvisionLoginRequest
    {
        [JsonPropertyName("appKey")]
        public string AppKey { get; set; } = null!;

        [JsonPropertyName("secretKey")]
        public string SecretKey { get; set; } = null!;
    }
}
