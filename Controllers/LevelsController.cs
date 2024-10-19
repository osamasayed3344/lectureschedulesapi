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
    public class LevelsController : Controller
    {
        List<Levels> levels = new List<Levels>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;

        // GET: api/<controller>
        [HttpGet]
        public List<Levels> Get()
        {
            connect.Open();
            command = new SqlCommand("select * from level order by levelid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                levels.Add(new Levels { id = int.Parse(reader["levelid"].ToString()), year = int.Parse(reader["levelyear"].ToString()) });
            }
            connect.Close();
            return levels;
        }

        // GET: api/<controller>/5
        [HttpGet("{id}")]
        public int Get(int id)
        {
            int levelselected = 0;
            connect.Open();
            command = new SqlCommand("select levelyear from level where levelid="+id+ " order by levelid ASC", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                levelselected = int.Parse(reader["levelyear"].ToString());
            }
            connect.Close();
            return levelselected;
        }

        // POST api/<controller>
        [HttpPost]
        public string Post([FromBody]Dictionary<string,string> data)
        {
            string error = "levelyear exists";
            string success = "successfull";
            try
            {
                if (data["levelyear"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("insert into level(levelyear) values(" + int.Parse(data["levelyear"]) + ")", connect);
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
            string error1 = "levelyear exists";
            string error2 = "successfull";
            try
            {
                if (data["levelyear"].ToString() != "")
                {
                    connect.Open();
                    command = new SqlCommand("update level set levelyear=" + int.Parse(data["levelyear"]) + " where levelid=" + id + "", connect);
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
            string error = "please delete levelyear of lecture schedule";

            List<Lectureschedule> lectureschedules = GetLectureschedule();
            int levelyear=Get(id);
            foreach (var lec in lectureschedules)
            {
                if (levelyear==lec.levelyear)
                {
                    return error;
                }
            }

            connect.Open();
            command = new SqlCommand("delete from level where levelid="+id+"",connect);
            command.ExecuteNonQuery();
            connect.Close();
            return success;
        }

        public List<Lectureschedule> GetLectureschedule()
        {
            List<Lectureschedule> alllecture = new List<Lectureschedule>();
            connect.Open();
            command = new SqlCommand("select levelyear,semestername from lectureschedule join level on(lectureschedule.levelid=level.levelid) join semester on(lectureschedule.semesterid=semester.semesterid);", connect);
            command.ExecuteNonQuery();
            SqlDataReader tablereader = command.ExecuteReader();
            while (tablereader.Read())
            {
                alllecture.Add(new Lectureschedule {levelyear=int.Parse(tablereader["levelyear"].ToString()),semestername=tablereader["semestername"].ToString()});
            }
            connect.Close();

            return alllecture;
        }
    }
}
