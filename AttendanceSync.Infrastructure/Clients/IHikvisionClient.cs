using AttendanceSync.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure.Clients
{
    public interface IHikvisionClient
    {
        Task<string> GetValidTokenAsync();
        Task<HikvisionAttendanceResponse?> GetAttendanceAsync(
            string token,
            DateTime beginTime,
            DateTime endTime,
            int pageIndex,
            int pageSize);
    }
}
