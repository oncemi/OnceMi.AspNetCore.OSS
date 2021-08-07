using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sample.AspNetCore.Mvc.Models
{
    public class ResultObject
    {
        public bool Status { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }
}
