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
    public class SubjectsController : Controller
    {
        List<Subjects> subjects = new List<Subjects>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;
        // GET: api/<controller>
        [HttpGet]
        public List<Subjects> Get()
        {
            connect.Open();
            command = new SqlCommand("select * from subject order by subjectid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                subjects.Add(new Subjects {subjectid=int.Parse(reader["subjectid"].ToString()),subjectname=reader["subjectname"].ToString() });
            }
            connect.Close();
            return subjects;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Subjects Get(int id)
        {
            Subjects subjectselected = new Subjects();
            connect.Open();
            command = new SqlCommand("select * from subject where subjectid="+id+ " order by subjectid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                subjectselected.subjectid = int.Parse(reader["subjectid"].ToString());
                subjectselected.subjectname = reader["subjectname"].ToString();
            }
            connect.Close();
            return subjectselected;
        }

        // POST api/<controller>
        [HttpPost]
        public string Post([FromBody]Dictionary<string,string> data)
        {
            string error = "subject exists";
            string success = "successfull";
            try
            {
                if(data["subjectname"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("insert into subject(subjectname) values('" + data["subjectname"] + "')", connect);
                    command.ExecuteNonQuery();
                    connect.Close();
                }
                return success;
            }
            catch
            {
                return error;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public string Put(int id, [FromBody]Dictionary<string, string> data)
        {
            string error1 = "subject exists";
            string error2 = "successfull";
            try
            {
                if(data["subjectname"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("update subject set subjectname = '" + data["subjectname"] + "' where subjectid = " + id + "", connect);
                    command.ExecuteNonQuery();
                    connect.Close();
                }
                return error2;
            }
            catch
            {
                return error1;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            string success = "successfull";
            string error = "please delete subject of lecture schedule";

            List<Tabledata> tabledatas = GetLectureschedule();
            Subjects subjects = Get(id);
            foreach (var tabledata in tabledatas)
            {
                if (tabledata.subjectname==subjects.subjectname)
                {
                    return error;
                }
            }

            connect.Open();
            command = new SqlCommand("delete from subject where subjectid = " + id + "", connect);
            command.ExecuteNonQuery();
            connect.Close();
            return success;
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
