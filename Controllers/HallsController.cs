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
    public class HallsController : Controller
    {
        List<Halls> halls = new List<Halls>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;

        // GET: api/<controller>
        [HttpGet]
        public List<Halls> Get()
        {
            connect.Open();
            command = new SqlCommand("select * from hall order by hallid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                halls.Add(new Halls { id = int.Parse(dataReader["hallid"].ToString()), name = dataReader["hallname"].ToString() });
            }
            connect.Close();
            return halls;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Halls Get(int id)
        {
            Halls hallselected = new Halls();
            connect.Open();
            command = new SqlCommand("select * from hall where hallid=" + id + " order by hallid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                hallselected.id = int.Parse(reader["hallid"].ToString());
                hallselected.name = reader["hallname"].ToString();
            }
            connect.Close();
            return hallselected;
        }

        // POST api/<controller>
        [HttpPost]
        public string Post([FromBody]Dictionary<string,string> data)
        {
            string error = "hallname exists";
            string success = "successfull";
            try
            {
                if (data["hallname"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("insert into hall(hallname) values('" + data["hallname"] + "')", connect);
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
        public string Put(int id, [FromBody]Dictionary<string,string> data)
        {
            string error1 = "hallname exists";
            string error2 = "successfull";
            try
            {
                if (data["hallname"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("update hall set hallname='" + data["hallname"] + "' where hallid=" + id + " ", connect);
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
            string error = "please delete hall of lecture schedule";

            List<Tabledata> tabledatas = GetLectureschedule();
            Halls halls = Get(id);
            foreach (var tabledata in tabledatas)
            {
                if (tabledata.hallname==halls.name)
                {
                    return error;
                }
            }

            connect.Open();
            command = new SqlCommand("delete from hall where hallid = " + id + "", connect);
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
