using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HPlusSport.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AliveController : ControllerBase
    {
        // GET: <AliveController>
        [HttpGet]
        public bool Get()
        {
            return true;
        }
    }
}
