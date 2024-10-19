using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Lectureschedule_api.Models;

namespace Lectureschedule_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamscheduleController : ControllerBase
    {
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;

        //---get
        [HttpGet]
        public List<Examschedule> Get()
        {
            List<Examschedule> examschedules = new List<Examschedule>();
            connect.Open();
            command = new SqlCommand("select levelyear,semestername from examschedule join level on(examschedule.levelid=level.levelid) join semester on(examschedule.semesterid=semester.semesterid);", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                examschedules.Add(new Examschedule {levelYear=int.Parse(reader["levelyear"].ToString()),semesterName=reader["semestername"].ToString() });
            }
            connect.Close();

            //------show select examtabledata
            foreach (var ex in examschedules)
            {
                List<Examtabledata> examtabledatas = new List<Examtabledata>();
                connect.Open();
                command = new SqlCommand("select subjectname,dayname,timefrom,timeto,dateexam from examtabledata join subject on(examtabledata.subjectid=subject.subjectid) join day on(examtabledata.dayid=day.dayid) where examscheduleid=(select examscheduleid from examschedule where levelid=(select levelid from level where levelyear=" + ex.levelYear+") and semesterid=(select semesterid from semester where semestername='"+ex.semesterName+"'));", connect);
                command.ExecuteNonQuery();
                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    examtabledatas.Add(new Examtabledata {subjectName=dataReader["subjectname"].ToString(),dayName= dataReader["dayname"].ToString(),Date= Convert.ToDateTime(dataReader["dateexam"]).ToString("yyyy-MM-dd"), timeFrom= dataReader["timefrom"].ToString(),timeTo= dataReader["timeto"].ToString() });
                }
                connect.Close();

                var grouptabledata = examtabledatas.GroupBy(x => new { x.timeFrom, x.timeTo }).Select(group => new GroupexamData {
                    timeFrom = group.Key.timeFrom,
                    timeTo=group.Key.timeTo,
                    cols=group.Select(groupcol=>new ColexamData {
                        subjectName=groupcol.subjectName,
                        dayName=groupcol.dayName,
                        Date=groupcol.Date
                    }).ToList()
                }).ToList();

                ex.examtabledata = grouptabledata;
            }
            return examschedules;
        }

        [HttpGet("{levelyear}/{semestername}")]
        public Examschedule Get(int levelyear, string semestername)
        {
            Examschedule examschedule = new Examschedule();
            connect.Open();
            command = new SqlCommand("select levelyear,semestername from examschedule join level on(examschedule.levelid=level.levelid) join semester on(examschedule.semesterid=semester.semesterid) where levelyear="+levelyear+" and semestername='"+semestername+"';", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                examschedule.levelYear =int.Parse(reader["levelyear"].ToString());
                examschedule.semesterName =reader["semestername"].ToString();
            }
            connect.Close();

            //---------show select tabledata
            List<Examtabledata> examtabledatas = new List<Examtabledata>();
            connect.Open();
            command = new SqlCommand("select subjectname,dayname,timefrom,timeto,dateexam from examtabledata join subject on(examtabledata.subjectid=subject.subjectid) join day on(examtabledata.dayid=day.dayid) where examscheduleid=(select examscheduleid from examschedule where levelid=(select levelid from level where levelyear=" + levelyear + ") and semesterid=(select semesterid from semester where semestername='" + semestername + "'));", connect);
            command.ExecuteNonQuery();
            SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                examtabledatas.Add(new Examtabledata { subjectName=dataReader["subjectname"].ToString(),dayName= dataReader["dayname"].ToString(),timeFrom= dataReader["timefrom"].ToString(),timeTo= dataReader["timeto"].ToString(),Date=Convert.ToDateTime(dataReader["dateexam"]).ToString("yyyy-MM-dd") });
            }
            connect.Close();

            var grouptabledata = examtabledatas.GroupBy(x => new { x.timeFrom, x.timeTo }).Select(group => new GroupexamData
            {
                timeFrom = group.Key.timeFrom,
                timeTo = group.Key.timeTo,
                cols = group.Select(groupcol => new ColexamData
                {
                    subjectName = groupcol.subjectName,
                    dayName = groupcol.dayName,
                    Date = groupcol.Date
                }).ToList()
            }).ToList();

            examschedule.examtabledata = grouptabledata;
            return examschedule;
        }
    }
}