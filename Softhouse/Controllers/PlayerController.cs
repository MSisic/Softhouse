using Newtonsoft.Json;
using PagedList;
using RestSharp;
using Softhouse.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Softhouse.Controllers
{
    public class PlayerController : Controller
    {
        private static List<Player> ApiPlayers;
        private static List<Player> LocalPlayer;
        private static IPagedList<Player> JsonPlayer;
        private static List<Team> Teams;

        public PlayerController()
        {
            if (ApiPlayers == null)
            {
                ApiPlayers = GetPlayers();
                if (LocalPlayer == null)
                {
                    LocalPlayer = ApiPlayers;
                }
                if (Teams == null)
                {
                    Teams = GetTeams();
                }
            }
        }

        #region API
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
        #endregion
        
        private List<SelectListItem> GetPositions(string action="")
        {
            List<SelectListItem> positions = new List<SelectListItem>();
            if (action == "index")
            {
                positions.Add(new SelectListItem { Text = "All", Value = null });
            }
            positions.Add(new SelectListItem { Text = "C", Value = "C" });
            positions.Add(new SelectListItem { Text = "F", Value = "F" });
            positions.Add(new SelectListItem { Text = "G", Value = "G" });
            return positions;
        }

        public ActionResult Index(string sortOrder, string searchParam, string position, string teamid, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.TeamSort = sortOrder == "team" ? "team_desc" : "team";
            ViewBag.PositionSort = sortOrder == "position" ? "position_desc" : "position";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            List<SelectListItem> ListTeams = new List<SelectListItem>();
            ListTeams.Add(new SelectListItem { Value = null, Text = "All" });
            ListTeams.AddRange(Teams.Select(x => new SelectListItem { Text = x.full_name, Value = x.id }).ToList());
            ViewBag.teamId = ListTeams;
            ViewBag.position = GetPositions("index");


            List<Player> list = LocalPlayer;
            if (!string.IsNullOrEmpty(searchParam))
            {
                list = LocalPlayer.Where(x => x.first_name.ToLower().Trim().Contains(searchParam)
                || x.last_name.ToLower().Trim().Contains(searchParam)
                || x.position.ToLower().Trim().Contains(searchParam)
                || x.team.full_name.ToLower().Trim().Contains(searchParam)).ToList();
            }
            if (!string.IsNullOrEmpty(position)&&position!="All")
            {
                list = list.Where(x => x.position == position).ToList();
            }
            if (!string.IsNullOrEmpty(teamid)&&teamid!="All")
            {
                list = list.Where(x => x.team.id == teamid).ToList();
            }
            switch (sortOrder)
            {
                case "name_desc":
                    list = list.OrderByDescending(x => x.first_name).ToList();
                    break;
                case "team":
                    list = list.OrderBy(x => x.team.full_name).ToList();
                    break;
                case "team_desc":
                    list = list.OrderByDescending(x => x.team.full_name).ToList();
                    break;
                case "position":
                    list = list.OrderBy(x => x.position).ToList();
                    break;
                case "position_desc":
                    list = list.OrderByDescending(x => x.position).ToList();
                    break;
                default:
                    list = list.OrderBy(x => x.first_name).ToList();
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            JsonPlayer = list.ToPagedList(pageNumber, pageSize);
            return View(list.ToPagedList(pageNumber,pageSize));
        }

        // GET: Player/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View("Index", LocalPlayer);
            }

            Player player = LocalPlayer.Where(x => x.id == id).FirstOrDefault();
            if (player == null)
            {
                return View("Index", LocalPlayer);
            }
            return View(player);
        }

        // GET: Player/Create
        public ActionResult Create()
        {
            ViewBag.teamId = new SelectList(Teams, "id", "full_name");
            ViewBag.position = GetPositions();
            return View();
        }

        // POST: Player/Create
        [HttpPost]
        public ActionResult Create(Player player)
        {
            if (ModelState.IsValid)
            {
                player.id = Guid.NewGuid().ToString();
                player.team= Teams.Where(x => x.id == player.teamId).FirstOrDefault();
                LocalPlayer.Add(player);
                return RedirectToAction("Index");
            }

            ViewBag.teamId = new SelectList(Teams, "id", "full_name");
            ViewBag.position = GetPositions();
            return View(player);
        }

        // GET: Player/Edit/5
        public ActionResult Edit(string id)
        {
            ViewBag.teamId = new SelectList(Teams, "id", "full_name");
            ViewBag.positionId = GetPositions();
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            Player player = LocalPlayer.Where(x => x.id == id).FirstOrDefault();
            if (player == null)
            {
                return RedirectToAction("Index");
            }
            player.teamId = player.team.id;
            ViewBag.teamId = new SelectList(Teams, "id", "full_name", player.teamId);
            ViewBag.position = new SelectList(GetPositions(), "value", "text", player.position);

            return View(player);           
        }

        // POST: Player/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, Player player)
        {
            if (ModelState.IsValid)
            {
                Player P=LocalPlayer.Where(x => x.id == id).FirstOrDefault();
                int index= LocalPlayer.IndexOf(P);
                player.team = Teams.Where(x => x.id == player.teamId).FirstOrDefault();
                LocalPlayer[index] = player;
                return RedirectToAction("Index");
            }
            ViewBag.teamId = new SelectList(Teams, "id", "city", player.teamId);
            ViewBag.position = new SelectList(GetPositions(), "value", "text", player.position);

            return View(player);
        }

        // GET: Player/Delete/5
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            Player player = LocalPlayer.Where(x => x.id == id).FirstOrDefault();
            if (player == null)
            {
                return RedirectToAction("Index");
            }
            return View("ConfirmDelete", player);
           
        }

        // POST: Player/Delete/5
        [HttpPost]
        public ActionResult Delete(string id, Player Player)
        {
                if (string.IsNullOrEmpty(id))
                {
                    return View("Index", LocalPlayer);
                }
                if (Player.id != id)
                {
                    return View("Index", LocalPlayer);
                }
                Player player = LocalPlayer.Where(x => x.id == id).FirstOrDefault();
                if (player != null)
                {
                    LocalPlayer.Remove(player);
                }
    
                  return RedirectToAction("Index");    
             
        }


        public ActionResult CreateTextFile(string data)
        {
            //string route = "/jsonTextFiles/";
            string name = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")+".txt";
            string file = Path.Combine(Server.MapPath("/jsonTextFiles/"), name);
            try
            {
                TextWriter tw = new StreamWriter(file, true);
                if (data.ToLower() == "all")
                {
                    string json = JsonConvert.SerializeObject(LocalPlayer);
                    tw.WriteLine(json);
                }
                if (data.ToLower() == "part")
                {
                    string json = JsonConvert.SerializeObject(JsonPlayer);
                    tw.WriteLine(json);
                }
                tw.Close();              

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            return RedirectToAction("Index");
        }
    }
}
