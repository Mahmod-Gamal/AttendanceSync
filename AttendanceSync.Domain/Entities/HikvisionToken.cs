using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Domain.Entities
{
    public class HikvisionToken
    {
        public int Id { get; set; }
        public string AccessToken { get; set; } = null!;
        public long ExpireTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
