using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lectureschedule_api.Models
{
    public class Examtabledata
    {
        public string timeFrom { set; get; }
        public string timeTo { set; get; }
        public string dayName { set; get; }
        public string Date { set; get; }
        public string subjectName { set; get; }
    }

    public class GroupexamData
    {
        public string timeFrom { set; get; }
        public string timeTo { set; get; }
        public List<ColexamData> cols { set; get; }
    }

    public class ColexamData
    {
        public string dayName { set; get; }
        public string Date { set; get; }
        public string subjectName { set; get; }
    }
}
