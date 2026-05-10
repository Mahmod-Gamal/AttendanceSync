using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Entities
{
    public class IntegrationErrorLog
    {
        public long Id { get; set; }
        public long? JobLogId { get; set; }
        public string Source { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
        public string? ExceptionDetails { get; set; }
        public string? RequestPayload { get; set; }
        public string? ResponsePayload { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
