using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;

namespace CustomAuthApp.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = "apikey")]
    public class HomeController : Controller
    {
        public IActionResult Get()
        {
           return Ok();
        }
    }
}
