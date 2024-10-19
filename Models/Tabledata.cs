using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lectureschedule_api.Models
{
    public class Tabledata
    {
        public string teachername { set; get; }
        public string subjectname { set; get; }
        public string hallname { set; get; }
        public string dayname { set; get; }
        public string timefrom { set; get; }
        public string timeto { set; get; }
    }

    public class ColsData
    {
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string HallName { get; set; }
        public string DayName { get; set; }
    }

    public class GroupsData
    {
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public List<ColsData> Cols { get; set; }
    }

    public class TabledataComparer
    {
        private List<GroupsData> allgroupsDatas;
        private List<GroupsData> postgroupsDatas;
        public TabledataComparer(List<GroupsData> allgroupsDatas, List<GroupsData> postgroupsDatas)
        {
            this.allgroupsDatas = allgroupsDatas;
            this.postgroupsDatas = postgroupsDatas;
        }

        public bool IsHallAvailable()
        {
            return !postgroupsDatas.Any(postindex => allgroupsDatas.Any(allindex => allindex.TimeFrom == postindex.TimeFrom && allindex.TimeTo == postindex.TimeTo && allindex.Cols.Any(allcol => postindex.Cols.Any(postcol => allcol.DayName == postcol.DayName && allcol.HallName == postcol.HallName))));
        }
    }
}
