using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Lectureschedule_api.Controllers
{
    [Route("api/[controller]")]
    public class DaysController : Controller
    {
        List<string> days = new List<string>();
        SqlConnection connect = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=lectureschedule_db;Integrated Security=True");
        SqlCommand command;
        // GET: api/<controller>
        [HttpGet]
        public List<string> Get()
        {
            connect.Open();
            command = new SqlCommand("select dayname from day",connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                days.Add(reader["dayname"].ToString());
            }
            connect.Close();
            return days;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public List<string> Get(int id)
        {
            List<string> dayselected    = new List<string>();
            connect.Open();
            command = new SqlCommand("select dayname from day where dayid="+id+"", connect);
            command.ExecuteNonQuery();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                dayselected.Add(reader["dayname"].ToString());
            }
            connect.Close();
            return dayselected;
        }
    }
}
