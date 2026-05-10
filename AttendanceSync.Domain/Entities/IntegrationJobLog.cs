using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Entities
{
    public class IntegrationJobLog
    {
        public long Id { get; set; }
        public string JobName { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string Status { get; set; } = null!;
        public int TotalFetchedRecords { get; set; }
        public int TotalSentRecords { get; set; }
        public int TotalFailedRecords { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
