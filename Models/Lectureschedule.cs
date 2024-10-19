using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lectureschedule_api.Models
{
    public class Lectureschedule
    {
        public int levelyear { set; get; }
        public string semestername { set; get; }
        public List<GroupsData> tabledatas { set; get; }
    }

}
