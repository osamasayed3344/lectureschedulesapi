using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lectureschedule_api.Models
{
    public class Teachers
    {
        public int id { set; get; }
        public string name { set; get; }
        public string email { set; get; }
        public string phone { set; get; }
        public string country { set; get; }
        public string password { set; get; }
        public int numberoflecture { set; get; }
    }
}
