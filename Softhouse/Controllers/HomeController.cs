using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using Softhouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Softhouse.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Player");
        }

    }
}