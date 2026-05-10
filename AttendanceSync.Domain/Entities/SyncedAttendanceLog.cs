using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Entities
{
    public class SyncedAttendanceLog
    {
        public long Id { get; set; }
        public string HikvisionRecordId { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public DateTime AttendanceTime { get; set; }
        public string? Direction { get; set; }
        public string? KayanRequestPayload { get; set; }
        public string? KayanResponsePayload { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
