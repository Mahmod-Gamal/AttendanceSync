using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Enums
{
    public enum IntegrationErrorSource
    {
        HikvisionAuth = 1,
        HikvisionAttendanceFetch = 2,
        KayanBulkSend = 3,
        Database = 4,
        Mapping = 5,
        Unknown = 6
    }
}
