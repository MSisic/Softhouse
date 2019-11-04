using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Softhouse.Models
{
    public class Root<T>
    {
        public List<T> data { get; set; }
    }
}