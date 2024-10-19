using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lectureschedule_api.Models
{
    public class Examschedule
    {
        public int levelYear { set; get; }
        public string semesterName { set; get; }
        public List<GroupexamData> examtabledata { set; get; }
    }
}
