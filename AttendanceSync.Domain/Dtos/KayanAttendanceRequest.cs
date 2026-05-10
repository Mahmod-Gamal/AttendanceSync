using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Dtos
{
    public class KayanAttendanceRequest
    {
        public string Tid { get; set; } = null!;
        public string EmployeeCardNumber { get; set; } = null!;
        public DateTime AttendanceDate { get; set; }
        public string FunctionType { get; set; } = null!;
        public string MachineName { get; set; } = null!;
    }
}
