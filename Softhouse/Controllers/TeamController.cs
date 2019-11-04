using Newtonsoft.Json;
using RestSharp;
using Softhouse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Softhouse.Controllers
{
    public class TeamController : Controller
    {
        private static List<Team> Teams;
        private static List<Player> ApiPlayers;

        public TeamController()
        {
            if (Teams == null)
            {
                Teams = GetTeams();
            }
            if (ApiPlayers == null)
            {
                ApiPlayers = GetPlayers();
            }
        }

        private List<Player> GetPlayers()
        {
            var client = new RestClient("https://free-nba.p.rapidapi.com/players?page=0&per_page=100");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "free-nba.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "47172a24dcmsh306ad131a519b9bp1e2af2jsnd7b014bcc3ba");
            IRestResponse response = client.Execute(request);
            Root<Player> root = JsonConvert.DeserializeObject<Root<Player>>(response.Content);
            return root.data;
        }

        private List<Team> GetTeams()
        {
            var client = new RestClient("https://free-nba.p.rapidapi.com/teams?page=0");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "free-nba.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "47172a24dcmsh306ad131a519b9bp1e2af2jsnd7b014bcc3ba");
            IRestResponse response = client.Execute(request);
            Root<Team> root = JsonConvert.DeserializeObject<Root<Team>>(response.Content);
            return root.data;
        }
        public ActionResult Index(string searchParam, string sortOrder)
        {

            ViewBag.CurrentSort = sortOrder;
            ViewBag.TeamSort = string.IsNullOrEmpty(sortOrder) ? "team_desc" : "";
            ViewBag.CitySort = sortOrder == "city" ? "city_desc" : "city";
            ViewBag.ConferenceSort = sortOrder == "conference" ? "conference_desc" : "conference";
            List<Team> list = Teams;
            if (!string.IsNullOrEmpty(searchParam))
            {
                list = GetTeams().Where(x => x.full_name.ToLower().Trim().Contains(searchParam)||
                x.city.ToLower().Trim().Contains(searchParam) ||
                x.conference.ToLower().Trim().Contains(searchParam) 
                ).ToList();
            }
            else
            {
                list = Teams;
            }

            switch (sortOrder)
            {
                case "team_desc":
                    list = list.OrderByDescending(x => x.full_name).ToList();
                    break;
                case "city":
                    list = list.OrderBy(x => x.city).ToList();
                    break;
                case "city_desc":
                    list = list.OrderByDescending(x => x.city).ToList();
                    break;
                case "conference":
                    list = list.OrderBy(x => x.conference).ToList();
                    break;
                case "conference_desc":
                    list = list.OrderByDescending(x => x.conference).ToList();
                    break;
                default:
                    list = list.OrderBy(x => x.full_name).ToList();
                    break;
            }

            return View(list);
        }


        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team team = Teams.Where(x=>x.id==id).FirstOrDefault();
            if (team == null)
            {
                return HttpNotFound();
            }
            ViewBag.Players= ApiPlayers.Where(x => x.team.id == id).ToList();

            return View(team);
        }
    }
}