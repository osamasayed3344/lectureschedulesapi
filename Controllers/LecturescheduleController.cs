using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Lectureschedule_api.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lectureschedule_api.Controllers
{
    [Route("api/[controller]")]
    public class LecturescheduleController : Controller
    {
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;
        // GET: api/<controller>
        [HttpGet]
        public List<Lectureschedule> Get()
        {
            List<Lectureschedule> lectureschedule = new List<Lectureschedule>();
            connect.Open();
            command = new SqlCommand("select levelyear,semestername from lectureschedule join level on(lectureschedule.levelid=level.levelid) join semester on(lectureschedule.semesterid=semester.semesterid); ", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                lectureschedule.Add(new Lectureschedule {levelyear= int.Parse(reader["levelyear"].ToString()),semestername= reader["semestername"].ToString() });
            }
            connect.Close();
            foreach (var tabledata in lectureschedule) {
                List<Tabledata> alltabledata = new List<Tabledata>();
                connect.Open();
                command = new SqlCommand("select teachername,subjectname,hallname,dayname,timefrom,timeto from tabledata join tabledatatoteacher on (tabledata.tabledataid=tabledatatoteacher.tabledataid) join teacher on(tabledatatoteacher.teacherid=teacher.teacherid) join subject on(tabledata.subjectid=subject.subjectid) join hall on(tabledata.hallid=hall.hallid) join day on(tabledata.dayid=day.dayid) where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + tabledata.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + tabledata.semestername + "')); ", connect);
                command.ExecuteNonQuery();
                SqlDataReader tablereader = command.ExecuteReader();
                while (tablereader.Read())
                {
                    alltabledata.Add(new Tabledata { teachername = tablereader["teachername"].ToString(), subjectname = tablereader["subjectname"].ToString(), hallname = tablereader["hallname"].ToString(), dayname = tablereader["dayname"].ToString(), timefrom = tablereader["timefrom"].ToString(), timeto = tablereader["timeto"].ToString() });
                }
                connect.Close();
                //---select
                var grouptabledata = alltabledata.GroupBy(x => new { x.timefrom,x.timeto}).Select(group => new GroupsData {
                    TimeFrom=group.Key.timefrom,
                    TimeTo=group.Key.timeto,
                    Cols=group.Select(groupcol=> new ColsData {
                        DayName=groupcol.dayname,
                        TeacherName=groupcol.teachername,
                        SubjectName=groupcol.subjectname,
                        HallName=groupcol.hallname
                    }).ToList()
                }).ToList();
                //---display
                tabledata.tabledatas=grouptabledata;
            }
            return lectureschedule;
        }

        // GET: api/<controller>/1
        [HttpGet("{levelyear}/{semestername}")]
        public Lectureschedule Get(int levelyear,string semestername)
        {
            Lectureschedule lectureschedulesselected = GetLectureschedule(levelyear,semestername);
            return lectureschedulesselected;
        }

        // POST api/<controller>
        [HttpPost]
        public string Post([FromBody]Lectureschedule lectureschedule)
        {
            string error1 = "Please enter your level and semester";
            string error2 = "Lecture Schedule is exists";
            string error3 = "Tabledata is exists";
            string error4 = "Please enter your table data";
            string success ="successfull";
            string errcatch = "";

            try
            {
                if (lectureschedule.levelyear != 0 && lectureschedule.semestername != null)
                {
                    List<Lectureschedule> listlectureschedule = new List<Lectureschedule>();
                    connect.Open();
                    command = new SqlCommand("select levelyear,semestername from lectureschedule join level on(lectureschedule.levelid=level.levelid) join semester on(lectureschedule.semesterid=semester.semesterid);", connect);
                    command.ExecuteNonQuery();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        listlectureschedule.Add(new Lectureschedule { levelyear = int.Parse(reader["levelyear"].ToString()), semestername = reader["semestername"].ToString() });
                    }
                    connect.Close();
                    //----------------------------------select table data-------------------------------------------
                    List<Tabledata> alltabledatas = new List<Tabledata>();
                    connect.Open();
                    command = new SqlCommand("select hallname,dayname,timefrom,timeto from tabledata join hall on(tabledata.hallid=hall.hallid) join day on(tabledata.dayid=day.dayid);", connect);
                    command.ExecuteNonQuery();
                    SqlDataReader readertabledata = command.ExecuteReader();
                    while (readertabledata.Read())
                    {
                        alltabledatas.Add(new Tabledata { timefrom = readertabledata["timefrom"].ToString(), timeto = readertabledata["timeto"].ToString(), dayname = readertabledata["dayname"].ToString(), hallname = readertabledata["hallname"].ToString() });
                    }
                    connect.Close();
                    //------------------select group
                    var grouptabledata = alltabledatas.GroupBy(x => new { x.timefrom, x.timeto }).Select(group => new GroupsData
                    {
                        TimeFrom = group.Key.timefrom,
                        TimeTo = group.Key.timeto,
                        Cols = group.Select(groupcol => new ColsData
                        {
                            DayName = groupcol.dayname,
                            HallName = groupcol.hallname
                        }).ToList()
                    }).ToList();
                    //----------insert------------------
                    var checklevelyear = listlectureschedule.Any(x => x.levelyear == lectureschedule.levelyear && x.semestername == lectureschedule.semestername);
                    if (checklevelyear)
                    {
                        return error2;
                    }
                    else
                    {
                        var posttabledata = lectureschedule.tabledatas.OfType<GroupsData>().ToList();
                        var check = new TabledataComparer(grouptabledata, posttabledata);
                        var checkempty = !posttabledata.Any(x => (x.TimeFrom == "" && x.TimeTo == "") || (x.Cols == null || !x.Cols.Any(y => y.TeacherName != "" || y.SubjectName != "" || y.HallName != "" || y.DayName != "")));
                        if (checkempty)
                        {
                            if (check.IsHallAvailable())
                            {
                                connect.Open();
                                command = new SqlCommand("insert into lectureschedule(levelid,semesterid) values((select levelid from level where levelyear=" + lectureschedule.levelyear + "),(select semesterid from semester where semestername='" + lectureschedule.semestername + "')); ", connect);
                                command.ExecuteNonQuery();
                                connect.Close();
                                //-----insert tabledata-------
                                foreach (var row in posttabledata)
                                {
                                    foreach (var col in row.Cols)
                                    {
                                        connect.Open();
                                        command = new SqlCommand("insert into tabledata(lecturescheduleid,subjectid,hallid,dayid,timefrom,timeto) values((select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')),(select subjectid from subject where subjectname='" + col.SubjectName + "'),(select hallid from hall where hallname='" + col.HallName + "'),(select dayid from day where dayname='" + col.DayName + "'),'" + row.TimeFrom + "','" + row.TimeTo + "'); " +
                                            "insert into tabledatatoteacher(tabledataid,teacherid) values( (select tabledataid from tabledata where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and subjectid=(select subjectid from subject where subjectname='" + col.SubjectName + "') and hallid=(select hallid from hall where hallname='" + col.HallName + "') and dayid=(select dayid from day where dayname='" + col.DayName + "') and timefrom='" + row.TimeFrom + "' and timeto='" + row.TimeTo + "') , (select teacherid from teacher where teachername ='" + col.TeacherName + "') ); ", connect);
                                        command.ExecuteNonQuery();
                                        connect.Close();
                                    }
                                }
                                return success;
                            }
                            else
                            {
                                return error3;
                            }
                        }
                        else
                        {
                            return error4;
                        }
                    }
                }
                else
                {
                    return error1;
                }
            }
            catch(Exception e)
            {
                errcatch = e.Message;
                return errcatch;
            }
        }

        // PUT api/<controller>/5
        [HttpPut]
        public string Put([FromBody]Lectureschedule lectureschedule)
        {
            string success = "successfull";
            string error = "Tabledata is exists";

            List<Tabledata> alltabledatas = new List<Tabledata>();
            connect.Open();
            command = new SqlCommand("select hallname,dayname,timefrom,timeto from tabledata join hall on(tabledata.hallid=hall.hallid) join day on(tabledata.dayid=day.dayid) where lecturescheduleid!=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear="+lectureschedule.levelyear+") and semesterid=(select semesterid from semester where semestername='"+lectureschedule.semestername+"'));", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                alltabledatas.Add(new Tabledata {hallname=reader["hallname"].ToString(),dayname= reader["dayname"].ToString(),timefrom= reader["timefrom"].ToString(),timeto= reader["timeto"].ToString() });
            }
            connect.Close();
            var grouptabledata = alltabledatas.GroupBy(x => new { x.timefrom, x.timeto }).Select(group => new GroupsData
            {
                TimeFrom = group.Key.timefrom,
                TimeTo = group.Key.timeto,
                Cols = group.Select(groupcol => new ColsData
                {
                    DayName = groupcol.dayname,
                    HallName = groupcol.hallname
                }).ToList()
            }).ToList();

            var posttabledata = lectureschedule.tabledatas.OfType<GroupsData>().ToList();
            var check = new TabledataComparer(grouptabledata,posttabledata);

            if (check.IsHallAvailable())
            {
                foreach (var tabledata in lectureschedule.tabledatas)
                {
                    foreach (var col in tabledata.Cols)
                    {
                        connect.Open();
                        command = new SqlCommand("select COUNT(1) from tabledata join tabledatatoteacher on (tabledata.tabledataid=tabledatatoteacher.tabledataid)  where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and timefrom='" + tabledata.TimeFrom + "' and timeto='" + tabledata.TimeTo + "' and dayid=(select dayid from day where dayname='" + col.DayName + "');", connect);
                        command.ExecuteNonQuery();
                        int rows = (int)command.ExecuteScalar();
                        connect.Close();
                        if (rows > 0)
                        {
                            connect.Open();
                            command = new SqlCommand("update tabledata set subjectid=(select subjectid from subject where subjectname='" + col.SubjectName + "'), hallid=(select hallid from hall where hallname='" + col.HallName + "') where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and timefrom='" + tabledata.TimeFrom + "' and timeto='" + tabledata.TimeTo + "' and dayid=(select dayid from day where dayname='" + col.DayName + "');" +
                                "update tabledatatoteacher set teacherid=(select teacherid from teacher where teachername='" + col.TeacherName + "') where tabledataid=(select tabledataid from tabledata where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and timefrom='" + tabledata.TimeFrom + "' and timeto='" + tabledata.TimeTo + "' and dayid=(select dayid from day where dayname='" + col.DayName + "'));", connect);
                            command.ExecuteNonQuery();
                            connect.Close();
                        }
                        else
                        {
                            connect.Open();
                            command = new SqlCommand("insert into tabledata(lecturescheduleid,subjectid,hallid,dayid,timefrom,timeto) values((select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')),(select subjectid from subject where subjectname='" + col.SubjectName + "'),(select hallid from hall where hallname='" + col.HallName + "'),(select dayid from day where dayname='" + col.DayName + "'),'" + tabledata.TimeFrom + "','" + tabledata.TimeTo + "');" +
                                            "insert into tabledatatoteacher(tabledataid,teacherid) values( (select tabledataid from tabledata where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and subjectid=(select subjectid from subject where subjectname='" + col.SubjectName + "') and hallid=(select hallid from hall where hallname='" + col.HallName + "') and dayid=(select dayid from day where dayname='" + col.DayName + "') and timefrom='" + tabledata.TimeFrom + "' and timeto='" + tabledata.TimeTo + "') , (select teacherid from teacher where teachername ='" + col.TeacherName + "') );", connect);
                            command.ExecuteNonQuery();
                            connect.Close();
                        }

                    }
                }
                return success;
            }
            else
            {
                return error;
            }

        }

        // DELETE api/<controller>/5
        [HttpDelete("{levelyear}/{semestername}")]
        public string Delete(int levelyear,string semestername)
        {
            string success = "successfull";

            Lectureschedule lectureschedule= GetLectureschedule(levelyear,semestername);
            foreach (var row in lectureschedule.tabledatas)
            {
                foreach (var col in row.Cols)
                {
                    connect.Open();
                    command = new SqlCommand("delete from tabledatatoteacher where tabledataid=(select tabledataid from tabledata where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid= (select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and timefrom='" + row.TimeFrom + "' and timeto='" + row.TimeTo + "' and dayid=(select dayid from day where dayname='" + col.DayName + "'));" +
                        "delete from tabledata where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "')) and timefrom='" + row.TimeFrom + "' and timeto='" + row.TimeTo + "' and dayid=(select dayid from day where dayname='" + col.DayName + "');", connect);
                    command.ExecuteNonQuery();
                    connect.Close();
                }
            }
            connect.Open();
            command = new SqlCommand("delete from lectureschedule where levelid= (select levelid from level where levelyear=" + lectureschedule.levelyear + ") and semesterid=(select semesterid from semester where semestername='" + lectureschedule.semestername + "');", connect);
            command.ExecuteNonQuery();
            connect.Close();

            return success;
        }

        public Lectureschedule GetLectureschedule(int levelyear, string semestername)
        {
            Lectureschedule lectureschedulesselected = new Lectureschedule();
            connect.Open();
            command = new SqlCommand("select levelyear,semestername from lectureschedule join level on(lectureschedule.levelid=level.levelid) join semester on(lectureschedule.semesterid=semester.semesterid) where levelyear=" + levelyear + " and semestername='" + semestername + "';", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                lectureschedulesselected.levelyear = int.Parse(reader["levelyear"].ToString());
                lectureschedulesselected.semestername = reader["semestername"].ToString();
            }
            connect.Close();
            List<Tabledata> alltabledata = new List<Tabledata>();
            connect.Open();
            command = new SqlCommand("select teachername,subjectname,hallname,dayname,timefrom,timeto from tabledata join tabledatatoteacher on (tabledata.tabledataid=tabledatatoteacher.tabledataid) join teacher on(tabledatatoteacher.teacherid=teacher.teacherid) join subject on(tabledata.subjectid=subject.subjectid) join hall on(tabledata.hallid=hall.hallid) join day on(tabledata.dayid=day.dayid) where lecturescheduleid=(select lecturescheduleid from lectureschedule where levelid=(select levelid from level where levelyear=" + levelyear + ") and semesterid=(select semesterid from semester where semestername='" + semestername + "'));", connect);
            command.ExecuteNonQuery();
            SqlDataReader tablereader = command.ExecuteReader();
            while (tablereader.Read())
            {
                alltabledata.Add(new Tabledata { teachername = tablereader["teachername"].ToString(), subjectname = tablereader["subjectname"].ToString(), hallname = tablereader["hallname"].ToString(), dayname = tablereader["dayname"].ToString(), timefrom = tablereader["timefrom"].ToString(), timeto = tablereader["timeto"].ToString() });
            }
            connect.Close();
            //---select
            var grouptabledata = alltabledata.GroupBy(x => new { x.timefrom, x.timeto }).Select(group => new GroupsData
            {
                TimeFrom = group.Key.timefrom,
                TimeTo = group.Key.timeto,
                Cols = group.Select(groupcol => new ColsData
                {
                    DayName = groupcol.dayname,
                    TeacherName = groupcol.teachername,
                    SubjectName = groupcol.subjectname,
                    HallName = groupcol.hallname
                }).ToList()
            }).ToList();
            //---display
            lectureschedulesselected.tabledatas = grouptabledata;

            return lectureschedulesselected;
        }
    }
}
