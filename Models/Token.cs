using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetFinderAPI.Controllers.Api
{
    public class Token
    {
        [JsonProperty("token")]
        public string token { get; set; }


    }
}