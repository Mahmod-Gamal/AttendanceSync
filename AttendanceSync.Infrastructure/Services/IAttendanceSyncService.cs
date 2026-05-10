using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Services
{
    public interface IAttendanceSyncService
    {
        Task RunAsync(long jobLogId);
    }
}
