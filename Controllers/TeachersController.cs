using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lectureschedule_api.Models;
using System.Data.SqlClient;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lectureschedule_api.Controllers
{
    [Route("api/[controller]")]
    public class TeachersController : Controller
    {
        List<Teachers> teachers = new List<Teachers>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;
        // GET: api/<controller>
        [HttpGet]
        public List<Teachers> Get()
        {
            connect.Open();
            command = new SqlCommand("select * from teacher  order by teacherid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teachers.Add(new Teachers {id=int.Parse(reader["teacherid"].ToString()),name=reader["teachername"].ToString(),email=reader["email"].ToString(),phone=reader["phone"].ToString(),country=reader["country"].ToString(),password=reader["password"].ToString(),numberoflecture=int.Parse(reader["lectureofcount"].ToString())});
            }
            connect.Close();
            return teachers;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Teachers Get(int id)
        {
            Teachers teacherselected = GetTeachers(id);
            return teacherselected;
        }

        public Teachers GetTeachers(int id)
        {
            Teachers teacherselected = new Teachers();
            connect.Open();
            command = new SqlCommand("select * from teacher where teacherid=" + id + " order by teacherid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                teacherselected.id = int.Parse(reader["teacherid"].ToString());
                teacherselected.name = reader["teachername"].ToString();
                teacherselected.phone = reader["phone"].ToString();
                teacherselected.password = reader["password"].ToString();
                teacherselected.numberoflecture = int.Parse(reader["lectureofcount"].ToString());
                teacherselected.email = reader["email"].ToString();
                teacherselected.country = reader["country"].ToString();
            }
            connect.Close();
            return teacherselected;
        }
        // POST api/<controller>
        [HttpPost]
        public string Post([FromBody]Dictionary<string,string> data)
        {
            string success = "successfull";
            string error = "User exists";
            try
            {
                connect.Open();
                command = new SqlCommand("insert into teacher(teachername,email,phone,country,password,lectureofcount) values('" + data["name"] + "','" + data["email"] + "','" + data["phone"] + "','" + data["country"] + "','" + data["password"] + "'," + int.Parse(data["numberoflecture"]) + ")", connect);
                command.ExecuteNonQuery();
                connect.Close();
                return success;
            }
            catch
            {
                return error;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            string success = "successfull";
            string error = "please delete teacher of lecture schedule";

            List<Tabledata> lectureschedules = GetLectureschedule();
            Teachers teachers = GetTeachers(id);
            foreach (var lec in lectureschedules)
            {
                if (teachers.name == lec.teachername)
                {
                    return error;
                }
            }
            //delete
            connect.Open();
            command = new SqlCommand("delete from teacher where teacherid = " + id + "", connect);
            command.ExecuteNonQuery();
            connect.Close();
            return success;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public string Put(int id, [FromBody]Dictionary<string,string> data)
        {
            string error1 = "Email is used";
            string error2 = "successfull";
            try
            {
                connect.Open();
                command = new SqlCommand("update teacher set teachername='" + data["name"] + "', email='" + data["email"] + "', phone = '" + data["phone"] + "' ,country='" + data["country"] + "', password='" + data["password"] + "', lectureofcount=" + int.Parse(data["numberoflecture"]) + " where teacherid=" + id + " ", connect);
                command.ExecuteNonQuery();
                connect.Close();
                return error2;
            }
            catch
            {
                return error1;
            }
        }

        public List<Tabledata> GetLectureschedule()
        {
            List<Tabledata> alltabledata = new List<Tabledata>();
            connect.Open();
            command = new SqlCommand("select teachername,subjectname,hallname from tabledata join tabledatatoteacher on (tabledata.tabledataid=tabledatatoteacher.tabledataid) join teacher on(tabledatatoteacher.teacherid=teacher.teacherid) join subject on(tabledata.subjectid=subject.subjectid) join hall on(tabledata.hallid=hall.hallid);", connect);
            command.ExecuteNonQuery();
            SqlDataReader tablereader = command.ExecuteReader();
            while (tablereader.Read())
            {
                alltabledata.Add(new Tabledata { teachername = tablereader["teachername"].ToString(), subjectname = tablereader["subjectname"].ToString(), hallname = tablereader["hallname"].ToString() });
            }
            connect.Close();

            return alltabledata;
        }
    }
}
