using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Softhouse.Models
{
    public class Player
    {
        public string id { get; set; }

        [Required]
        [Display(Name ="First name")]
        public string first_name { get; set; }
        [Required]
        [Display(Name = "Last name")]

        public string last_name { get; set; }
        [Required]
        [Display(Name = "Position")]

        public string position { get; set; }
        [Display(Name = "Team")]
        public string teamId { get; set; }
        [Display(Name = "Team")]
        public Team team { get; set; }

    }
}