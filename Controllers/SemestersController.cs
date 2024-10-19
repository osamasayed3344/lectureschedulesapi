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
    public class SemestersController : Controller
    {
        List<Semesters> semesters = new List<Semesters>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;
        // GET: api/<controller>
        [HttpGet]
        public List<Semesters> Get()
        {
            connect.Open();
            command = new SqlCommand("select * from semester order by semesterid", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                semesters.Add(new Semesters {semestername= reader["semestername"].ToString(), semesterid= int.Parse(reader["semesterid"].ToString()) });
            }
            connect.Close();
            return semesters;
        }

        // GET: api/<controller>/5
        [HttpGet("{id}")]
        public List<string> Get(int id)
        {
            List<string> semesterselected = new List<string>();
            connect.Open();
            command = new SqlCommand("select semestername from semester where semesterid=" + id + " order by semesterid", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                semesterselected.Add(reader["semestername"].ToString());
            }
            connect.Close();
            return semesterselected;
        }
    }
}
