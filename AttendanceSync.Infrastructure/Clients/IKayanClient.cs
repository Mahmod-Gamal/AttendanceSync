using AttendanceSync.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Clients
{
    public interface IKayanClient
    {
        Task<string> SendBulkAttendanceAsync(List<KayanAttendanceRequest> request);
    }
}
